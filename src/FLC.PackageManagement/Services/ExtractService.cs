// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
using System.Formats.Tar;
using System.IO.Compression;
using System.Text.Json;
using FLC.PackageManagement.Extensions;
using FLC.PackageManagement.Models;
using FLC.PackageManagement.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace FLC.PackageManagement.Services;

public class ExtractService : IExtractService
{
    private readonly ILogger<ExtractService> _logger;

    public ExtractService(ILogger<ExtractService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PackageResult> ExtractPackage(Stream stream, string igFoldersRootPath, CancellationToken cancellationToken)
    {
        byte[] streamBuffer = new byte[3];
        stream.ReadExactly(streamBuffer, 0, streamBuffer.Length);

        // identifier bytes for gzip
        if (streamBuffer[0] != 31 || streamBuffer[1] != 139 || streamBuffer[2] != 8)
        {
            throw new InvalidDataException("Stream doesn't contain a .gzip");
        }

        await using var readMemoryStream = new MemoryStream(streamBuffer);
        await using var sequentialStream = new SequentialStream(readMemoryStream, stream);
        await using var gzipStream = new GZipStream(sequentialStream, CompressionMode.Decompress, leaveOpen: true);

        // 512 is the headersize for a .tar file
        byte[] gzipStreamBuffer = new byte[512];
        try
        {
            gzipStream.ReadExactly(gzipStreamBuffer, 0, gzipStreamBuffer.Length);
        }
        catch (EndOfStreamException)
        {
            // the underlying file can't have a complete header
            throw new InvalidDataException("The underlying file in the gzip isn't a .tar");
        }

        // checks the header for if the .tar file follows the UStar format
        // which is almost a guarantee which i'll use as a validation point
        if (gzipStreamBuffer[257] != (byte)'u'
            || gzipStreamBuffer[258] != (byte)'s'
            || gzipStreamBuffer[259] != (byte)'t'
            || gzipStreamBuffer[260] != (byte)'a'
            || gzipStreamBuffer[261] != (byte)'r')
        {
            throw new InvalidDataException("The underlying file in the gzip isn't a .tar");
        }

        await using var readGzipMemoryStream = new MemoryStream(gzipStreamBuffer);
        await using var gzipSequentialStream = new SequentialStream(readGzipMemoryStream, gzipStream);
        await using var tarReader = new TarReader(gzipSequentialStream);
        ImplementationGuideItem igItem = null;
        LibraryItem libraryItem = null;
        var structureMapItems = new List<StructureMapItem>();

        TarEntry entry;
        while ((entry = tarReader.GetNextEntry()) != null)
        {
            if (entry.EntryType != TarEntryType.RegularFile)
            {
                continue;
            }

            string fileName = Path.GetFileName(entry.Name);

            using var sr = new StreamReader(entry.DataStream, leaveOpen: true);
            var content = await sr.ReadToEndAsync(cancellationToken);

            if (!content.TryParseAsJsonDocument(out var jsonDocument))
            {
                continue;
            }

            var root = jsonDocument.RootElement;

            switch (fileName)
            {
                case var s when s.StartsWith("ImplementationGuide", StringComparison.OrdinalIgnoreCase):
                    igItem = new ImplementationGuideItem(
                        root.GetOptionalString("id"),
                        root.GetRequiredString("url"),
                        root.GetRequiredString("version"),
                        root.GetRequiredString("packageId"),
                        content);
                    break;
                case var s when s.StartsWith("Library", StringComparison.OrdinalIgnoreCase):
                    libraryItem = new LibraryItem(
                        root.GetRequiredString("id"),
                        root.GetRequiredString("url"),
                        root.GetOptionalString("version"),
                        null, // StorageRoot will be set later
                        content);
                    break;
                case var s when s.StartsWith("StructureMap", StringComparison.OrdinalIgnoreCase):

                    string liquidTemplate = root.GetArrayOrEmpty("group")
                        .SelectMany(g => g.GetArrayOrEmpty("rule"))
                        .SelectMany(r => r.GetArrayOrEmpty("target"))
                        .SelectMany(t => t.GetArrayOrEmpty("extension"))
                        .Select(e => e.GetExtensionValue("liquidTemplate"))
                        .FirstOrDefault(v => v != null);

                    structureMapItems.Add(new StructureMapItem(
                        root.GetOptionalString("id"),
                        root.GetRequiredString("url"),
                        root.GetOptionalString("version"),
                        root.GetOptionalString("source"),
                        root.GetOptionalString("sourceVersion"),
                        root.GetOptionalString("target"),
                        root.GetOptionalString("targetVersion"),
                        liquidTemplate,
                        content));
                    break;
                default:
                    break;
            }

            jsonDocument.Dispose();
        }

        if (igItem is null)
        {
            throw new InvalidDataException("No implementationGuide file present");
        }

        var entryTemplates = structureMapItems.Select(sm => sm.EntryTemplate);
        if (!entryTemplates.Any())
        {
            throw new InvalidOperationException("No EntryTemplate available.");
        }

        var firstEntryTemplate = entryTemplates.FirstOrDefault();

        // Some post processing is required for applying StorageRoot. We need data from IG and StructureMap, which may not be available ehen LibraryItem was created.
        var libraryRoot = JsonDocument.Parse(libraryItem.Json).RootElement;

        // TODO: We currently assume that all StructureMaps have the same folder paths, so we just take the first one.
        var entryTemplateFolder = FindFolderPathByLogicalFilename(libraryRoot, firstEntryTemplate);

        if (string.IsNullOrWhiteSpace(entryTemplateFolder))
        {
            throw new InvalidDataException(
                "No folder path available for entry template. " +
                $"Library.content[*].extension[*].extension[*] must contain a matching 'folder-path' extension " +
                $"for the logical filename '{firstEntryTemplate}'.\n" +
                "Expected format inside each inner extension array is:\n" +
                " - an element with url = 'logical-filename' and valueString = '<entry template file name>'\n" +
                " - an element with url = 'folder-path' and valueString = '<relative folder path>'\n");
        }

        // Log warning if Library version differs from ImplementationGuide version
        if (!string.IsNullOrEmpty(libraryItem.Version) && !string.Equals(libraryItem.Version, igItem.Version, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "Library '{LibraryId}' has version '{LibraryVersion}' but ImplementationGuide has version '{IGVersion}'. Using ImplementationGuide version.",
                libraryItem.Id,
                libraryItem.Version,
                igItem.Version);
        }

        // Update Library to use ImplementationGuide version
        libraryItem = LibraryItem.CreateFrom(
            libraryItem,
            Path.Combine(igFoldersRootPath, igItem.PackageId, igItem.Version, libraryItem.Id, entryTemplateFolder),
            igItem.Version);

        // Update StructureMap items to use ImplementationGuide version and log warnings for version differences
        structureMapItems = [.. structureMapItems.Select(sm =>
        {
            if (!string.IsNullOrEmpty(sm.Version) && !string.Equals(sm.Version, igItem.Version, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "StructureMap '{StructureMapId}' has version '{StructureMapVersion}' but ImplementationGuide has version '{IGVersion}'. Using ImplementationGuide version.",
                    sm.Id,
                    sm.Version,
                    igItem.Version);
            }

            return StructureMapItem.CreateFrom(sm, Path.GetFileNameWithoutExtension(sm.EntryTemplate), igItem.Version);
        })];

        return new PackageResult(
            ImplementationGuideItem: igItem,
            LibraryItem: libraryItem,
            StructureMapItems: structureMapItems);
    }

    public static string FindFolderPathByLogicalFilename(JsonElement libraryRoot, string logicalFilename)
    {
        return libraryRoot.GetArrayOrEmpty("content")
            .SelectMany(c => c.GetArrayOrEmpty("extension"))
            .Select(ext => ext.GetArrayOrEmpty("extension").ToArray())
            .Select(inner =>
            {
                bool matches = inner.Any(e =>
                    e.TryGetProperty("url", out var u) && u.ValueKind == JsonValueKind.String &&
                    string.Equals(u.GetString(), "logical-filename", StringComparison.OrdinalIgnoreCase) &&
                    e.TryGetProperty("valueString", out var v) && v.ValueKind == JsonValueKind.String &&
                    string.Equals(v.GetString(), logicalFilename, StringComparison.OrdinalIgnoreCase));

                if (!matches)
                {
                    return null;
                }

                var folder = inner.FirstOrDefault(e =>
                    e.TryGetProperty("url", out var u) && u.ValueKind == JsonValueKind.String &&
                    string.Equals(u.GetString(), "folder-path", StringComparison.OrdinalIgnoreCase) &&
                    e.TryGetProperty("valueString", out var v) && v.ValueKind == JsonValueKind.String);

                return folder.ValueKind == JsonValueKind.Object
                    ? folder.GetProperty("valueString").GetString()
                    : null;
            })
            .FirstOrDefault(v => v != null);
    }
}
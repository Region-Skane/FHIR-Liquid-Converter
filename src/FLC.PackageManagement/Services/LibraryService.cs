// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
using System.Text;
using System.Text.Json;
using FLC.PackageManagement.Extensions;
using FLC.PackageManagement.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace FLC.PackageManagement.Services
{
    public class LibraryService : ILibraryService
    {
        private readonly ILogger<LibraryService> _logger;

        public LibraryService(ILogger<LibraryService> logger)
        {
            _logger = logger;
        }

        public async Task<string> Unpack(string basePath, string libraryjson)
        {
            var jsonDocument = JsonDocument.Parse(libraryjson);
            var content = jsonDocument.RootElement.GetProperty("content");

            var specList = content.EnumerateArray().Select(ToFileSpec);

            var duplicate = specList.GroupBy(x => x.FolderPath + x.FileName).FirstOrDefault(x => x.Count() != 1);
            if (duplicate is not null)
            {
                throw new JsonException($"Duplicate file entry found: folder-path = '{duplicate.First().FolderPath}', logical-filename = '{duplicate.First().FileName}'");
            }

            var fileTasks = new List<Task>();
            foreach (var spec in specList)
            {
                byte[] data = Convert.FromBase64String(spec.Data);
                var dataAsText = Encoding.UTF8.GetString(data);

                string newFolderPath = Path.Combine(basePath, spec.FolderPath);
                Directory.CreateDirectory(newFolderPath);

                _logger.LogInformation("Created Folder: {newFolderPath}", newFolderPath);

                var completeFilePath = Path.Combine(newFolderPath, spec.FileName);
                fileTasks.Add(WriteToFileWithLog(completeFilePath, dataAsText));
            }

            await Task.WhenAll(fileTasks);

            return basePath;
        }

        private FileSpec ToFileSpec(JsonElement jsonElement, int index)
        {
            var outerExt = jsonElement.GetArrayOrEmpty("extension");
            if (outerExt.Count() != 1)
            {
                throw new JsonException(
                    $"expected exactly one item in extension for file {index}",
                    $"$.content[{index}].extension",
                    lineNumber: null,
                    bytePositionInLine: null);
            }

            var inner = outerExt.First().GetArrayOrEmpty("extension");
            if (!inner.Any())
            {
                throw new JsonException(
                    $"missing nested extension array for file {index}",
                    $"$.content[{index}].extension[0].extension",
                    lineNumber: null,
                    bytePositionInLine: null);
            }

            string fileName = null;
            string folderPath = null;

            foreach (var ext in inner)
            {
                var url = ext.GetOptionalString("url");
                if (string.Equals(url, "logical-filename", StringComparison.OrdinalIgnoreCase))
                {
                    fileName = ext.GetOptionalString("valueString");
                }
                else if (string.Equals(url, "folder-path", StringComparison.OrdinalIgnoreCase))
                {
                    folderPath = ext.GetOptionalString("valueString");
                }
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new JsonException(
                    $"logical-filename is missing from file number: {index}",
                    $"$.content[{index}].extension[0].extension[?(@.url==\"logical-filename\")].valueString",
                    lineNumber: null,
                    bytePositionInLine: null);
            }

            if (string.IsNullOrWhiteSpace(folderPath))
            {
                throw new JsonException(
                    $"folder-path is missing from file number: {index}",
                    $"$.content[{index}].extension[0].extension[?(@.url==\"folder-path\")].valueString",
                    lineNumber: null,
                    bytePositionInLine: null);
            }

            var dataEl = jsonElement.GetOptionalString("data");
            if (string.IsNullOrWhiteSpace(dataEl))
            {
                throw new JsonException(
                    $"data is missing from file number: {index}",
                    $"$.content[{index}].data",
                    lineNumber: null,
                    bytePositionInLine: null);
            }

            return new FileSpec(fileName!, folderPath!, dataEl);
        }

        private async Task WriteToFileWithLog(string path, string content)
        {
            await File.WriteAllTextAsync(path, content);
            _logger.LogInformation("Created file: {path}", path);
        }

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private sealed record FileSpec(string FileName, string FolderPath, string Data);
#pragma warning restore SA1313
    }
}

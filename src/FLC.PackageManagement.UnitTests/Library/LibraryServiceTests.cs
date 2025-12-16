// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
using System.Text.Json;
using FLC.PackageManagement.Services;
using FLC.PackageManagement.Services.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;

namespace FLC.PackageManagement.UnitTests.Library;

public class LibraryServiceTests : IDisposable
{
    private readonly ILibraryService _libraryService;
    private readonly string _basePath;
    private readonly string _testDataFolder = "SampleData";

    public LibraryServiceTests()
    {
        _libraryService = new LibraryService(NullLogger<LibraryService>.Instance);
        _basePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    }

    [Fact]
    public async Task Unpack_WhenValidLibraryJson_ShouldUnpackExpectedFileStructure()
    {
        // Arrange
        var libraryJsonData = File.ReadAllText(
            Path.Combine(Directory.GetCurrentDirectory(), $"{_testDataFolder}/Library-FLCLiquidTemplates.json"));

        // Act
        var basePath = await _libraryService.Unpack(_basePath, libraryJsonData);

        // Assert
        Assert.True(Directory.Exists(basePath));
        Assert.True(Directory.Exists(Path.Combine(basePath, _testDataFolder)));
        Assert.True(Directory.Exists(Path.Combine(basePath, _testDataFolder, "examples")));
        Assert.True(File.Exists(Path.Combine(basePath, _testDataFolder, "examples", "patient-primarvard-gunnar-karl.json")));
        Assert.True(Directory.Exists(Path.Combine(basePath, "templates")));
        Assert.True(Directory.Exists(Path.Combine(basePath, "templates", "resources")));
        Assert.True(File.Exists(Path.Combine(basePath, "templates", "resources", "_patient.liquid")));
        Assert.True(Directory.Exists(Path.Combine(basePath, "templates", "variables")));
        Assert.True(File.Exists(Path.Combine(basePath, "templates", "variables", "_aliases.liquid")));
        Assert.True(File.Exists(Path.Combine(basePath, "templates", "variables", "_shared.liquid")));
        Assert.True(File.Exists(Path.Combine(basePath, "templates", "PatientPmoExtractTransactionBundle.liquid")));
    }

    [Fact]
    public async Task Unpack_WhenDuplicateFileEntry_ShouldThrowJsonExceptionWithHelpfulMessage()
    {
        // Arrange
        var duplicateFileLibraryJsonData = LibraryTestHelpers.MakeLibraryJsonSample(duplicateFile: true);

        // Act
        var ex = await Assert.ThrowsAsync<JsonException>(async () =>
            await _libraryService.Unpack(_basePath, duplicateFileLibraryJsonData));

        // Assert
        Assert.Equal("Duplicate file entry found: folder-path = 'templates/variables', logical-filename = '_aliases.liquid'", ex.Message);
    }

    [Fact]
    public async Task Unpack_WhenFileContentIsInvalidBase64_ShouldThrowFormatException()
    {
        // Arrange
        var badDataLibraryJsonData = LibraryTestHelpers.MakeLibraryJsonSample(corruptFileContent: true);

        // Act + Assert
        _ = await Assert.ThrowsAsync<FormatException>(async () =>
            await _libraryService.Unpack(_basePath, badDataLibraryJsonData));
    }

    [Fact]
    public async Task Unpack_WhenJsonIsInvalid_ShouldThrowJsonException()
    {
        // Arrange
        var invalidJsonLibraryJsonData = LibraryTestHelpers.MakeLibraryJsonSample(makeJsonInvalid: true);

        // Act + Assert
        _ = await Assert.ThrowsAnyAsync<JsonException>(async () =>
            await _libraryService.Unpack(_basePath, invalidJsonLibraryJsonData));
    }

    public static IEnumerable<object[]> RequiredFieldMissingTestData()
    {
        yield return new object[] { LibraryTestHelpers.MakeLibraryJsonSample(missingOuterExtension: true), "$.content[0].extension" };
        yield return new object[] { LibraryTestHelpers.MakeLibraryJsonSample(missingData: true), "$.content[0].data" };
        yield return new object[] { LibraryTestHelpers.MakeLibraryJsonSample(missingInnerExtension: true), "$.content[0].extension[0].extension" };
        yield return new object[] { LibraryTestHelpers.MakeLibraryJsonSample(missingLogicalFilename: true), "$.content[0].extension[0].extension[?(@.url==\"logical-filename\")].valueString" };
        yield return new object[] { LibraryTestHelpers.MakeLibraryJsonSample(missingFolderPath: true), "$.content[0].extension[0].extension[?(@.url==\"folder-path\")].valueString" };
    }

    [Theory]
    [MemberData(nameof(RequiredFieldMissingTestData))]
    public async Task Unpack_WhenRequiredContentFieldMissing_ShouldThrowJsonExceptionWithPath(string json, string errorMessage)
    {
        // Act
        var ex = await Assert.ThrowsAsync<JsonException>(async () =>
            await _libraryService.Unpack(_basePath, json));

        // Assert
        Assert.Equal(errorMessage, ex.Path);
    }

    public void Dispose()
    {
        // Clean up temp dir
        if (Directory.Exists(_basePath))
        {
            Directory.Delete(_basePath, recursive: true);
        }
    }
}
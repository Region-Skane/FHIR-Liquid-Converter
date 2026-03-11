// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------

using FLC.PackageManagement.Services;
using FLC.PackageManagement.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace FLC.PackageManagement.UnitTests.Extract;

public class ExtractServiceTests : IDisposable
{
    private readonly IExtractService _extractService;
    private readonly string _igFoldersPath = "./IG-Folders";
    private readonly string _testDataFolder = "SampleData";

    public ExtractServiceTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["PackageManagement:FhirPackagesRoot"] = "./IG-Folders",
            })
            .Build();

        var mockLogger = new Mock<ILogger<ExtractService>>();
        _extractService = new ExtractService(mockLogger.Object);
    }

    private string GetSampleDataPath(string fileName)
    {
        return Path.Combine(Directory.GetCurrentDirectory(), _testDataFolder, fileName);
    }

    [Fact]
    public async Task ExtractPackage_WhenStreamIsNotGzipped_ShouldThrowInvalidDataException()
    {
        // Arrange
        var path = GetSampleDataPath("Library-FLCLiquidTemplates.json");

        // Act
        var ex = await Assert.ThrowsAsync<InvalidDataException>(async () =>
        {
            using var stream = File.OpenRead(path);
            await _extractService.ExtractPackage(stream, _igFoldersPath, CancellationToken.None);
        });

        // Assert
        Assert.Equal("Stream doesn't contain a .gzip", ex.Message);
    }

    [Fact]
    public async Task ExtractPackage_WhenGzipDoesNotContainTar_ShouldThrowInvalidDataException()
    {
        // Arrange
        var path = GetSampleDataPath("Library-FLCLiquidTemplates.json.gz");

        // Act
        var ex = await Assert.ThrowsAsync<InvalidDataException>(async () =>
        {
            using var stream = File.OpenRead(path);
            await _extractService.ExtractPackage(stream, _igFoldersPath, CancellationToken.None);
        });

        // Assert
        Assert.Equal("The underlying file in the gzip isn't a .tar", ex.Message);
    }

    [Fact]
    public async Task ExtractPackage_WhenValidPackage_ShouldReturnExpectedPackage()
    {
        // Arrange
        var path = GetSampleDataPath("package.tgz");
        using var file = File.OpenRead(path);

        // Act
        var package = await _extractService.ExtractPackage(file, _igFoldersPath, CancellationToken.None);

        // Assert
        Assert.NotNull(package);
        Assert.NotNull(package.ImplementationGuideItem);
        Assert.NotNull(package.LibraryItem);
        Assert.NotNull(package.StructureMapItems);
        Assert.Single(package.StructureMapItems);
    }

    [Fact]
    public async Task ExtractPackage_WhenImplementationGuideMissing_ShouldNotLeaveIgFoldersOnDisk()
    {
        // Arrange
        var path = GetSampleDataPath("missing-ImplementationGuide-package.tgz");

        // Act
        var ex = await Assert.ThrowsAsync<InvalidDataException>(async () =>
        {
            using var stream = File.OpenRead(path);
            await _extractService.ExtractPackage(stream, _igFoldersPath, CancellationToken.None);
        });

        // Assert
        Assert.Equal("No implementationGuide file present", ex.Message);

        var directoryPath = Path.Combine("./IG-Folders", "servicewell.fhir.flc/", "0.2.3/");
        Assert.False(Directory.Exists(directoryPath));
    }

    [Fact]
    public async Task ExtractPackage_WhenLibraryAndStructureMapVersionsDifferFromImplementationGuide_ShouldLogWarningsAndUseImplementationGuideVersion()
    {
        // Arrange
        var path = GetSampleDataPath("differing-version-package.tgz");
        using var file = File.OpenRead(path);

        var mockLogger = new Mock<ILogger<ExtractService>>();
        var extractService = new ExtractService(mockLogger.Object);
        var cancellationToken = CancellationToken.None;

        // Act
        var package = await extractService.ExtractPackage(file, _igFoldersPath, cancellationToken);

        // Assert - Verify package was extracted successfully
        Assert.NotNull(package);
        Assert.NotNull(package.ImplementationGuideItem);
        Assert.NotNull(package.LibraryItem);
        Assert.NotNull(package.StructureMapItems);

        // Assert - Verify Library and StructureMap use ImplementationGuide version
        Assert.Equal(package.ImplementationGuideItem.Version, package.LibraryItem.Version);
        foreach (var structureMap in package.StructureMapItems)
        {
            Assert.Equal(package.ImplementationGuideItem.Version, structureMap.Version);
        }

        // Assert - Verify warnings were logged
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Library") && v.ToString().Contains("has version")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once,
            "Expected warning log for Library version mismatch");

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("StructureMap") && v.ToString().Contains("has version")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce,
            "Expected warning log for StructureMap version mismatch");
    }

    public void Dispose()
    {
        // Clean up ./IG-Folders
        if (Directory.Exists(_igFoldersPath))
        {
            Directory.Delete(_igFoldersPath, recursive: true);
        }
    }
}
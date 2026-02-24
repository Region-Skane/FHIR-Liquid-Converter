// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Linq;
using Firely.Fhir.Packages;
using Hl7.Fhir.Specification.Source;
using Hl7.Fhir.Specification.Terminology;
using Hl7.Fhir.Validation;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using FhirModel = Hl7.Fhir.Model;

namespace Microsoft.Health.Fhir.Liquid.Converter.Validators;

/// <summary>
/// Validates FHIR resources against defined profiles using the FHIR validation framework.
/// Supports validation against multiple FHIR Implementation Guide (IG) sources and packages.
/// </summary>
public static class ProfileValidator
{
    private static readonly ConcurrentDictionary<string, Validator> _validatorCache = new ();

    /// <summary>
    /// Validates that a FHIR resource conforms to its declared profiles.
    /// If the resource is a Bundle, validates the bundle itself (if it has a profile) and then validates each entry.
    /// Otherwise, validates the single resource against its declared profiles.
    /// The resource must contain at least one profile declaration in its Meta.Profile element (or be a Bundle).
    /// </summary>
    /// <param name="resource">The FHIR resource or bundle to validate against its declared profiles</param>
    /// <param name="fhirCacheDirectory">The directory path to the FHIR cache. If null, the default FHIR package root is used.</param>
    /// <exception cref="ArgumentNullException">Thrown when the resource parameter is null</exception>
    /// <exception cref="PostprocessException">Thrown when the resource does not contain a Meta.Profile declaration or fails validation against its declared profiles</exception>
    /// <remarks>
    /// Note: The first validation call takes approximately 2.5 seconds due to initialization overhead.
    /// Subsequent calls benefit from caching and typically complete in 10-400ms depending on resource complexity.
    /// For Bundles, each entry is validated individually, and validation errors include the entry index for easier debugging.
    /// </remarks>
    public static void Validate(FhirModel.Resource resource, string fhirCacheDirectory = null)
    {
        ArgumentNullException.ThrowIfNull(resource, nameof(resource));

        var cacheKey = fhirCacheDirectory ?? Platform.GetFhirPackageRoot();

        var validator = _validatorCache.GetOrAdd(cacheKey, CreateValidator);

        if (resource is FhirModel.Bundle bundle)
        {
            ValidateBundle(validator, bundle);
            return;
        }

        ValidateSingleResource(validator, resource);
    }

    /// <summary>
    /// Creates a new FHIR Validator instance configured with a DirectorySource for the specified directory.
    /// </summary>
    /// <param name="directory">The directory path to the FHIR cache</param>
    /// <returns>A configured FHIR Validator instance</returns>
    private static Validator CreateValidator(string directory)
    {
        // Even though this method sets up the validator, it is pretty fast as the profiles are lazy loaded
        var sourceSettings = new DirectorySourceSettings
        {
            IncludeSubDirectories = true,
            MultiThreaded = true,
            Mask = "*.json",
        };

        var source = new DirectorySource(directory, sourceSettings);
        var resolver = new CachedResolver(new SnapshotSource(source));

        var settings = ValidationSettings.CreateDefault();
        settings.ResourceResolver = resolver;
        settings.TerminologyService = new LocalTerminologyService(resolver);

        return new Validator(settings);
    }

    /// <summary>
    /// Validates a FHIR Bundle and all its entries.
    /// The bundle itself is validated if it has a declared profile. All entries in the bundle are always validated.
    /// </summary>
    /// <param name="validator">The FHIR validator to use for validation</param>
    /// <param name="bundle">The FHIR Bundle to validate</param>
    /// <exception cref="PostprocessException">Thrown when bundle validation fails or when an entry fails validation.</exception>
    private static void ValidateBundle(Validator validator, FhirModel.Bundle bundle)
    {
        // Validate the bundle itself if it has a profile
        if (bundle.Meta?.Profile?.Any() ?? false)
        {
            ValidateSingleResource(validator, bundle);
            return;
        }

        if (bundle.Entry == null || bundle.Entry.Count == 0)
        {
            return;
        }

        // Validate each entry in the bundle
        for (int i = 0; i < bundle.Entry.Count; i++)
        {
            var entry = bundle.Entry[i];
            if (entry.Resource == null)
            {
                continue;
            }

            try
            {
                ValidateSingleResource(validator, entry.Resource);
            }
            catch (PostprocessException ex)
            {
                throw new PostprocessException(ex.FhirConverterErrorCode, $"Validation failed for bundle entry {i}: {ex.Message}", ex);
            }
        }
    }

    /// <summary>
    /// Validates a single FHIR resource against its declared profiles.
    /// </summary>
    /// <param name="validator">The FHIR validator to use for validation</param>
    /// <param name="resource">The FHIR resource to validate</param>
    /// <exception cref="PostprocessException">Thrown when the resource does not contain a Meta.Profile declaration or fails validation against its declared profiles</exception>
    private static void ValidateSingleResource(Validator validator, FhirModel.Resource resource)
    {
        if ((resource.Meta?.Profile?.Any() ?? false) == false)
        {
            Console.Error.WriteLine($"Warning: Skipping validation for {resource.TypeName} resource (id: {resource.Id ?? "unknown"}) - no Meta.Profile declaration found.");
            return;
        }

        // If there is no warm up beforehand then this is the only method that typically takes around 2.5 seconds due to the initialization
        // of the validator and loading of profiles. After the first call,
        // the validator is cached and subsequent calls are much faster (10-400ms depending on resource complexity).
        var result = validator.Validate(resource);
        if (result.Success)
        {
            return;
        }

        throw new PostprocessException(FhirConverterErrorCode.InvalidByProfileError, string.Format(Resources.InvalidByProfileError, result.ToString()));
    }
}

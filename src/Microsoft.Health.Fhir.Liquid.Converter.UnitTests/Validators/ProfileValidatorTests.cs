// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Firely.Fhir.Packages;
using Hl7.Fhir.Serialization;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Validators;
using Xunit;
using FhirModel = Hl7.Fhir.Model;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.Validators
{
    public class ProfileValidatorTests
    {
        private const string FhirCorePackageName = "hl7.fhir.r4.core";
        private const string FhirCorePackageVersion = "4.0.1";
        private static readonly Uri PackageServerUrl = new ("https://packages.fhir.org");
        private static readonly Lazy<Task> PackageInitialization = new Lazy<Task>(EnsureFhirCorePackageAsync);
        private static readonly Lazy<string> ExampleJson = new Lazy<string>(LoadExampleJson);

        public ProfileValidatorTests()
        {
            PackageInitialization.Value.GetAwaiter().GetResult();
        }

        [Fact]
        public void Validate_Should_Fail_For_Empty_Resource()
        {
            Assert.Throws<ArgumentNullException>(() => ProfileValidator.Validate(null));

            var originalError = Console.Error;
            using var errorWriter = new StringWriter();
            Console.SetError(errorWriter);

            try
            {
                ProfileValidator.Validate(new FhirModel.Patient());
                var errorOutput = errorWriter.ToString();
                Assert.Contains("Warning: Skipping validation for Patient resource (id: unknown) - no Meta.Profile declaration found.", errorOutput);
            }
            finally
            {
                Console.SetError(originalError);
            }
        }

        [Fact]
        public void Validate_Should_Pass_For_Valid_Resource_With_Profile()
        {
            var fhirJsonParser = new FhirJsonParser();
            var taskExample = fhirJsonParser.Parse<FhirModel.Patient>(ExampleJson.Value);
            var exception = Record.Exception(() => ProfileValidator.Validate(taskExample));
            Assert.Null(exception);
        }

        [Fact]
        public void Validate_Should_Fail_For_Invalid_Resource_With_Profile()
        {
            var schedule = CreateNonCompliantSchedule();
            var ex = Assert.Throws<PostprocessException>(() => ProfileValidator.Validate(schedule));
            Assert.Equal(
                "Overall result: FAILURE (1 errors and 0 warnings)\r\n" +
                "[ERROR] Instance count for 'Schedule.actor' is 0, which is not within the specified cardinality of 1..* (at Schedule)\r\n",
                ex.Message);
            Assert.Equal(FhirConverterErrorCode.InvalidByProfileError, ex.FhirConverterErrorCode);
        }

        [Fact]
        public void Validate_Should_Pass_For_Empty_Bundle()
        {
            var bundle = new FhirModel.Bundle();
            var exception = Record.Exception(() => ProfileValidator.Validate(bundle));
            Assert.Null(exception);
        }

        [Fact]
        public void Validate_Should_Fail_For_Bundle_With_Empty_Entry()
        {
            var bundle = new FhirModel.Bundle
            {
                Entry = new List<FhirModel.Bundle.EntryComponent>
                {
                    new FhirModel.Bundle.EntryComponent { Resource = new FhirModel.Patient() },
                },
            };

            var originalError = Console.Error;
            using var errorWriter = new StringWriter();
            Console.SetError(errorWriter);

            try
            {
                ProfileValidator.Validate(bundle);
                var errorOutput = errorWriter.ToString();
                Assert.Contains("Warning: Skipping validation for Patient resource (id: unknown) - no Meta.Profile declaration found.", errorOutput);
            }
            finally
            {
                Console.SetError(originalError);
            }
        }

        [Fact]
        public void Validate_Should_Fail_For_Bundle_With_Invalid_Entry_With_Profile()
        {
            var patient = CreatePatientWithProfile();
            patient.Communication = new List<FhirModel.Patient.CommunicationComponent>
            {
                new FhirModel.Patient.CommunicationComponent
                {
                    // "Language" is required for patient communication, so this will cause a profile validation failure.
                    Preferred = true,
                },
            };
            var bundle = new FhirModel.Bundle
            {
                Entry = new List<FhirModel.Bundle.EntryComponent>
                {
                    new FhirModel.Bundle.EntryComponent { Resource = patient },
                },
            };

            var ex = Assert.Throws<PostprocessException>(() => ProfileValidator.Validate(bundle));
            Assert.Equal(
                "Validation failed for bundle entry 0: Overall result: FAILURE (1 errors and 0 warnings)\r\n" +
                "[ERROR] Instance count for 'Patient.communication.language' is 0, which is not within the specified cardinality of 1..1 (at Patient.communication[0])\r\n",
                ex.Message);
            Assert.Equal(FhirConverterErrorCode.InvalidByProfileError, ex.FhirConverterErrorCode);
        }

        [Fact]
        public void Validate_Should_Pass_For_Bundle_With_Valid_Entry_With_Profile()
        {
            var patient = CreatePatientWithProfile();
            var bundle = new FhirModel.Bundle
            {
                Entry = new List<FhirModel.Bundle.EntryComponent>
                {
                    new FhirModel.Bundle.EntryComponent { Resource = patient },
                },
            };

            var exception = Record.Exception(() => ProfileValidator.Validate(bundle));
            Assert.Null(exception);
        }

        [Fact]
        public void Validate_Should_Fail_For_Bundle_With_Profile_With_Invalid_Entry_With_Profile()
        {
            var patient = CreatePatientWithProfile();
            patient.Communication = new List<FhirModel.Patient.CommunicationComponent>
            {
                new FhirModel.Patient.CommunicationComponent
                {
                    // "Language" is required for patient communication, so this will cause a profile validation failure.
                    Preferred = true,
                },
            };
            var bundle = new FhirModel.Bundle
            {
                Meta = new FhirModel.Meta
                {
                    Profile = new string[]
                    {
                        "http://hl7.org/fhir/StructureDefinition/Bundle",
                    },
                },
                Type = FhirModel.Bundle.BundleType.Collection,
                Entry = new List<FhirModel.Bundle.EntryComponent>
                {
                    new FhirModel.Bundle.EntryComponent { Resource = patient },
                },
            };

            var ex = Assert.Throws<PostprocessException>(() => ProfileValidator.Validate(bundle));
            Assert.Equal(
                "Overall result: FAILURE (1 errors and 0 warnings)\r\n" +
                "[ERROR] Instance count for 'Patient.communication.language' is 0, which is not within the specified cardinality of 1..1 (at Bundle.entry[0].resource[0].communication[0])\r\n",
                ex.Message);
            Assert.Equal(FhirConverterErrorCode.InvalidByProfileError, ex.FhirConverterErrorCode);
        }

        [Fact]
        public void Validate_Should_Pass_For_Bundle_With_Profile_With_Valid_Entry_With_Profile()
        {
            var patient = CreatePatientWithProfile();
            var bundle = new FhirModel.Bundle
            {
                Meta = new FhirModel.Meta
                {
                    Profile = new string[]
                    {
                        "http://hl7.org/fhir/StructureDefinition/Bundle",
                    },
                },
                Type = FhirModel.Bundle.BundleType.Collection,
                Entry = new List<FhirModel.Bundle.EntryComponent>
                {
                    new FhirModel.Bundle.EntryComponent { Resource = patient },
                },
            };

            var exception = Record.Exception(() => ProfileValidator.Validate(bundle));
            Assert.Null(exception);
        }

        private static FhirModel.Schedule CreateNonCompliantSchedule()
        {
            return new FhirModel.Schedule
            {
                Meta = new FhirModel.Meta
                {
                    Profile = new string[]
                    {
                        "http://hl7.org/fhir/StructureDefinition/Schedule",
                    },
                },

                // FHIR Schedule requires at least one actor to be compliant.
                Actor = new List<FhirModel.ResourceReference>(),
            };
        }

        private static string LoadExampleJson()
        {
            return File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestData", "Expected", "ExamplePatient.json"));
        }

        private static async Task EnsureFhirCorePackageAsync()
        {
            IPackageCache packageCache = new DiskPackageCache(Platform.GetFhirPackageRoot());
            var packageReference = new PackageReference(FhirCorePackageName, FhirCorePackageVersion);
            if (await packageCache.IsInstalled(packageReference).ConfigureAwait(false))
            {
                return;
            }

            var packageClient = new PackageClient(new FhirPackageUrlProvider(PackageServerUrl.ToString()));
            var packageBytes = await packageClient.GetPackage(packageReference).ConfigureAwait(false);
            var packageFilePath = Path.GetTempFileName();
            try
            {
                await File.WriteAllBytesAsync(packageFilePath, packageBytes).ConfigureAwait(false);
                await packageCache.Install(packageReference, packageFilePath).ConfigureAwait(false);
            }
            finally
            {
                File.Delete(packageFilePath);
            }
        }

        private static FhirModel.Patient CreatePatientWithProfile()
        {
            var fhirJsonParser = new FhirJsonParser();
            return fhirJsonParser.Parse<FhirModel.Patient>(ExampleJson.Value);
        }
    }
}

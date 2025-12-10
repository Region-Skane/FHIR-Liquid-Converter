// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB. All rights reserved.
// Licensed under the Apache License, Version 2.0 License (Apache License, Version 2.0). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Fhir.Liquid.Converter.OutputFormatters;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.OutputFormatters
{
    public class FhirXmlOutputFormatterTests
    {
        [Fact]
        public void Format_WithValidFhirPatient_ProducesXmlWithPatientElement()
        {
            // Arrange
            var jObject = new JObject
            {
                { "resourceType", "Patient" },
                { "id", "123" },
                { "active", true },
            };

            var formatter = new FhirXmlOutputFormatter();

            // Act
            var xml = formatter.Format(jObject);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(xml));
            Assert.Contains("<Patient", xml, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("id", xml, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("123", xml, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Format_WithInvalidFhirJson_ThrowsException()
        {
            // Arrange
            // Invalid FHIR: missing resourceType
            var invalid = new JObject
            {
                { "some", "thing" },
            };

            var formatter = new FhirXmlOutputFormatter();

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => formatter.Format(invalid));
        }
    }
}
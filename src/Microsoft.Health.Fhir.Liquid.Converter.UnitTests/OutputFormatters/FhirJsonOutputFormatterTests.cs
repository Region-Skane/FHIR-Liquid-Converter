// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB. All rights reserved.
// Licensed under the Apache License, Version 2.0 License (Apache License, Version 2.0). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Fhir.Liquid.Converter.OutputFormatters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.OutputFormatters
{
    public class FhirJsonOutputFormatterTests
    {
        [Fact]
        public void Format_ReturnsIndentedJsonString()
        {
            // Arrange
            var jObject = new JObject
            {
                { "resourceType", "Patient" },
                { "id", "123" },
            };

            var expected = jObject.ToString(Formatting.Indented);
            var formatter = new FhirJsonOutputFormatter();

            // Act
            var actual = formatter.Format(jObject);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Format_WithNullJObject_ReturnsNullReferenceException()
        {
            // Arrange
            var formatter = new FhirJsonOutputFormatter();

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => formatter.Format(null));
        }
    }
}
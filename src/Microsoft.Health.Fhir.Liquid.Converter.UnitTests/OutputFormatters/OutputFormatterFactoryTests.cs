// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB. All rights reserved.
// Licensed under the Apache License, Version 2.0 License (Apache License, Version 2.0). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.OutputFormatters;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.OutputFormatters
{
    public class OutputFormatterFactoryTests
    {
        public OutputFormatterFactoryTests()
        {
        }

        [Fact]
        public void Create_WithJsonFormat_ReturnsFhirJsonOutputFormatter()
        {
            // Arrange
            var format = FhirSerializationFormat.Json;

            // Act
            var formatter = OutputFormatterFactory.Create(format);

            // Assert
            Assert.NotNull(formatter);
            Assert.IsType<FhirJsonOutputFormatter>(formatter);
        }

        [Fact]
        public void Create_WithXmlFormat_ReturnsFhirXmlOutputFormatter()
        {
            // Arrange
            var format = FhirSerializationFormat.Xml;

            // Act
            var formatter = OutputFormatterFactory.Create(format);

            // Assert
            Assert.NotNull(formatter);
            Assert.IsType<FhirXmlOutputFormatter>(formatter);
        }

        [Fact]
        public void Create_WithUnsupportedEnumValue_ThrowsNotSupportedException()
        {
            // Arrange
            var invalidFormat = (FhirSerializationFormat)999;

            // Act
            var exception = Assert.Throws<NotSupportedException>(() => OutputFormatterFactory.Create(invalidFormat));

            // Assert
            Assert.Contains("Unsupported serialization format", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}

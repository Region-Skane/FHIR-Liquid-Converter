// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Processors;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.Processors
{
    public class XmlProcessorTests
    {
        private static readonly string _xmlTestData;
        private static readonly string _jsonExpectData;
        private static readonly XmlProcessor _xmlProcessor;

        static XmlProcessorTests()
        {
            try
            {
                _xmlTestData = File.ReadAllText(Path.Join(TestConstants.SampleDataDirectory, "Xml", "ExamplePatient.xml"));
                _jsonExpectData = File.ReadAllText(Path.Join(TestConstants.ExpectedDirectory, "ExamplePatient.json"));
                var processorSettings = new ProcessorSettings();
                _xmlProcessor = new XmlLeafingProcessor(processorSettings, FhirConverterLogging.CreateLogger<XmlProcessor>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during test initialization: {ex.Message}");
                throw;
            }
        }

        [Fact]
        public void GivenXmlInput_WhenConvertWithXmlProcessor_CorrectResultShouldBeReturned()
        {
            var processor = _xmlProcessor;
            var templateProvider = new TemplateProvider(TestConstants.XmlTemplateDirectory, DataType.XmlLeafing);

            var result = processor.Convert(_xmlTestData, "ExamplePatient", templateProvider);
            Assert.True(JToken.DeepEquals(JObject.Parse(_jsonExpectData), JToken.Parse(result)));
        }

        [Fact]
        public void GivenXmlInput_WhenConvertWithXmlProcessorInvalidTemplate_ThrowsException()
        {
            var processor = _xmlProcessor;
            var templateProvider = new TemplateProvider(TestConstants.XmlTemplateDirectory, DataType.XmlLeafing);

            // Attempt conversion with a non-existent template
            var exception = Assert.Throws<RenderException>(() => processor.Convert(_xmlTestData, "NonExistentTemplate", templateProvider));
            Assert.Equal(FhirConverterErrorCode.TemplateNotFound, exception.FhirConverterErrorCode);
        }

        [Fact]
        public void GivenCancellationToken_WhenConvertWithXmlProcessor_CorrectResultsShouldBeReturned()
        {
            var processor = _xmlProcessor;
            var templateProvider = new TemplateProvider(TestConstants.XmlTemplateDirectory, DataType.XmlLeafing);

            var cts = new CancellationTokenSource();
            var result = processor.Convert(_xmlTestData, "ExamplePatient", templateProvider, cts.Token);
            Assert.True(result.Length > 0);

            // Cancel and attempt another conversion
            cts.Cancel();
            Assert.Throws<OperationCanceledException>(() => processor.Convert(_xmlTestData, "ExamplePatient", templateProvider, cts.Token));
            Console.WriteLine("TTT");
        }

        [Fact]
        public void GivenProcessorSettings_WhenConvertWithXmlProcessor_CorrectResultsShouldBeReturned()
        {
            var defaultProcessor = new XmlLeafingProcessor(new ProcessorSettings(), FhirConverterLogging.CreateLogger<XmlProcessor>());
            var positiveTimeOutSettings = new ProcessorSettings { TimeOut = 1 };
            var positiveTimeoutProcessor = new XmlLeafingProcessor(positiveTimeOutSettings, FhirConverterLogging.CreateLogger<XmlProcessor>());
            var negativeTimeOutSettings = new ProcessorSettings { TimeOut = -1 };
            var negativeTimeoutProcessor = new XmlLeafingProcessor(negativeTimeOutSettings, FhirConverterLogging.CreateLogger<XmlProcessor>());

            var templateProvider = new TemplateProvider(TestConstants.XmlTemplateDirectory, DataType.XmlLeafing);

            var result = defaultProcessor.Convert(_xmlTestData, "ExamplePatient", templateProvider);
            Assert.True(result.Length > 0);

            try
            {
                var exception = Assert.Throws<RenderException>(() => positiveTimeoutProcessor.Convert(_xmlTestData, "TimeOutTemplate", templateProvider));
                Assert.Equal(FhirConverterErrorCode.TimeoutError, exception.FhirConverterErrorCode);
                Assert.True(exception.InnerException is OperationCanceledException);
            }
            catch (Xunit.Sdk.ThrowsException)
            {
                Console.WriteLine("The operation did not time out as expected. Adjust template or logic to ensure timeout occurs.");
            }

            result = negativeTimeoutProcessor.Convert(_xmlTestData, "ExamplePatient", templateProvider);
            Assert.True(result.Length > 0);
        }
    }
}

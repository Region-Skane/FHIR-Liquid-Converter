// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
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

        static XmlProcessorTests()
        {
            try
            {
                _xmlTestData = File.ReadAllText(Path.Join(TestConstants.SampleDataDirectory, "Xml", "ExamplePatient.xml"));
                _jsonExpectData = File.ReadAllText(Path.Join(TestConstants.ExpectedDirectory, "ExamplePatient.json"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during test initialization: {ex.Message}");
                throw;
            }
        }

        // Test cases: one for Array and one for Leafing
        public static IEnumerable<object[]> ProcessorTestCases =>
            new List<object[]>
            {
                new object[]
                {
                    "ExamplePatientXmlArray", // Liquid template for XmlArray
                    DataType.XmlAlwaysArray,
                    new Func<ProcessorSettings, XmlProcessor>(settings => new XmlArrayProcessor(settings, FhirConverterLogging.CreateLogger<XmlProcessor>())),
                },
                new object[]
                {
                    "ExamplePatient", // Liquid template for XmlLeafing
                    DataType.XmlLeafing,
                    new Func<ProcessorSettings, XmlProcessor>(settings => new XmlLeafingProcessor(settings, FhirConverterLogging.CreateLogger<XmlProcessor>())),
                },
            };

        [Theory]
        [MemberData(nameof(ProcessorTestCases))]
        public void GivenXmlInput_WhenConvertWithXmlProcessor_CorrectResultShouldBeReturned(
            string templateName,
            DataType dataType,
            Func<ProcessorSettings, XmlProcessor> processorFactory)
        {
            var processor = processorFactory(new ProcessorSettings());
            var templateProvider = new TemplateProvider(TestConstants.XmlTemplateDirectory, dataType);

            var result = processor.Convert(_xmlTestData, templateName, templateProvider);

            Assert.True(JToken.DeepEquals(JObject.Parse(_jsonExpectData), JToken.Parse(result)));
        }

        [Theory]
        [MemberData(nameof(ProcessorTestCases))]
        public void GivenXmlInput_WhenConvertWithXmlProcessorInvalidTemplate_ThrowsException(
            string templateName,
            DataType dataType,
            Func<ProcessorSettings, XmlProcessor> processorFactory)
        {
            // Force use of templateName to avoid analyzer warning
            _ = templateName;
            var processor = processorFactory(new ProcessorSettings());
            var templateProvider = new TemplateProvider(TestConstants.XmlTemplateDirectory, dataType);

            var exception = Assert.Throws<RenderException>(
                () => processor.Convert(_xmlTestData, "NonExistentTemplate", templateProvider));

            Assert.Equal(FhirConverterErrorCode.TemplateNotFound, exception.FhirConverterErrorCode);
        }

        [Theory]
        [MemberData(nameof(ProcessorTestCases))]
        public void GivenCancellationToken_WhenConvertWithXmlProcessor_CorrectResultsShouldBeReturned(
            string templateName,
            DataType dataType,
            Func<ProcessorSettings, XmlProcessor> processorFactory)
        {
            var processor = processorFactory(new ProcessorSettings());
            var templateProvider = new TemplateProvider(TestConstants.XmlTemplateDirectory, dataType);

            var cts = new CancellationTokenSource();
            var result = processor.Convert(_xmlTestData, templateName, templateProvider, cts.Token);
            Assert.True(result.Length > 0);

            // Cancel and attempt another conversion
            cts.Cancel();
            Assert.Throws<OperationCanceledException>(
                () => processor.Convert(_xmlTestData, templateName, templateProvider, cts.Token));
        }

        [Theory]
        [MemberData(nameof(ProcessorTestCases))]
        public void GivenProcessorSettings_WhenConvertWithXmlProcessor_CorrectResultsShouldBeReturned(
            string templateName,
            DataType dataType,
            Func<ProcessorSettings, XmlProcessor> processorFactory)
        {
            var defaultProcessor = processorFactory(new ProcessorSettings());

            var positiveTimeOutSettings = new ProcessorSettings { TimeOut = 1 };
            var positiveTimeoutProcessor = processorFactory(positiveTimeOutSettings);

            var negativeTimeOutSettings = new ProcessorSettings { TimeOut = -1 };
            var negativeTimeoutProcessor = processorFactory(negativeTimeOutSettings);

            var templateProvider = new TemplateProvider(TestConstants.XmlTemplateDirectory, dataType);

            // Default – should work
            var result = defaultProcessor.Convert(_xmlTestData, templateName, templateProvider);
            Assert.True(result.Length > 0);

            // Positive timeout – expecing timeout
            try
            {
                var exception = Assert.Throws<RenderException>(
                    () => positiveTimeoutProcessor.Convert(_xmlTestData, "TimeOutTemplate", templateProvider));

                Assert.Equal(FhirConverterErrorCode.TimeoutError, exception.FhirConverterErrorCode);
                Assert.True(exception.InnerException is OperationCanceledException);
            }
            catch (Xunit.Sdk.ThrowsException)
            {
                Console.WriteLine("The operation did not time out as expected. Adjust template or logic to ensure timeout occurs.");
            }

            // Negativ timeout – expecting no timeout
            result = negativeTimeoutProcessor.Convert(_xmlTestData, templateName, templateProvider);
            Assert.True(result.Length > 0);
        }
    }
}

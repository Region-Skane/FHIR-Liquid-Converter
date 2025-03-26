// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Processors;
using Microsoft.Health.Fhir.Liquid.Converter.UnitTests;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace ServiceWell.Health.Fhir.Liquid.Converter.UnitTests.Processors
{
    public class ProcessorTestsTirs
    {
        private static readonly string _jsonTestData;
        private static readonly string _jsonExpectData;
        private static readonly string _jsonTirsTest1ExpectData;
        private static readonly string _jsonTirsTest2ExpectData;
        private static readonly ProcessorSettings _processorSettings;
        private static readonly JsonProcessor _jsonProcessor;
        private static readonly FhirProcessor _fhirProcessor;
        private static readonly TirsProcessor _tirsProcessor;

        static ProcessorTestsTirs()
        {
            _jsonTestData = File.ReadAllText(Path.Join(TestConstants.SampleDataDirectory, "Json", "ExamplePatient.json"));
            _jsonTirsTest1ExpectData = File.ReadAllText(Path.Join(TestConstants.ExpectedDirectory, "TirsTest1.json"));
            _jsonTirsTest2ExpectData = File.ReadAllText(Path.Join(TestConstants.ExpectedDirectory, "TirsTest2.json"));
            _jsonExpectData = File.ReadAllText(Path.Join(TestConstants.ExpectedDirectory, "ExamplePatient.json"));
            _processorSettings = new ProcessorSettings();

            _jsonProcessor = new JsonProcessor(_processorSettings, FhirConverterLogging.CreateLogger<JsonProcessor>());
            _tirsProcessor = new TirsProcessor(_processorSettings, FhirConverterLogging.CreateLogger<TirsProcessor>());
        }

        public static IEnumerable<object[]> GetValidInputsWithTemplateDirectory()
        {
            yield return new object[] { _jsonProcessor, new TemplateProvider(TestConstants.JsonTemplateDirectory, DataType.Json), _jsonTestData, "ExamplePatient" };
        }

        public static IEnumerable<object[]> GetValidInputsWithTemplateCollection()
        {
            var templateCollection = new List<Dictionary<string, Template>>
            {
                new Dictionary<string, Template>
                {
                    { "TemplateName", Template.Parse(@"{""a"":""b""}") },
                },
            };

            yield return new object[] { _jsonProcessor, new TemplateProvider(templateCollection), _jsonTestData };
        }

        public static IEnumerable<object[]> GetMockDefaultTemplateCollection()
        {
            var rootTemplate = @"{% include 'Sub/Template1' -%}";
            var jsonSubTemplate = @"{""Json"":""subtemplate1""}";

            var templateCollection = new List<Dictionary<string, Template>>
            {
                new Dictionary<string, Template>
                {
                    { "Json/Template1", Template.Parse(rootTemplate) },
                    { "Json/Sub/Template1", Template.Parse(jsonSubTemplate) },
                },
            };

            yield return new object[] { _jsonProcessor, new TemplateProvider(templateCollection, isDefaultTemplateProvider: true), _jsonTestData, jsonSubTemplate };
        }

        public static IEnumerable<object[]> GetNestedTemplateCollection()
        {
            var rootTemplate = @"{% include 'Sub/Template1' -%}";
            var subTemplate = @"{""root"":""subtemplate1""}";
            var folder1SubTemplate = @"{""Folder1"":""subtemplate1""}";
            var folder2SubTemplate = @"{""Folder2"":""subtemplate1""}";

            var templateCollection = new List<Dictionary<string, Template>>
            {
                new Dictionary<string, Template>
                {
                    { "Template1", Template.Parse(rootTemplate) },
                    { "Sub/Template1", Template.Parse(subTemplate) },
                    { "Folder1/Template1", Template.Parse(rootTemplate) },
                    { "Folder1/Sub/Template1", Template.Parse(folder1SubTemplate) },
                    { "Folder2/Template1", Template.Parse(rootTemplate) },
                    { "Folder2/Sub/Template1", Template.Parse(folder2SubTemplate) },
                },
            };

            yield return new object[] { _jsonProcessor, new TemplateProvider(templateCollection), _jsonTestData, subTemplate, folder1SubTemplate, folder2SubTemplate };
        }

        public static IEnumerable<object[]> GetValidInputsWithProcessSettings()
        {
            var positiveTimeOutSettings = new ProcessorSettings
            {
                TimeOut = 1, // expect operation to timeout after 1ms
            };

            var negativeTimeOutSettings = new ProcessorSettings
            {
                TimeOut = -1, // expect operation to not timeout
            };

            yield return new object[]
            {
                new JsonProcessor(new ProcessorSettings(), FhirConverterLogging.CreateLogger<JsonProcessor>()), new JsonProcessor(positiveTimeOutSettings, FhirConverterLogging.CreateLogger<JsonProcessor>()), new JsonProcessor(negativeTimeOutSettings, FhirConverterLogging.CreateLogger<JsonProcessor>()),
                new TemplateProvider(TestConstants.TestTemplateDirectory, DataType.Json), _jsonTestData,
            };
        }

        public static IEnumerable<object[]> GetValidInputsWithLargeForLoop()
        {
            yield return new object[]
            {
                _jsonProcessor,
                new TemplateProvider(TestConstants.TestTemplateDirectory, DataType.Json),
                _jsonTestData,
            };
        }

        public static IEnumerable<object[]> GetValidInputsWithNestingTooDeep()
        {
            yield return new object[]
            {
                _jsonProcessor,
                new TemplateProvider(TestConstants.TestTemplateDirectory, DataType.Json),
                _jsonTestData,
            };
        }


        [Fact]
        public void GivenJObjectInput_WhenConvertWithJsonProcessor_CorrectResultShouldBeReturned()
        {
            var processor = _jsonProcessor;
            var templateProvider = new TemplateProvider(TestConstants.JsonTemplateDirectory, DataType.Json);
            var testData = JsonConvert.DeserializeObject<JObject>(_jsonTestData, new JsonSerializerSettings() { DateParseHandling = DateParseHandling.None });
            var result = processor.Convert(testData, "ExamplePatient", templateProvider);
            Assert.True(JToken.DeepEquals(JObject.Parse(_jsonExpectData), JToken.Parse(result)));
        }


        [Fact]
        public void Tirs2()
        {
            var codevaluesetJson = File.ReadAllText(Path.Join(TestConstants.SampleDataDirectory, "Tirs", "codevalueset.json"));
            var codevalueJson = File.ReadAllText(Path.Join(TestConstants.SampleDataDirectory, "Tirs", "codevalue.json"));
            var processor = _tirsProcessor;
            var templateProvider = new TemplateProvider(TestConstants.TirsTemplateDirectory, DataType.Json);
            var codevaluesetTestData = JObject.Parse(codevaluesetJson);
            var codevalueTestData = JObject.Parse(codevalueJson);
            var result = processor.Convert(
                new Dictionary<string, JObject>()
                {
                    { "http://bki.skane.se/sdv-millennium/ont/codevalueset", codevaluesetTestData },
                    { "http://bki.skane.se/sdv-millennium/ont/codevalue", codevalueTestData },
                },
                "CodevaluesetToFhir",
                templateProvider);
            Assert.True(JToken.DeepEquals(JObject.Parse(_jsonTirsTest2ExpectData), JToken.Parse(result)));
        }

        [Fact]
        public void TestToFhir()
        {
            var processor = _tirsProcessor;
            var templateProvider = new TemplateProvider(TestConstants.TirsTemplateDirectory, DataType.Json);
            var result = processor.Convert(
                new Dictionary<string, JObject>(),
                "CodevaluesetToFhir2",
                templateProvider);

            // Skriv JSON till filen
            File.WriteAllText(@"C:\Users\OlofMattsson\codesystem.json", result);

            Assert.True(JToken.DeepEquals(JObject.Parse(_jsonTirsTest2ExpectData), JToken.Parse(result)));
        }

        [Fact]
        public void TestHistorskaLabbdata()
        {
            var _jsonTestDataHLD = File.ReadAllText(Path.Join(@"C:\SourceCode\regionskane\rs-on-fhir\skane-fhir-se-historiskalabbdata\projectfiles\fhirmapper\sampledata", "Kemiresultat2_560444801_XML.json"));
            var processor = _jsonProcessor;
            var templateProvider = new TemplateProvider(@"C:\SourceCode\regionskane\rs-on-fhir\skane-fhir-se-historiskalabbdata\projectfiles\fhirmapper\templates", DataType.Json);
            var testData = JsonConvert.DeserializeObject<JObject>(_jsonTestDataHLD, new JsonSerializerSettings() { DateParseHandling = DateParseHandling.None });
            var result = processor.Convert(testData, "kemiresultat2", templateProvider);
           // var result = processor.Convert(testData, "translate", templateProvider);
            Assert.True(JToken.DeepEquals(JObject.Parse(_jsonTirsTest2ExpectData), JToken.Parse(result)));
        }
    }
}

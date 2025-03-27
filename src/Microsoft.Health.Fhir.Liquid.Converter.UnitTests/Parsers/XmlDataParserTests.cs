// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Parsers;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.Parsers
{
    public class XmlDataParserTests
    {
        private readonly IDataParser _parser = new XmlDataParserLeafing();

        public static IEnumerable<object[]> GetNullOrEmptyXmlData()
        {
            yield return new object[] { null };
            yield return new object[] { string.Empty };
            yield return new object[] { " " };
            yield return new object[] { "\n" };
        }

        public static IEnumerable<object[]> GetInvalidXmlData()
        {
            // Malformed XML
            yield return new object[] { "abc" }; // Not XML at all
            yield return new object[] { "<root>" }; // Unclosed root
            yield return new object[] { "<root><child></root>" }; // Mismatched tags
            yield return new object[] { "<root><child/></roo>" }; // Typo in closing tag
            yield return new object[] { "<root><child><nested></child></nested></root>" }; // Incorrect nesting
            yield return new object[] { "<?xml version=\"1.0\"?><root><child></child" }; // Missing closing bracket
        }

        public static IEnumerable<object[]> GetValidXmlData()
        {
            // Simple valid XML
            yield return new object[] { "<root></root>" };
            yield return new object[] { "<root><child/></root>" };

            // XML with attributes and namespaces
            yield return new object[] {
                @"<root xmlns=""http://example.com"" attr=""value"">
                    <child attr2=""val2"">Text</child>
                  </root>"
            };

            // XML with multiple siblings and nested elements
            yield return new object[] {
                @"<root>
                    <child>Value1</child>
                    <child>Value2</child>
                    <parent>
                        <childA>A</childA>
                        <childA>B</childA>
                        <childB>C</childB>
                    </parent>
                  </root>"
            };

            // XML with whitespace and text nodes
            yield return new object[] {
                @"<root>
                    <child>
                        <grandchild>   Some Text   </grandchild>
                    </child>
                  </root>"
            };
        }

        [Theory]
        [MemberData(nameof(GetNullOrEmptyXmlData))]
        public void GivenNullOrEmptyData_WhenParse_ExceptionShouldBeThrown(string input)
        {
            var exception = Assert.Throws<DataParseException>(() => _parser.Parse(input));
            Assert.Equal(FhirConverterErrorCode.NullOrWhiteSpaceInput, exception.FhirConverterErrorCode);
        }

        [Theory]
        [MemberData(nameof(GetInvalidXmlData))]
        public void GivenInvalidXmlData_WhenParse_ExceptionShouldBeThrown(string input)
        {
  
            var exception = Assert.Throws<DataParseException>(() => _parser.Parse(input));
            Assert.Equal(FhirConverterErrorCode.InputParsingError, exception.FhirConverterErrorCode);
            Assert.Throws<XmlException>(() => { var doc = new System.Xml.XmlDocument(); doc.LoadXml(input); });
            
        }

        [Theory]
        [MemberData(nameof(GetValidXmlData))]
        public void GivenValidXmlData_WhenParse_DataShouldBeParsedCorrectly(string input)
        {
            var data = _parser.Parse(input);
            Assert.NotNull(data);

            // Verify the data can also be loaded by the default XML parser without exception
            var doc = new System.Xml.XmlDocument();
            doc.LoadXml(input);
            Assert.NotNull(doc.DocumentElement);
        }
    }
}

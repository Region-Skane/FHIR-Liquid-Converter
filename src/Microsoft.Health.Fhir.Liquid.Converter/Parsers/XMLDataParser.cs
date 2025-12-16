// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter.Parsers
{
    public abstract class XmlDataParser : IDataParser
    {
        public object Parse(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                throw new DataParseException(FhirConverterErrorCode.NullOrWhiteSpaceInput, "The input data is null or whitespace.");
            }

            try
            {
                var xdoc = XDocument.Parse(data, LoadOptions.PreserveWhitespace);
                var dict = XmlToDictionary(xdoc.Root, null);
                return dict;
            }
            catch (XmlException ex)
            {
                throw new DataParseException(FhirConverterErrorCode.InputParsingError, $"XML parsing error: {ex.Message}", ex);
            }
        }

        private Dictionary<string, object> XmlToDictionary(XElement element, string parentNamespace)
        {
            return ProcessElement(element, parentNamespace) as Dictionary<string, object>;
        }

        protected object ProcessElement(XElement element, string parentNamespace)
        {
            var dict = new Dictionary<string, object>();

            AddNamespaceIfDifferent(dict, element, parentNamespace);
            AddAttributes(dict, element);

            var ns = element.Name.NamespaceName;

            if (element.HasElements)
            {
                var groupedElements = GroupElementsByLocalName(element.Elements());

                foreach (var (childName, childElements) in groupedElements)
                {
                    dict[childName] = HandleChildren(childName, childElements, ns);
                }

                var trimmedValue = element.Value.Trim();
                if (!string.IsNullOrEmpty(trimmedValue) && !HasSignificantChildText(element))
                {
                    dict["#text"] = trimmedValue;
                }
            }
            else
            {
                var value = element.Value.Trim();
                if (dict.Count > 0 && !string.IsNullOrEmpty(value))
                {
                    dict["#text"] = value;
                    return dict;
                }

                if (!string.IsNullOrEmpty(value))
                {
                    return value;
                }

                if (dict.Count > 0)
                {
                    return dict;
                }
            }

            return dict;
        }

        /// <summary>
        /// Defines how to transform a group of child XML elements with the same name into the target data structure.
        /// Implementations determine whether to return a single object (leafing behavior)
        /// or always return a list of objects (array behavior), regardless of the number of elements.
        /// </summary>
        /// <param name="childName">The local name of the repeated child XML elements.</param>
        /// <param name="childElements">A list of child elements with the same name under the current XML node.</param>
        /// <param name="ns">The XML namespace of the parent element.</param>
        /// <returns>
        /// For <c>XmlDataParserLeafing</c>, returns a single object if one element is present, or a list otherwise.
        /// For <c>XmlDataParserArrayAlways</c>, always returns a list of parsed child elements.
        /// </returns>
        protected abstract object HandleChildren(string childName, List<XElement> childElements, string ns);

        private void AddNamespaceIfDifferent(Dictionary<string, object> dict, XElement element, string parentNamespace)
        {
            var ns = element.Name.NamespaceName;
            if (!string.IsNullOrEmpty(ns) && ns != parentNamespace)
            {
                dict["@xmlns"] = ns;
            }
        }

        private void AddAttributes(Dictionary<string, object> dict, XElement element)
        {
            foreach (var attr in element.Attributes())
            {
                dict["@" + attr.Name.LocalName] = attr.Value ?? string.Empty;
            }
        }

        private Dictionary<string, List<XElement>> GroupElementsByLocalName(IEnumerable<XElement> elements)
        {
            var grouped = new Dictionary<string, List<XElement>>();
            foreach (var child in elements)
            {
                var name = child.Name.LocalName;
                if (!grouped.TryGetValue(name, out var list))
                {
                    list = new List<XElement>();
                    grouped[name] = list;
                }

                list.Add(child);
            }

            return grouped;
        }

        private bool HasSignificantChildText(XElement element) =>
            element.Elements().Any(e => !string.IsNullOrWhiteSpace(e.Value));
    }
}

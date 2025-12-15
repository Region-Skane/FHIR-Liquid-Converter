// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.Health.Fhir.Liquid.Converter.Parsers
{
    /// <summary>
    /// Always wraps children in a list, even if only one element exists.
    /// Enforces a uniform array structure in the resulting dictionary, regardless of how many child elements exist.
    /// </summary>
    public class XmlDataParserArrayAlways : XmlDataParser
    {
        protected override object HandleChildren(string childName, List<XElement> childElements, string ns)
        {
            return childElements.Select(e => ProcessElement(e, ns)).ToList();
        }
    }
}

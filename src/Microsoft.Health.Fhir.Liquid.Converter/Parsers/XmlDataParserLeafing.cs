using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.Health.Fhir.Liquid.Converter.Parsers
{
    /// <summary>
    /// Structures XML-to-dictionary by flattening single-child elements into a single value (instead of wrapping them in an array)
    /// </summary>
    public class XmlDataParserLeafing : XmlDataParser
    {
        protected override object HandleChildren(string childName, List<XElement> childElements, string ns)
        {
            return childElements.Count == 1
                ? ProcessElement(childElements[0], ns)
                : childElements.Select(e => ProcessElement(e, ns)).ToList();
        }
    }
}

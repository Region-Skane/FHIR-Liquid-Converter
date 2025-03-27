using System;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Parsers;

namespace Microsoft.Health.Fhir.Liquid.Converter.Processors
{
    /// <summary>
    /// Xml Processor using leafing parser <see cref="XmlDataParserLeafing"/>
    /// </summary>
    public class XmlLeafingProcessor : XmlProcessor
    {
        public XmlLeafingProcessor(ProcessorSettings processorSettings, ILogger<XmlProcessor> logger)
            : base(processorSettings, new XmlDataParserLeafing(), logger)
        {
        }
    }
}

// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Parsers;

namespace Microsoft.Health.Fhir.Liquid.Converter.Processors
{
    /// <summary>
    /// Xml Processor using array always parser <see cref="XmlDataParserArrayAlways"/>
    /// </summary>
    public class XmlArrayProcessor : XmlProcessor
    {
        public XmlArrayProcessor(ProcessorSettings processorSettings, ILogger<XmlProcessor> logger)
            : base(processorSettings, new XmlDataParserArrayAlways(), logger)
        {
        }
    }
}

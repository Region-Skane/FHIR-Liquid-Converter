// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Liquid.Converter.OutputProcessors
{
    /// <summary>
    /// Defines an interface for post-processing the output of liquid template rendering in the FHIR converter.
    /// </summary>
    public interface IOutputPostProcessor
    {
        /// <summary>
        /// Process the output after liquid template rendering.
        /// </summary>
        /// <param name="input">The input string to process.</param>
        /// <param name="settings">The processor settings.</param>
        /// <returns>The processed output as a JObject.</returns>
        JObject Process(JObject input, ProcessorSettings settings);
    }
}

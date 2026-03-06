// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Health.Fhir.Liquid.Converter.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter.OutputProcessors
{
    /// <summary>
    /// Factory class for creating appropriate output post processors based on the provided processor settings.
    /// </summary>
    public static class OutputPostProcessorsFactory
    {
        /// <summary>
        /// Creates an appropriate output post processor based on the provided settings.
        /// </summary>
        /// <param name="settings">The processor settings.</param>
        /// <returns>The appropriate output post processor.</returns>
        public static List<IOutputPostProcessor> Create(ProcessorSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings, nameof(settings));
            ArgumentNullException.ThrowIfNull(settings.Validation, nameof(settings.Validation));

            if (settings.Validation.ValidateOutput)
            {
                return [new PostProcessorFhirValidator()];
            }

            return [];
        }
    }
}

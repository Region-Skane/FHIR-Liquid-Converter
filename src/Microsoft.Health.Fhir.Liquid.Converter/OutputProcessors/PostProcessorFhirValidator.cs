// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------

using System;
using Hl7.Fhir.Serialization;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Validators;
using Newtonsoft.Json.Linq;
using FhirModel = Hl7.Fhir.Model;

namespace Microsoft.Health.Fhir.Liquid.Converter.OutputProcessors
{
    /// <summary>
    /// <inheritdoc cref="IOutputPostProcessor"/>
    /// </summary>
    public class PostProcessorFhirValidator : IOutputPostProcessor
    {
        /// <summary>
        /// <inheritdoc cref="IOutputPostProcessor.Process(JObject, ProcessorSettings)"/>
        /// </summary>
        /// <param name="input">The input JObject to process.</param>
        /// <param name="settings">The processor settings.</param>
        /// <returns>The processed JObject.</returns>
        /// <exception cref="PostprocessException">Thrown when the output is not a valid FHIR resource.</exception>
        public JObject Process(JObject input, ProcessorSettings settings)
        {
            FhirModel.Resource fhirResource;
            try
            {
                var parser = new FhirJsonParser();
                fhirResource = parser.Parse<FhirModel.Resource>(input.ToString());
            }
            catch (Exception ex)
            {
                throw new PostprocessException(
                    FhirConverterErrorCode.JsonParsingError,
                    string.Format(Resources.JsonParsingError, $"The generated output could not be interpreted as a valid FHIR resource. {ex.Message}"),
                    rawOutputString: settings.AllowOutputValidationErrors ? input.ToString() : null,
                    ex);
            }

            ProfileValidator.Validate(fhirResource, settings.Validation.FhirCacheDirectory, input);

            return input;
        }
    }
}

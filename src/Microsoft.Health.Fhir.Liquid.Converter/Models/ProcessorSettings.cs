// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
//
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Liquid.Converter.Models
{
    /// <summary>
    /// Represents configuration settings for controlling template processing behavior.
    /// </summary>
    public class ProcessorSettings
    {
        /// <summary>
        /// Time out for rendering templates in milliseconds. By default no time out is set, which is zero in DotLiquid.
        /// </summary>
        public int TimeOut { get; set; } = 0;

        /// <summary>
        /// Max iterations for rendering templates.
        /// </summary>
        public int MaxIterations { get; set; } = 100000;

        /// <summary>
        /// Enable the Telemetry Logger in the processor.
        /// </summary>
        public bool EnableTelemetryLogger { get; set; } = false;

        /// <summary>
        /// Allow output validation errors, ie bad json. Will return the raw output.
        /// </summary>
        public bool AllowOutputValidationErrors { get; set; } = false;

        /// <summary>
        /// Validation settings for the processor, including whether to validate output FHIR resources and the directory to use for caching FHIR resources.
        /// </summary>
        public ValidationSettings Validation { get; set; } = new ();

        /// <summary>
        /// Output Fhir Serialization Format, eg Json (Default) or Xml
        /// </summary>
        public FhirSerializationFormat SerializationFormat { get; set; } = FhirSerializationFormat.Json;

        /// <summary>
        /// Settings related to validation of output FHIR resources, including whether to validate output and the directory to use for caching FHIR resources.
        /// </summary>
        public class ValidationSettings
        {
            /// <summary>
            /// Validate output FHIR resources against their declared profiles. By default, validation is disabled to optimize performance.
            /// </summary>
            public bool ValidateOutput { get; set; }

            /// <summary>
            /// Directory to use for caching FHIR resources. If not set, the default FHIR cache directory is used.
            /// </summary>
            public string FhirCacheDirectory { get; set; } = null;
        }
    }
}

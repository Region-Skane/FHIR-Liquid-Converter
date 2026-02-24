// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
//
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Fhir.Liquid.Converter.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter.Exceptions
{
    public class PostprocessException : FhirConverterException
    {
        public PostprocessException(FhirConverterErrorCode fhirConverterErrorCode, string message)
            : base(fhirConverterErrorCode, message)
        {
        }

        public PostprocessException(FhirConverterErrorCode fhirConverterErrorCode, string message, Exception innerException)
            : base(fhirConverterErrorCode, message, innerException)
        {
        }

        public PostprocessException(FhirConverterErrorCode fhirConverterErrorCode, string message, string rawOutputString)
            : base(fhirConverterErrorCode, message)
        {
            RawOutputString = rawOutputString;
        }

        public PostprocessException(FhirConverterErrorCode fhirConverterErrorCode, string message, string rawOutputString, Exception innerException)
            : base(fhirConverterErrorCode, message, innerException)
        {
            RawOutputString = rawOutputString;
        }

        /// <summary>
        /// Contains the raw output from liquid transformation in case of output validation error
        /// </summary>
        public string RawOutputString { get; }
    }
}

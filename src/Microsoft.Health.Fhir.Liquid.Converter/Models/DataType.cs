// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Liquid.Converter.Models
{
    /// <summary>
    /// The input data types supported by FHIR Converter
    /// </summary>
    public enum DataType
    {
        Hl7v2,
        Ccda,
        Json,
        Fhir,
        XmlLeafing, // Flattening single-child elements into a single value
        XmlAlwaysArray, // Always wraps children in an array
    }
}

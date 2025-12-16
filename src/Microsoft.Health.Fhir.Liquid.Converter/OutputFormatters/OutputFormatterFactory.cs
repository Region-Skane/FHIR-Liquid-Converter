// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB. All rights reserved.
// Licensed under the Apache License, Version 2.0 License (Apache License, Version 2.0). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Fhir.Liquid.Converter.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter.OutputFormatters;

public static class OutputFormatterFactory
{
    public static IOutputFormatter Create(FhirSerializationFormat format)
    {
        return format switch
        {
            FhirSerializationFormat.Json => new FhirJsonOutputFormatter(),
            FhirSerializationFormat.Xml => new FhirXmlOutputFormatter(),
            _ => throw new NotSupportedException($"Unsupported serialization format: {format}")
        };
    }
}

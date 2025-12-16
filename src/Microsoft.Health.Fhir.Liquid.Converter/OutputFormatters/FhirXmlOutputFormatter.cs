// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB. All rights reserved.
// Licensed under the Apache License, Version 2.0 License (Apache License, Version 2.0). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Liquid.Converter.OutputFormatters;

public class FhirXmlOutputFormatter : IOutputFormatter
{
    private static readonly ParserSettings _parserSettings = new ()
    {
        AcceptUnknownMembers = true,
        AllowUnrecognizedEnums = true,
    };

    private static readonly FhirJsonParser _parser = new (_parserSettings);

    private static readonly SerializerSettings _serializerSettings = new SerializerSettings
    {
        Pretty = true,
    };

    private static readonly FhirXmlSerializer _serializer = new (_serializerSettings);

    public string Format(JObject cleanedJson)
    {
        var json = cleanedJson.ToString();
        var resource = _parser.Parse<Resource>(json);
        return _serializer.SerializeToString(resource);
    }
}
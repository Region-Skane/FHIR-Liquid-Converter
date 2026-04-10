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
    private static readonly DeserializerSettings _parserSettings = new ()
    {
        AcceptUnknownMembers = true,
        AllowUnrecognizedEnums = true,
    };

    private static readonly FhirJsonDeserializer _parser = new (_parserSettings);

    private static readonly FhirXmlSerializer _serializer = new ();

    public string Format(JObject cleanedJson)
    {
        var json = cleanedJson.ToString();
        var resource = _parser.Deserialize<Resource>(json);
        return _serializer.SerializeToString(resource, pretty: true);
    }
}
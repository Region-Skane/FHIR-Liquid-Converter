// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB. All rights reserved.
// Licensed under the Apache License, Version 2.0 License (Apache License, Version 2.0). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Liquid.Converter.OutputFormatters;

public interface IOutputFormatter
{
    string Format(JObject cleanedJson);
}
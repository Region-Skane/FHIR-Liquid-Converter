// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------
using Hl7.Fhir.Serialization;
using Microsoft.Health.Fhir.Liquid.Converter.Extensions;
using System.Collections.Generic;

namespace ServiceWell.Health.Fhir.Liquid.Converter
{
    /// <summary>
    /// Filters for Fhir Terminology Services
    /// </summary>
    ///
    public partial class FlcFilters
    {

        public static IDictionary<string, object> FhirTerminologyClient(string fhirServer, string fhirRequest, params object[] parameters)
        {
            for (int i = 0; i < parameters?.Length; i++)
            {
                fhirRequest = fhirRequest.Replace($"[p{i}]", parameters[i]?.ToString() ?? string.Empty);
            }

            return FhirTerminologyClient(fhirServer, fhirRequest);
        }

        public static IDictionary<string, object> FhirTerminologyClient(string fhirServer, string fhirRequest)
        {
            //https://onto.fhir.link/fhir
            //https://r4.ontoserver.csiro.au/fhir
            var client = new Hl7.Fhir.Rest.FhirClient(fhirServer);

            var response = client.Get(fhirRequest);
            var jsonObject = response.ToJObject();

            // Omvandla till en Dictionary och returnera
            return jsonObject.ToObject() as Dictionary<string, object> ?? new Dictionary<string, object>();//uses the ToObject extension
        }
    }
}

// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Fhir.Liquid.Converter.Extensions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Microsoft.Health.Fhir.Liquid.Converter
{
    /// <summary>
    /// Filters for TIRS and BKI
    /// </summary>
    ///
    public partial class Filters
    {

        public static IDictionary<string, object> TirsClient(string server, string request, params object[] parameters)
        {
            for (int i = 0; i < parameters?.Length; i++)
            {
                request = request.Replace($"[p{i}]", parameters[i]?.ToString() ?? string.Empty);
            }

            return TirsClient(server, request);
        }

        public static IDictionary<string, object> TirsClient(string server, string request)
        {
            //https://tirs.skane.se/api/v2.0/
            var client = new HttpClient()
            {
                BaseAddress = new Uri(server)
            };
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var response = client.GetAsync(request).GetAwaiter().GetResult();


            // Läs innehållet i svaret synkront
            var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            // Kontrollera om innehållet är JSON
            if (content.TrimStart().StartsWith("{") || content.TrimStart().StartsWith("["))
            {
                // Omvandla JSON-sträng till JObject
                var jsonObject = JObject.Parse(content);

                // Försök att hämta "data"-fältet
                var data = jsonObject["data"];

                if (data != null)
                {
                    return data.ToObject() as Dictionary<string, object> ?? new Dictionary<string, object>();
                }
                else
                {
                    throw new Exception("'data' field is missing in the response.");
                }
            }
            else
            {
                throw new Exception("Response is not in JSON format.");
            }
        }

        /// <summary>
        /// Get the value in key "data" for the bki-koncept
        /// </summary>
        /// <param name="enumerableInput"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static IEnumerable GetBkiKoncept(IEnumerable enumerableInput, string bkiKoncept)
        {
            if (enumerableInput == null)
            {
                return null;
            }

            // Enumerate to a list so we can repeatedly parse through the collection.
            List<object> listedInput = enumerableInput.Cast<object>().ToList();

            //Get the right bki-koncept and take the data
            var data = listedInput.Where(kvp =>
            {
                return ((KeyValuePair<string, object>)kvp).Key == bkiKoncept;
            })?.Select(target => ((KeyValuePair<string, object>)target).Value).ToList();

            // If the list happens to be empty we are done already.
            if (!data.Any())
            {
                return data;
            }

            return (IEnumerable)((Dictionary<string, object>)data.First())["data"];
        }

        public static object DebugTirs(object input)
        {
            return input;
        }
    }
}

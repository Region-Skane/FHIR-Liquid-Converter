using DotLiquid;
using DotLiquid.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

namespace Microsoft.Health.Fhir.Liquid.Converter
{
    /// <summary>
    /// Filters for TIRS and BKI
    /// </summary>
    ///
    public partial class Filters
    {

        // Regex pattern to capture different parts of the BKI URI
        // Updated Regex to optionally match segments beyond the domain
        private static readonly Regex BkiRegex = new Regex(
            @"^(?<scheme>https?:\/\/)(?<doman>[^\/]+)(?:\/(?<sektor>[^\/]+))?(?:\/(?<typ>[^\/]+))?(?:\/(?<koncept>[^\/]+))?(?:\/(?<reference>[^\/]+))?(?:\/(?<version>[^\/?]+))?(?:\?(?<query>.*))?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Extract a specific named group from the URI using Regex
        public static string GetUriPart(string input, string partName)
        {
            var match = BkiRegex.Match(input);
            if (match.Success)
            {
                return match.Groups[partName].Success ? match.Groups[partName].Value : "Segment not found";
            }
            return "Part not found";
        }

        // Extract part of the URI including the scheme
        public static string GetUriPartIncludingScheme(string input, string partName)
        {
            var match = BkiRegex.Match(input);
            if (match.Success)
            {
                return match.Groups["scheme"].Value + match.Groups[partName].Value;
            }
            return "Part not found";
        }

        // Extract domain-specific methods
        public static string BkiDoman (string input) => GetUriPart(input, "doman");

        public static string DomanBki(string input) => GetUriPartIncludingScheme(input, "doman");

        // Sector-specific methods
        public static string BkiSektor (string input) => GetUriPart(input, "sektor");
        
        public static string SektorBki (string input) => GetUriPartIncludingScheme(input, "doman") + "/" + GetUriPart(input, "sektor");

        // Typ-specific methods
        public static string BkiTyp(string input) => GetUriPart(input, "typ");
        public static string TypBki(string input) => SektorBki(input) + "/" + GetUriPart(input, "typ");

        // Koncept-specific methods
        public static string BkiKoncept(string input) => GetUriPart(input, "koncept");
        public static string KonceptBki(string input) => TypBki(input) + "/" + GetUriPart(input, "koncept");

        // Referens-specific methods
        public static string BkiReferens(string input) => GetUriPart(input, "reference");
        public static string ReferensBki(string input) => KonceptBki(input) + "/" + GetUriPart(input, "reference");

        public static string BkiVersion(string input) => GetUriPart(input, "version");
        public static string BkiFormat(string input) => GetQueryParam(input, "_format");

        // Extract query parameters specifically if needed
        public static string GetQueryParam(string input, string paramName)
        {
            Uri uri = new Uri(input);
            string query = HttpUtility.ParseQueryString(uri.Query)[paramName];
            return string.IsNullOrEmpty(query) ? "Parameter not found" : query;
        }
    }
}

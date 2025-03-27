// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Liquid.Converter.Tool.Models
{
    public class ConverterResult
    {
        public ConverterResult(ProcessStatus status, string fhirResource, TraceInfo traceInfo, string errorMessage = null)
        {
            Status = status;
            if (status == ProcessStatus.OK)
            {
                FhirResource = new JRaw(fhirResource);
            }
            else
            {
                RawOutput = fhirResource;
            }

            ErrorMessage = errorMessage;
            TraceInfo = traceInfo;
        }

        public ProcessStatus Status { get; set; }

        public JRaw FhirResource { get; set; }

        public TraceInfo TraceInfo { get; set; }

        public string RawOutput { get; set; }

        public string ErrorMessage { get; set; }
    }
}

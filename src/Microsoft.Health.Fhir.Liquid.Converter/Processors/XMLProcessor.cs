using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using DotLiquid;
using EnsureThat;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Json;
using Microsoft.Health.Fhir.Liquid.Converter.Parsers;
using Microsoft.Health.MeasurementUtility;
using NJsonSchema;

namespace Microsoft.Health.Fhir.Liquid.Converter.Processors
{
    public abstract class XmlProcessor : BaseProcessor
    {
        private readonly IDataParser _parser;

        protected XmlProcessor(ProcessorSettings processorSettings, IDataParser parser, ILogger<XmlProcessor> logger)
            : base(processorSettings, logger)
        {
            _parser = EnsureArg.IsNotNull(parser, nameof(parser));
        }

        protected override string InternalConvert(string data, string rootTemplate, ITemplateProvider templateProvider, TraceInfo traceInfo = null)
        {
            object xmlData;

            using (ITimed inputDeserializationTime = Performance.TrackDuration(
                duration => LogTelemetry(FhirConverterMetrics.InputDeserializationDuration, duration)))
            {
                xmlData = _parser.Parse(data);
            }

            var liquidResult = InternalConvertFromObject(xmlData, rootTemplate, templateProvider, traceInfo);

            return liquidResult;
        }

        protected override Context CreateBaseContext(ITemplateProvider templateProvider, IDictionary<string, object> data)
        {
            var cancellationToken = Settings.TimeOut > 0 ? new CancellationTokenSource(Settings.TimeOut).Token : CancellationToken.None;

            var context = new JSchemaContext(
                environments: new List<Hash> { Hash.FromDictionary(data) },
                outerScope: new Hash(),
                registers: Hash.FromDictionary(new Dictionary<string, object> { { "file_system", templateProvider.GetTemplateFileSystem() } }),
                errorsOutputMode: ErrorsOutputMode.Rethrow,
                maxIterations: Settings.MaxIterations,
                formatProvider: CultureInfo.InvariantCulture,
                cancellationToken: cancellationToken)
            {
                ValidateSchemas = new List<JsonSchema>(),
            };

            return context;
        }
    }
}
// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
//
// Modifications Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Tool.Configuration;
using Microsoft.Health.Fhir.Liquid.Converter.Tool.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter.Tool
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
#if DEBUG
            var appSettings = GetAppSettings();
            if (appSettings.AllowDebuggerLaunch && !Debugger.IsAttached)
            {
                Debugger.Launch();
                Console.WriteLine("Debugger attached, waiting for input...");
                Console.ReadLine();
            }
#endif
            var parseResult = Parser.Default.ParseArguments<ConverterOptions, PullTemplateOptions, PushTemplateOptions,
                PackageManagementOptions, PackageManagementListOptions, PackageManagementValidateOptions, FlcConvertOptions>(args);
            try
            {
                int exitCode = 0;

                parseResult.WithParsed<ConverterOptions>(ConverterLogicHandler.Convert);
                await parseResult.WithParsedAsync<PullTemplateOptions>(TemplateManagementLogicHandler.PullAsync);
                await parseResult.WithParsedAsync<PushTemplateOptions>(TemplateManagementLogicHandler.PushAsync);
                await parseResult.WithParsedAsync<PackageManagementOptions>(PackageManagementLogicHandler.ImportPackage);
                await parseResult.WithParsedAsync<PackageManagementListOptions>(PackageManagementLogicHandler.ListPackages);
                await parseResult.WithParsedAsync<PackageManagementValidateOptions>(
                    async options => exitCode = await PackageManagementLogicHandler.ValidatePackage(options));
                await parseResult.WithParsedAsync<FlcConvertOptions>(FlcConverterLogicHandler.FlcConvert);
                parseResult.WithNotParsed(HandleOptionsParseError);

                return exitCode;
            }
            catch (PostprocessException ex)
            {
                Console.Error.WriteLine($"PostProcess failed: {ex.Message}");
                return (int)ex.FhirConverterErrorCode;
            }
            catch (PostprocessException ex)
            {
                Console.Error.WriteLine($"PostProcess failed: {ex.Message}");
                return (int)ex.FhirConverterErrorCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Process failed: {ex.Message}");
                return -1;
            }
        }

        private static AppSettings GetAppSettings()
        {
            var configuration = ConfigurationHelper.BuildConfiguration();
            return configuration.Get<AppSettings>();
        }

        private static void HandleOptionsParseError(IEnumerable<Error> errors)
        {
            if (!errors.IsHelp() && !errors.IsVersion())
            {
                throw new InputParameterException(@"The input option is invalid.");
            }
        }
    }
}
// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotLiquid;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.TemplateManagement.ArtifactProviders;
using Microsoft.Health.Fhir.TemplateManagement.Configurations;
using Xunit;

namespace Microsoft.Health.Fhir.TemplateManagement.UnitTests.Providers
{
    public class DefaultTemplateCollectionProviderTests
    {
        private readonly TemplateCollectionConfiguration _config;
        private readonly IMemoryCache _cache;
        private readonly IConvertDataTemplateCollectionProvider _defaultTemplateCollectionProvider;

        private static readonly string _templateDirectory = Path.Join("../../../../../", "data", "Templates");

        private static Dictionary<DefaultRootTemplateParentPath, string> _defaultTemplatesFolderInfo = new ()
        {
            { DefaultRootTemplateParentPath.Hl7v2, "Hl7v2" },
            { DefaultRootTemplateParentPath.Ccda, "Ccda" },
            { DefaultRootTemplateParentPath.Json, "Json" },
            { DefaultRootTemplateParentPath.Fhir, "Stu3ToR4" },
            { DefaultRootTemplateParentPath.FhirToHl7v2, "FhirToHl7v2" },
        };

        public DefaultTemplateCollectionProviderTests()
        {
            _config = new TemplateCollectionConfiguration();
            _cache = new MemoryCache(new MemoryCacheOptions());
            _defaultTemplateCollectionProvider = new DefaultTemplateCollectionProvider(_cache, _config);
        }

        [Fact]
        public async Task GivenDefaultTemplateProvider_WhenGetTemplateCollectionAsync_ThenTemplatesAreCached()
        {
            var templateCollection = await _defaultTemplateCollectionProvider.GetTemplateCollectionAsync(CancellationToken.None);

            Assert.Single(templateCollection);

            // Verify templates are cached
            Assert.True(_cache.TryGetValue("cached-default-templates", out List<Dictionary<string, Template>> cachedTemplateCollection));
            Assert.Equal(templateCollection, cachedTemplateCollection);

            // Call again to test cache
            cachedTemplateCollection = await _defaultTemplateCollectionProvider.GetTemplateCollectionAsync(CancellationToken.None);

            Assert.NotNull(cachedTemplateCollection);
            Assert.Equal(templateCollection, cachedTemplateCollection);
        }
    }
}

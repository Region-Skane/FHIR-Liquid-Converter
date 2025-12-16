// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FLC.PackageManagement.UnitTests.Library
{
    public static class LibraryTestHelpers
    {
        public static string MakeLibraryJsonSample(
            bool duplicateFile = false,
            bool corruptFileContent = false,
            bool makeJsonInvalid = false,
            bool missingOuterExtension = false,
            bool missingInnerExtension = false,
            bool missingFolderPath = false,
            bool missingLogicalFilename = false,
            bool missingData = false)
        {
            var filename = new JsonObject
            {
                ["url"] = JsonValue.Create("logical-filename"),
                ["valueString"] = JsonValue.Create("_aliases.liquid"),
            };

            var folderPath = new JsonObject
            {
                ["url"] = JsonValue.Create("folder-path"),
                ["valueString"] = JsonValue.Create("templates/variables"),
            };

            var innerExtension = new JsonArray();
            if (!missingLogicalFilename)
            {
                innerExtension.Add(filename);
            }

            if (!missingFolderPath)
            {
                innerExtension.Add(folderPath);
            }

            var folderStructureExtension = new JsonObject
            {
                ["url"] = JsonValue.Create("http://bki.skane.se/fhir/earkiv-ips-pmo-flc/StructureDefinition/attachment-folder-structure"),
            };

            if (!missingInnerExtension)
            {
                folderStructureExtension.Add("extension", innerExtension);
            }

            var outerExtension = new JsonArray() { folderStructureExtension };
            var contentEntry = new JsonObject();
            if (!missingOuterExtension)
            {
                contentEntry.Add("extension", outerExtension);
            }

            if (!missingData)
            {
                contentEntry.Add("data", JsonValue.Create((corruptFileContent ? "$" : string.Empty) + "eyUgY29tbWVudCAtJX0gRmhpciBzaG9ydGhhbmQgYWxpYXNlcyB7JSBlbmRjb21tZW50IC0lfQo="));
            }

            var contentArray = new JsonArray() { contentEntry };

            if (duplicateFile)
            {
                contentArray.Add(contentArray.First().DeepClone());
            }

            var rootObject = new JsonObject() { ["content"] = contentArray };

            var json = rootObject.ToJsonString(new JsonSerializerOptions { WriteIndented = true });

            if (!makeJsonInvalid)
            {
                return json;
            }

            return json[..^1];
        }
    }
}

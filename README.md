# FHIR Liquid Converter

An open source tool by **Service Well AB** for converting healthcare data to HL7 FHIR using Liquid templates,  
with extended support for terminology translation and FHIR Implementation Guides integration.

---

## License

This project is licensed under the Apache License, Version 2.0.  
See the [LICENSE](LICENSE) file for full license text.

**Third-party components:**  
Portions of this project include source code from Microsoft Corporation,  
which is licensed under the MIT License.

The original Microsoft source files retain their copyright  
and license headers. These headers may refer to a `LICENSE` file in the repo root,  
but in this project, the MIT License text is provided in [`LICENSE-MICROSOFT.txt`](LICENSE-MICROSOFT.txt).

For clarity:
- The overall project license is **Apache License 2.0**.
- The MIT license applies only to the specific files originating from Microsoft.

---

## What's New? (April 2025)

The latest updates include:

- Support for terminology translation via Ontoserver or local mapping files.
- Improved caching and error handling for terminology filters.
- XML as source format.
- Compatible with the FHIR Liquid Converter Extension for VS Code with better debugging possibilities.

---

## Architecture

The FHIR Liquid Converter is built around:

- **Liquid templates** for defining data mappings.
- **Terminology filters** for concept translations.
- Optional integration with **Ontoserver** for terminology services.

Goal: Use FHIR IGs (Implementation Guides) as the foundation for conversion projects with the FLC extension.  
The FLC extension holds the Liquid templates and also FSH for documentation, ConceptMaps, LogicalModels,  
Mapping resources, etc. Everything can be tested locally and published/distributed as a standard FHIR IG.

---

## Templates & Authoring

Templates are written in [Liquid](https://shopify.github.io/liquid/) and leverage both built-in and custom filters.

| Conversion Type              | Notes                                        |
|-----------------------------|-----------------------------------------------|
| HL7v2 to FHIR                | Customizable using Liquid templates.         |
| JSON to FHIR                 | Supported with flexible mapping logic.       |
| FHIR STU3 to R4              | Supports version migration mappings.         |
| FHIR to HL7v2 (*Preview*)    | Early support for reverse mappings.          |
| XML to FHIR   (*Preview*)    | Supported with flexible mapping logic.       |

### Concepts

When authoring templates, consider reviewing:

- [Filters and Tags](docs/Filters-and-Tags.md)
- [Snippet Concept](docs/SnippetConcept.md)
- [Resource ID Generation](docs/concepts/resource-id-generation.md)
- [Validation and Post-processing](docs/concepts/validation-and-postprocessing.md)

---

## Terminology Dependencies

The FHIR Liquid Converter allows specifying a FHIR Terminology Server that is required for specific terminology filters. This enables:

- Automatic upload of terminology resources to Ontoserver.
- Validation that required dependencies are present.

Terminology translation can be done at runtime via Ontoserver or via local mapping files for batch processing and migrations.

---

## Added Value and Project Direction

This project is a fork of the original Microsoft FHIR Converter, with enhancements and strategic differences led by **Service Well AB**.

### 🚀 Advanced Terminology Service Integration

- Support for **FHIR Terminology Services**, including:
  - ConceptMap
  - Local mapping files for migrations and batch processing (MS-style fallback where appropriate).
- Focus on making FHIR IG and terminology a **first-class citizen** of the conversion process — not an afterthought.

### Native Integration with FHIR Implementation Guides (IG)

- Mapping templates are treated as part of the **IG structure** — authored and versioned together.
- Template packages are published as **FHIR Packages (npm-compatible)**, supporting dependency resolution via:
  - Private NPM feeds.
  - Standard FHIR Package tooling.
- Enables traceability and governance:
  - Which ConceptMaps and templates were used for a specific IG release.
  - Which terminology versions are required.

This approach provides stronger alignment with real-world healthcare interoperability projects and national/international specifications.

### ⚡ Why FLC?

- Simplifies migration from legacy systems while staying fully compliant with FHIR best practices.
- Ensures that **terminology mappings** are handled consistently across all stages — from development, to testing, to production.
- Allows combining local mapping performance (batch jobs, migrations) with the flexibility of runtime Terminology Services for real-time scenarios.

| Feature                           | Microsoft Original                  | Service Well Fork (FLC)                  |
|----------------------------------|--------------------------------------|------------------------------------------|
| Template Authoring               | Liquid templates                    | Liquid templates + IG package integration |
| Terminology Translation          | Static mapping HL7 v2               | FHIR Terminology Service (Ontoserver) + static mapping JSON and XML |
| Dependency Management            | Local JSON mappings                 | Declarative via `flc-config.yaml` (to do) + automated Terminology checks |
| Package Model                    | Standalone templates                | Templates as part of IG FHIR Packages (npm) |
| Target Audience                  | Azure-integrated environments       | Vendor-neutral, IG-centered environments (regions, hospitals, public sector) |

---

## Deployment

The converter logic is designed for flexible integration:

- Use as a library within your .NET projects.
- Integrate with existing CI/CD pipelines.
- Optional containerization for hosted services (no vendor lock-in).

---

## Security

For security information and how to report vulnerabilities, please see [SECURITY.md](SECURITY.md).

---

## External Resources

- [DotLiquid wiki](https://github.com/dotliquid/dotliquid/wiki)
- [Liquid wiki](https://github.com/Shopify/liquid/wiki)
- [HL7 Community 2-To-FHIR Project](https://confluence.hl7.org/display/OO/2-To-FHIR+Project)

---

## Contributing

Contributions and suggestions are welcome!

Please submit pull requests or issues via GitHub. All contributions should respect the project's license and code of conduct.

See [CONTRIBUTING.md](CONTRIBUTING.md) for more details.

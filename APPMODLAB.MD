---
title: "Spec-Driven API Modernization"
description: "Modernize SOAP/REST APIs to unified OpenAPI with CargoLink shipping - WSDL to REST transformation"
authors: ["marconsilva"]
category: "Spec-Driven Development"
industry: "Cross-Industry"
services: ["Azure API Management", "Azure App Service", "Azure SQL Database"]
languages: [".NET", "C#"]
frameworks: ["WCF", "ASP.NET Core Web API"]
modernizationTools: ["Spec2Cloud"]
agenticTools: []
tags: ["api-modernization", "soap-to-rest", "openapi", "wcf", "api-management"]
extensions: ["github.copilot"]
thumbnail: ""
video: ""
version: "1.0.0"
screenshots:
  - path: "assets/screenshots/01-rest-api-endpoints.md"
    caption: "Legacy REST API — live endpoint responses on port 5800"
  - path: "assets/screenshots/02-legacy-code-structure.md"
    caption: "Solution structure and SOAP vs REST inconsistencies"
  - path: "assets/screenshots/03-soap-service-contracts.md"
    caption: "WCF SOAP service contracts and data contracts"
  - path: "assets/screenshots/openapi-v1-generated.json"
    caption: "Auto-generated OpenAPI spec from running API"
  - path: "openapi-legacy-rest.json"
    caption: "Reverse-engineered OpenAPI 3.0.3 spec from legacy code"
---

## Legacy Application Overview

The CargoLink shipping platform exposes both SOAP (WCF) and REST (ASP.NET Core Web API) endpoints with inconsistent contracts and no unified API specification. The screenshots below document the legacy API surface, code structure, and service contracts.

### REST API Endpoints

The legacy REST API runs on port 5800, exposing shipping, tracking, and rate endpoints with inconsistent naming conventions and response formats.

[View: REST API Endpoints](assets/screenshots/01-rest-api-endpoints.md)

*Open the Markdown file to view the live endpoint responses and request/response examples.*

### Legacy Code Structure

The solution contains three projects — Core, RestApi, and SoapServices — with duplicated business logic across the SOAP and REST layers.

[View: Legacy Code Structure](assets/screenshots/02-legacy-code-structure.md)

*Open the Markdown file to view the full solution structure and SOAP vs REST inconsistencies.*

### SOAP Service Contracts

WCF service contracts define the SOAP interface with `[ServiceContract]` and `[DataContract]` attributes — these must be mapped to equivalent OpenAPI schemas during modernization.

[View: SOAP Service Contracts](assets/screenshots/03-soap-service-contracts.md)

*Open the Markdown file to view the complete WCF contract definitions.*

### Auto-Generated OpenAPI Specification

The OpenAPI spec was auto-generated from the running legacy REST API, capturing the current endpoint surface as a baseline for modernization.

[View: OpenAPI v1 Spec (Generated)](assets/screenshots/openapi-v1-generated.json)

*Open the JSON file to view the auto-generated OpenAPI 3.0 specification.*

### Reverse-Engineered OpenAPI Specification

A manually reverse-engineered OpenAPI 3.0.3 spec that unifies both SOAP and REST endpoints into a single coherent API contract.

[View: OpenAPI Legacy REST Spec](openapi-legacy-rest.json)

*Open the JSON file to view the unified OpenAPI specification.*

---

## Solution Walkthrough

This section documents the exact GitHub Copilot CLI commands used to modernize the CargoLink SOAP/REST APIs into a unified ASP.NET Core 8.0 Web API, driven by an auto-generated OpenAPI specification. Each step was executed on the `solution-final` branch with corresponding git tags.

### Prerequisites

```powershell
# Refresh PATH
$env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

# Create branch and output directory
cd C:\code\gbb\labs\appmodlab-spec-driven-api-modernization
git checkout main && git checkout -b solution-final
New-Item -ItemType Directory -Force -Path assets/outputs
```

### Step 01 — Explore Legacy API

**Tag:** `step-01-explore-legacy-api`

```powershell
gh copilot -- -p "Analyze the CargoLink legacy WCF SOAP and REST API codebase in this repository. Examine all three projects: CargoLink.SoapServices (WCF SOAP services with ServiceContract/DataContract attributes), CargoLink.RestApi (ASP.NET Web API controllers), and CargoLink.Core (shared business logic). Document: 1) All SOAP service contracts (IShipmentService, ITrackingService, IRateService) with their operations, 2) All REST endpoints (ShipmentController, TrackingController, QuoteController) with routes and HTTP methods, 3) All data contracts and DTOs, 4) Inconsistencies between SOAP and REST naming/structure. Also try building the solution with MSBuild at 'C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe' and NuGet restore with C:\Tools\nuget\nuget.exe. Write a comprehensive analysis report to assets/outputs/step-01-explore-legacy-api.md" --allow-all-tools --yolo *>&1 | Out-File -FilePath "assets\outputs\step-01-explore-legacy-api.txt" -Encoding utf8

git add -A && git commit -m "Step 01: Explore Legacy API"
git tag step-01-explore-legacy-api
```

**Output:** `assets/outputs/step-01-explore-legacy-api.md` — Full analysis of 3 WCF SOAP services, 3 REST controllers, all DataContracts/DTOs, and naming inconsistencies.

### Step 02 — Generate API Specification

**Tag:** `step-02-generate-api-spec`

```powershell
gh copilot -- -p "Reverse-engineer the CargoLink WCF service contracts and REST API controllers to generate a comprehensive unified OpenAPI 3.0 specification. Analyze the SOAP services in CargoLink.SoapServices/ and REST controllers in CargoLink.RestApi/Controllers/ and all DataContracts and DTOs. Generate a complete OpenAPI 3.0.3 JSON spec file at assets/outputs/openapi-unified-v2.json that covers all operations from both SOAP and REST, includes proper request/response schemas, adds error responses (400, 404, 500), uses RESTful naming conventions, and includes descriptions. Also write a mapping document to assets/outputs/step-02-generate-api-spec.md explaining the SOAP-to-REST mapping decisions." --allow-all-tools --yolo *>&1 | Out-File -FilePath "assets\outputs\step-02-generate-api-spec.txt" -Encoding utf8

git add -A && git commit -m "Step 02: Generate API Specification"
git tag step-02-generate-api-spec
```

**Output:** `assets/outputs/openapi-unified-v2.json` — Unified OpenAPI 3.0.3 spec; `assets/outputs/step-02-generate-api-spec.md` — SOAP-to-REST mapping decisions.

### Step 03 — Analyze API Patterns

**Tag:** `step-03-analyze-patterns`

```powershell
gh copilot -- -p "You are in the CargoLink shipping API modernization repo. Analyze the API patterns in this codebase. Read the WCF SOAP services in CargoLink.SoapServices/ and REST controllers in CargoLink.RestApi/Controllers/ and the shared core in CargoLink.Core/. Then create a file at assets/outputs/step-03-analyze-patterns.md documenting: 1) SOAP-to-REST mapping patterns showing how each WCF OperationContract maps to REST HTTP methods and routes, 2) Data contract differences between WCF DataContracts and REST DTOs, 3) Error handling gaps, 4) Business logic duplication, 5) Naming inconsistencies. Include a recommendations table for modernization." --allow-all-tools --yolo *>&1 | Out-File -FilePath "assets\outputs\step-03-analyze-patterns.txt" -Encoding utf8

git add -A && git commit -m "Step 03: Analyze API Patterns"
git tag step-03-analyze-patterns
```

**Output:** `assets/outputs/step-03-analyze-patterns.md` — Pattern analysis with SOAP→REST mapping table, data contract diffs, and modernization recommendations.

### Step 04 — Design Modern API

**Tag:** `step-04-design-modern-api`

```powershell
gh copilot -- -p "You are in the CargoLink shipping API modernization repo. Based on the legacy SOAP/REST analysis and the unified OpenAPI spec at assets/outputs/openapi-unified-v2.json, design the modern ASP.NET Core 8.0 Web API. Create a design document at assets/outputs/step-04-design-modern-api.md that includes: 1) Modern project structure, 2) RESTful route design, 3) DTO design unifying SOAP DataContracts and REST models, 4) Service layer interfaces, 5) Middleware design, 6) Dependency injection configuration, 7) API versioning strategy. Include C# code snippets for key interfaces and DTOs." --allow-all-tools --yolo *>&1 | Out-File -FilePath "assets\outputs\step-04-design-modern-api.txt" -Encoding utf8

git add -A && git commit -m "Step 04: Design Modern API"
git tag step-04-design-modern-api
```

**Output:** `assets/outputs/step-04-design-modern-api.md` — Complete API design with project structure, routes, DTOs, service interfaces, and middleware specs.

### Step 05 — Generate API Scaffold

**Tag:** `step-05-generate-scaffold`

```powershell
gh copilot -- -p "You are in the CargoLink shipping API modernization repo. Scaffold a new modern ASP.NET Core 8.0 Web API project. Create the project at CargoLink.ModernApi/ using 'dotnet new webapi'. The project should have Controllers/, Models/, Services/, Middleware/ folders with stub files for all endpoints. Add it to the solution file. Write summary to assets/outputs/step-05-generate-scaffold.md." --allow-all-tools --yolo *>&1 | Out-File -FilePath "assets\outputs\step-05-generate-scaffold.txt" -Encoding utf8

git add -A && git commit -m "Step 05: Generate API Scaffold"
git tag step-05-generate-scaffold
```

**Output:** `CargoLink.ModernApi/` project with Controllers, Models, Services, Middleware folders; `assets/outputs/step-05-generate-scaffold.md`.

### Step 06 — Implement Endpoints

**Tag:** `step-06-implement-endpoints`

```powershell
gh copilot -- -p "You are in the CargoLink shipping API modernization repo. The modern API scaffold exists at CargoLink.ModernApi/. Now implement the key endpoints with full business logic ported from the legacy code. Read the legacy services and implement ShipmentService, TrackingService, RateService with business logic matching legacy calculations. Update controllers with proper HTTP status codes and validation. Implement ErrorHandlingMiddleware. Make sure dotnet build succeeds. Write summary to assets/outputs/step-06-implement-endpoints.md." --allow-all-tools --yolo *>&1 | Out-File -FilePath "assets\outputs\step-06-implement-endpoints.txt" -Encoding utf8

git add -A && git commit -m "Step 06: Implement Endpoints"
git tag step-06-implement-endpoints
```

**Output:** Fully implemented services with shipping cost calculation, tracking events, rate tiers; `assets/outputs/step-06-implement-endpoints.md`.

### Step 07 — Add API Versioning & Docs

**Tag:** `step-07-versioning-docs`

```powershell
gh copilot -- -p "You are in the CargoLink API modernization repo. The modern ASP.NET Core 8.0 API project is at CargoLink.ModernApi/. Add API versioning and Swagger documentation. Add Asp.Versioning.Mvc and Asp.Versioning.Mvc.ApiExplorer NuGet packages. Configure URL segment versioning and Swagger with OpenApiInfo. Add XML comments and ProducesResponseType attributes. Ensure dotnet build succeeds. Write summary to assets/outputs/step-07-versioning-docs.md." --allow-all-tools --yolo *>&1 | Out-File -FilePath "assets\outputs\step-07-versioning-docs.txt" -Encoding utf8

git add -A && git commit -m "Step 07: Add API Versioning and Docs"
git tag step-07-versioning-docs
```

**Output:** Versioned API with `[ApiVersion]` attributes, Swagger UI, XML docs; `assets/outputs/step-07-versioning-docs.md`.

### Step 08 — Validate Against Spec

**Tag:** `step-08-validate`

```powershell
gh copilot -- -p "You are in the CargoLink API modernization repo. Validate the modern API implementation matches the OpenAPI spec. Read assets/outputs/openapi-unified-v2.json and compare against CargoLink.ModernApi controllers and models. Run dotnet build. Create assets/outputs/step-08-validate.md with a compliance matrix." --allow-all-tools --yolo *>&1 | Out-File -FilePath "assets\outputs\step-08-validate.txt" -Encoding utf8

git add -A && git commit -m "Step 08: Validate Against Spec"
git tag step-08-validate
```

**Output:** `assets/outputs/step-08-validate.md` — Compliance matrix showing all endpoints implemented with schema validation results.

### Push Solution

```powershell
git push origin solution-final --tags
```

### Summary

| Step | Tag | Description |
|------|-----|-------------|
| 01 | `step-01-explore-legacy-api` | Analyzed 3 WCF SOAP services + 3 REST controllers |
| 02 | `step-02-generate-api-spec` | Generated unified OpenAPI 3.0.3 spec from legacy contracts |
| 03 | `step-03-analyze-patterns` | Documented SOAP→REST patterns and inconsistencies |
| 04 | `step-04-design-modern-api` | Designed modern ASP.NET Core 8.0 API architecture |
| 05 | `step-05-generate-scaffold` | Scaffolded CargoLink.ModernApi project with all layers |
| 06 | `step-06-implement-endpoints` | Implemented all endpoints with ported business logic |
| 07 | `step-07-versioning-docs` | Added API versioning and Swagger/OpenAPI documentation |
| 08 | `step-08-validate` | Validated implementation against generated OpenAPI spec |

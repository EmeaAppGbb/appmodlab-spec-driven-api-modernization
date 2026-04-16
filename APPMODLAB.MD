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

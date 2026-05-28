# ForecastFlow Architecture Review & Diagrams

This directory contains the results of the infrastructure verification and the updated High-Level and Low-Level architecture diagrams.

## 📄 Verification Report
- [VERIFICATION_REPORT.md](VERIFICATION_REPORT.md) - Detailed review of alignment, security, scalability, and cost.

## 🖼️ Architecture Diagrams

### High-Level Diagrams
These diagrams show the system components and their interactions in different environments.

| Environment | Diagram |
|-------------|---------|
| **Current (Mock/Local)** | [High-Level Mock SVG](diagrams/high_level_mock.svg) |
| **Production (AWS)** | [High-Level Production SVG](diagrams/high_level_production.svg) |

### Low-Level Diagrams
These diagrams show the internal class and module relationships.

| Layer | Diagram |
|-------|---------|
| **Backend (.NET)** | [Low-Level Backend SVG](diagrams/low_level_backend.svg) |
| **Frontend (Angular)** | [Low-Level Frontend SVG](diagrams/low_level_frontend.svg) |

---
*Note: Mermaid source files (.mmd) are available in the [diagrams/](diagrams/) folder.*

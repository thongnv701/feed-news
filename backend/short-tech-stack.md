Here is the tech stack specification for your `feed-news` service. I've structured this as a clean Markdown file that you can feed directly into your LLM agent or project documentation.

---

# Tech Stack Specification: feed-news (C# Edition)

**Project Goal:** A high-performance, resource-efficient backend service that fetches news from Reuters and VNExpress, generates AI summaries via Gemini, and delivers them to Slack.

## 1. Core Runtime & Language

* **Framework:** `.NET 10 (LTS)`
* **Purpose:** Provides the latest performance optimizations and native support for high-concurrency tasks.


## 2. AI Orchestration & Models

* **Orchestrator:** `Microsoft.SemanticKernel`
* **Purpose:** The professional .NET alternative to LangChain. Manages the "chains," prompts, and integration with the Gemini API.


* **LLM Provider:** `Google Gemini 1.5 Flash`
* **Purpose:** Chosen for its high speed and massive context window, making it ideal for processing multiple news articles simultaneously at low cost.


* **Connector:** `Microsoft.SemanticKernel.Connectors.Google`
* **Purpose:** Official library to bridge Semantic Kernel with the Gemini API.



## 3. Data Ingestion (Scraping & Parsing)

* **RSS Parsing:** `System.ServiceModel.Syndication`
* **Purpose:** The standard .NET library for securely and efficiently parsing RSS feeds (specifically for VNExpress).


* **HTML Scraping:** `AngleSharp`
* **Purpose:** A high-performance, CSS-selector-based HTML parser used to extract content from Reuters and VNExpress pages where RSS is insufficient.


* **HTTP Client:** `IHttpClientFactory` + `Polly`
* **Purpose:** Manages connection pooling and provides **Resilience Patterns** (Retry, Circuit Breaker) if news sites are temporarily down.



## 4. Communication & Delivery

* **Slack Integration:** `SlackNet`
* **Purpose:** A comprehensive C# Slack client used to format and push Rich Text (Blocks) summaries to a specific channel.



## 5. Infrastructure & Operations

* **Project Structure:** `Clean Architecture`
* **Purpose:** Separates Domain logic (Core), External Services (Infrastructure), and the Entry Point (Worker) to ensure the code is testable and maintainable.


* **Configuration:** `Microsoft.Extensions.Configuration`
* **Purpose:** Handles environment variables and `appsettings.json` for API keys and Slack tokens.




# Votify - Core Backend & Full-Stack Architecture

A robust, secure, and scalable system developed for a digital voting platform. This project was built to ensure transparency, complex state management, and high availability during simulated voting processes, integrating modern UI/UX principles and AI capabilities.

## 🏗️ System Architecture & Engineering

The system was designed following a clean architecture pattern to ensure separation of concerns, testability, and maintainability. The core solution bridges a robust backend with a reactive frontend ecosystem:

*   **API / Web:** The presentation layer handling secured HTTP requests, exposing RESTful endpoints, and delivering a reactive user interface (Blazor).
*   **Business Logic:** Contains the core functionality, service layers (including AI integrations), and complex state machines governing the voting rules.
*   **Domain:** The heart of the software, defining core entities, interfaces, and domain exceptions without external dependencies.
*   **Persistence:** Responsible for data access, implementing patterns to interact with the underlying relational database.

### Tech Stack
*   **Language & Framework:** C#, .NET, Blazor
*   **Architecture:** N-Tier, REST API
*   **UI/UX:** CSS, Global Components, Markdown Parsing
*   **Integration:** LLM APIs (Artificial Intelligence)

---

## 👨‍💻 My Technical Contributions

This software was developed collaboratively by an engineering team. Within this cross-functional environment, my specific responsibilities and architectural implementations included:

### 🧠 AI Integration & Service Architecture
*   **AI-Driven Roadmap Generator:** Engineered a dedicated backend service to asynchronously query LLM APIs, dynamically constructing system prompts using historical user data (scores and feedback).
*   **Full-Stack Implementation:** Built secured endpoints to serve the AI responses and developed a reactive frontend with state management (loading/success/error states) and secure Markdown-to-HTML parsing.

### 🛡️ Security & Access Control
*   **Dynamic Role-Based Access:** Designed a complex authorization system segregating Public Voting from Authenticated Jury access, synchronizing UI toggles with backend DTOs.
*   **Data Isolation:** Implemented strict backend filtering for the Jury Dashboard to guarantee data privacy, ensuring unauthorized requests receive a `403 Forbidden` response.

### ⚙️ Core Backend & State Management
*   **Lifecycle State Machine:** Developed the manual execution logic for voting events (Start, Stop, Resume, Finish), strictly enforcing valid state transitions, dynamic timestamp adjustments (`DateTime.UtcNow`), and transaction blocking during paused states.
*   **Validation & Cascading Logic:** Handled complex edge cases in data persistence, including restricted deletion protocols (preventing the unlinking of active events) and comprehensive timeline validation.

### 🎨 UI/UX Engineering (Frontend)
*   **Cognitive Load Reduction:** Redesigned complex interfaces following Nielsen's Heuristics, refactoring standard continuous-scroll views into progressive Wizard patterns with Blazor state preservation.
*   **Component Standardization:** Unanimously improved UI consistency by developing global alert components for predictable error handling and unifying CSS design tokens across the application.

---

## 🔄 Team Workflow & Agile Methodology

To maintain high code quality and coordinate effectively across the team, we adopted industry-standard agile practices:

*   **Version Control (Git):** We utilized a strict Git workflow, ensuring the `main` branch always contained production-ready code.
*   **Pull Requests & Code Reviews:** All new features and bug fixes were developed in isolated feature branches. Code was merged into the main branch exclusively via Pull Requests, requiring peer review to maintain architectural standards.

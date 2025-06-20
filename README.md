# SkillSnap

## Project Summary
SkillSnap is a web application that helps users manage, track, and improve their skills and projects. It provides a platform for users to create and update their portfolios, manage projects, and list their skills. The app is built with ASP.NET Core (API) and Blazor WebAssembly (client).

## Key Features
- **CRUD Operations:** Full create, read, update, and delete support for Projects, Skills, and Portfolio Users.
- **Security:** JWT-based authentication and role-based authorization. Only authenticated users can create/update, and only admins can delete.
- **Caching:** In-memory caching for API GET endpoints with cache invalidation on data changes, and cache hit/miss logging for performance verification.
- **State Management:** Scoped state container services (e.g., `UserSessionService`) for persisting user/session info across components.
- **Responsive UI:** Modern Blazor components with improved layout, spacing, and navigation.

## Development Process and Use of Copilot
Development followed an iterative, test-driven approach. GitHub Copilot was used extensively for:
- Generating CRUD controllers, Blazor pages, and service classes.
- Suggesting code structure, naming, and documentation.
- Providing code comments, helper methods, and error handling patterns.
- Improving UI layout and consistency.
- Refactoring for maintainability and performance.

## Known Issues or Future Improvements
- **Role Management:** Admin role assignment currently requires manual intervention or a special endpoint.
- **Validation:** Client-side validation can be further improved for better UX.
- **Error Handling:** More user-friendly error messages and logging could be added.
- **Testing:** Automated tests for API and UI are planned.
- **UI Enhancements:** More responsive/mobile-friendly design and theming.
- **Performance:** Further optimization for large datasets and real-time updates.

---

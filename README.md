# SkillSnap

## Project Summary
SkillSnap is a web application that helps users manage, track, and improve their skills and projects. It provides a platform for users to create and update their portfolios, manage projects, and list their skills. The app is built with ASP.NET Core (API) and Blazor WebAssembly (client).

## Key Features
- **CRUD Operations:** Full create, read, update, and delete support for Projects, Skills, and Portfolio Users.
- **Security:** JWT-based authentication and role-based authorization. Only authenticated users can create/update, and only admins can delete.
- **Caching:** In-memory caching for API GET endpoints with cache invalidation on data changes, and cache hit/miss logging for performance verification.
- **State Management:** Scoped state container services (e.g., `UserSessionService`) for persisting user/session info across components.
- **Responsive UI:** Modern Blazor components with improved layout, spacing, and navigation.

---

## App Description and Key Features

SkillSnap is designed to help users build and showcase their professional portfolios by managing projects and tracking skills. Users can register, log in, and securely manage their own portfolio, including adding, editing, and deleting projects and skills. The application features a clean, responsive interface, navigation menu, and detailed forms for CRUD operations. Admin users have additional privileges, such as deleting any portfolio user.

---

## Development Challenges

During development, several challenges were encountered, including handling authentication and authorization in a Blazor WebAssembly environment, ensuring secure API communication, and managing state across components. Implementing efficient caching and cache invalidation required careful attention to avoid stale data. Another challenge was providing a responsive and user-friendly UI while keeping the codebase maintainable and modular. Debugging authentication issues and ensuring the correct propagation of JWT tokens for protected endpoints also required significant effort.

---

## Business Logic, Data Persistence, and State Management

Business logic is primarily handled in the ASP.NET Core API controllers, which enforce validation, authorization, and data transformation (using DTOs). Data persistence is managed via Entity Framework Core, with models mapped to a relational database. On the client side, Blazor services encapsulate API calls and business rules, while state management is achieved using scoped services such as `UserSessionService` to persist user and session information across components without requiring reloads. This separation of concerns ensures maintainability and testability.

---

## Security Implementation

Security is implemented using JWT-based authentication, with tokens stored in browser localStorage and attached to API requests via Blazor service helpers. The API uses `[Authorize]` and `[Authorize(Roles = "Admin")]` attributes to restrict access to sensitive endpoints. Only authenticated users can create or update resources, and only users with the "Admin" role can perform delete operations. Passwords are securely hashed, and user roles are managed through ASP.NET Identity. The application also ensures that tokens are refreshed and validated on each protected request.

---

## Performance Improvements

To optimize performance, in-memory caching is used for API GET endpoints, significantly reducing database load and improving response times for frequently accessed data. Cache invalidation is triggered automatically after any create, update, or delete operation to ensure users always see fresh data. Cache hit/miss events are logged for monitoring and tuning. On the client side, local state caching is used to minimize unnecessary API calls and improve UI responsiveness. The UI layout and component rendering have also been optimized for fast load times and smooth navigation.

---

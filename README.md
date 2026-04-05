# SunnyEast / Солнечный восток

## 🌍 Overview
SunnyEast is a full-stack commerce and operations platform built for a multi-store retail business. It combines a customer-facing storefront, an internal staff workspace, and an admin back office in one system.

This project is used for a real business and is designed as a production-ready system, not as a demo-only showcase. It covers the full product flow: authentication, role-based access, catalog management, carts, checkout, pickup orders, real-time status updates, push notifications, staff management, and database backup/restore for operational safety.


## 🔗 Live Project
Website: https://solnechny-vostok.ru/

SunnyEast is live and publicly accessible. The repository reflects the actual application structure behind the running product.


## ✨ Highlights
- Production-oriented full-stack application for a real retail business.
- Public storefront and internal business tooling in one system.
- Role-based workflows for customers, staff, administrators, and super admin.
- Real-time order updates, push notifications, and operational recovery tools.
- Clear layered architecture with separate domain, application, infrastructure, API, and client projects.


## 👥 Roles In The System
| Role | What they can do |
| --- | --- |
| Customer | Browse categories and products, manage cart, choose a pickup shop, place orders, view order history, manage account data, and receive order status notifications. |
| Salesman | Work with orders assigned to their shop, receive new orders in real time, update order statuses, and archive completed or canceled orders. |
| Administrator | Manage catalog data, work with categories and products, handle operational order flow, and use media upload tools for product content. |
| Super Admin | Has full control over the system: shops, users, staff, assignments, business data, and database backup/restore operations. |


## 🧩 Product Capabilities
- Multi-store storefront with shop-specific order routing.
- Category and product management with discounts, images, and volume-based pricing.
- Cart and checkout flow with pickup store selection.
- Order lifecycle management: `Submitted -> Ready -> Issued / Canceled -> Archived`.
- Role-based UI and API authorization.
- JWT authentication with ASP.NET Identity.
- Email and SMS verification flows with cooldowns and quota control.
- Account management with contact linking and password reset.
- Real-time order updates via SignalR.
- Web Push notifications for staff and customers.
- PWA support with service worker and installable client behavior.
- Media upload workflow for product images.
- Full MySQL database snapshot and restore flow with automatic pre-restore backup.


## 🛍️ What The Platform Actually Does
From the user's side, SunnyEast works like a modern pickup-based online store: customers explore products, add items to cart, choose a shop, place an order, and follow its status.

From the business side, it behaves like a lightweight ERP/back-office system: staff members process incoming orders, administrators maintain catalog data, and the super admin manages users, stores, permissions, and recovery operations.

That combination is the strongest part of the project. It is both a storefront and an internal business system.


## 🔔 Real-Time And Operational Features
- New orders are pushed instantly to the relevant staff members and to the customer.
- Staff dashboards react to order changes without manual refresh.
- Push notifications keep users informed outside the active browser tab.
- Database restore is protected by an automatic backup step before import.
- Backup rotation and restore flow show operational thinking, not just feature coding.


## 🧠 Interesting Technical Details
- Volume-based pricing is calculated from category-defined units such as grams, kilograms, milliliters, liters, and pieces.
- Staff access is scoped by role and by shop assignment, which is closer to a real business model than a global admin panel.
- SignalR authentication supports JWT tokens through query-based hub access.
- The project includes contact verification and account merge-like behavior when linking email or phone data.
- Startup applies migrations and seeds base roles automatically.


## 🏗️ Architecture
The solution is organized in a layered structure inspired by Clean Architecture principles:

```text
src/
  Domain                -> entities and enums
  Application           -> business logic, handlers, mapping, validation
  Application.Contract  -> DTOs, commands, queries, responses
  Infrastructure        -> EF Core, identity, external services, persistence
  WebApi                -> controllers, auth, SignalR hub, API composition
  Client                -> Blazor WebAssembly UI
  Client.Infrastructure -> client services, auth state, HTTP, realtime, preferences
```


This separation makes the project easier to reason about and shows clear boundaries between domain logic, infrastructure, API surface, and UI.

## 🛠️ Tech Stack
- `ASP.NET Core` Web API
- `Blazor WebAssembly`
- `.NET 9`
- `Entity Framework Core`
- `MySQL`
- `ASP.NET Identity`
- `JWT authentication`
- `SignalR`
- `MudBlazor`
- `MediatR`
- `AutoMapper`
- `FluentValidation`
- `MailKit`
- `Web Push`
- `Docker`
- `GitHub Actions`


## 🚀 Local Setup
### 1. Configure backend settings
Set the required values in `src/WebApi/appsettings.json` or in environment variables:
- `ConnectionStrings:mySql`
- `JWT:Secret`
- email settings
- SMS provider settings
- VAPID keys for push notifications
- optional backup settings

### 2. Configure client settings
Check `src/Client/Client/wwwroot/appsettings.json`:
- API base URL
- CDN base URL if you use image upload flow
- push notification public key

### 3. Run the backend
```bash
dotnet run --project src/WebApi/WebApi.csproj
```

### 4. Run the client
```bash
dotnet run --project src/Client/Client.csproj
```

On startup, the backend applies pending migrations and seeds base roles. In development mode, it also creates a seeded `SuperAdmin`.


## 🔐 Security And Access Model
- Protected API endpoints use role-based authorization.
- The client stores and attaches JWT access tokens for authenticated requests.
- SignalR order updates also work with authenticated token-based access.
- Sensitive database operations are restricted to `SuperAdmin`.

## 📦 Delivery And DevOps
- A GitHub Actions workflow builds the solution on pull requests.
- A production workflow versions releases, builds Docker images for API and client, and pushes them to Docker Hub.
- The repository includes separate container build flows for backend and frontend delivery.

## 📈 What This Project Demonstrates About Me
- I can build more than pages and endpoints. I can model a real workflow.
- I can connect UX, business rules, API design, persistence, and infrastructure into one coherent product.
- I understand authorization, role separation, operational tooling, and real-time interaction.
- I can structure a medium-sized .NET solution in a way that remains navigable and scalable.

## 📌 Final Note
SunnyEast is a real-world product built around actual business workflows: selling products, managing orders, coordinating staff, and supporting daily operations across multiple stores.

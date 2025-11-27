# The Gadget Hub

**The Gadget Hub** is a robust web application built with **ASP.NET Core** designed to manage and showcase electronic gadgets and tech accessories. It serves as a digital platform for browsing products, managing inventory, and facilitating e-commerce interactions.

## 🚀 Features

* **Product Catalog:** Browse a wide range of gadgets with detailed descriptions and specifications.
* **Category Management:** Organize products into distinct categories for easy navigation.
* **User Authentication:** Secure login and registration system (ASP.NET Core Identity).
* **Shopping Cart:** Functionality for users to add items and review their selections.
* **Admin Dashboard:** Backend interface for administrators to manage products, categories, and orders.
* **Responsive Design:** Optimized for mobile, tablet, and desktop devices using Bootstrap.
* **Search Functionality:** Quickly find gadgets by name or tag.

## 🛠️ Technology Stack

* **Framework:** [ASP.NET Core](https://dotnet.microsoft.com/apps/aspnet) (MVC Pattern)
* **Language:** C#
* **Frontend:** HTML, CSS, [Bootstrap](https://getbootstrap.com/), [jQuery](https://jquery.com/)
* **Database:** Microsoft SQL Server
* **ORM:** Entity Framework Core
* **IDE:** Visual Studio / VS Code

## 📂 Project Structure

The solution is organized as follows:

```text
GadgetHubSolution/
├── GadgetHubWeb/           # Main Web Application Project
│   ├── Controllers/        # MVC Controllers
│   ├── Models/             # Data Models
│   ├── Views/              # Razor Views
│   ├── wwwroot/            # Static files (CSS, JS, Images)
│   ├── appsettings.json    # Configuration settings
│   └── Program.cs          # Application entry point
└── .vs/                    # Visual Studio configuration

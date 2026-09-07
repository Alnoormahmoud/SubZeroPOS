# 🧊 SubZero POS — نظام ساب زيرو لنقاط البيع

<p align="center">
  <img src="docs/images/logo.jpg" alt="SubZero POS Logo" width="180">
</p>

<h2 align="center">نظام نقاط بيع وإدارة المبيعات والمخزون</h2>

<p align="center">
  A modern desktop Point of Sale and Inventory Management System
</p>

<p align="center">
  <img src="https://img.shields.io/badge/C%23-.NET-blue" alt="C#">
  <img src="https://img.shields.io/badge/UI-WPF-purple" alt="WPF">
  <img src="https://img.shields.io/badge/Database-SQL%20Server-red" alt="SQL Server">
  <img src="https://img.shields.io/badge/ORM-EF%20Core-green" alt="Entity Framework Core">
  <img src="https://img.shields.io/badge/Platform-Windows-lightgrey" alt="Windows">
</p>

---

## 📌 About SubZero POS

SubZero POS is a desktop Point of Sale and business management system designed to help businesses manage their daily operations from one centralized application.

The system provides functionality for:

- Point of Sale
- Products
- Categories
- Inventory
- Customers
- Orders
- Invoices
- Receipts
- Users
- Roles
- Shifts
- Application Settings
- Database Backups

SubZero POS was developed as a real-world production application and has been delivered to real clients.

This public repository is intended to demonstrate the project, its architecture, technologies, features, and implementation.

---

## ⚠️ Important Note

SubZero POS is not only a demo or academic project.

The application has been developed and delivered to real clients for actual business use.

However, if you are a developer, student, recruiter, or anyone interested in testing the application, you are welcome to download and evaluate it.

The installer provides a:

### 🎁 14-Day Free Trial

The 14-day free trial allows users to explore and test the application before deciding whether they want to continue using the full production version.

During the trial, you can explore:

- POS workflow
- Product management
- Inventory
- Customers
- Orders
- Invoices
- Printing
- PDF generation
- Settings
- Shifts
- Database backups

After the trial period, users who want to continue using the full version should contact the developer.

📱 WhatsApp: +256 706 819705

---

# ✨ Features

## 🛒 Point of Sale

- Create sales orders
- Add products to orders
- Change product quantities
- Remove products
- Calculate order totals
- Apply discounts
- Add delivery fees
- Select customers
- Generate order numbers
- Complete sales quickly

---

## 📦 Inventory Management

- Add products
- Edit products
- Delete products
- Manage categories
- Track product quantities
- Search products
- Product images
- Automatic stock updates after sales
- Inventory organization

---

## 👥 Customer Management

- Add customers
- Edit customers
- Search customers
- Customer information
- Customer order history
- Customer information on invoices

---

## 🧾 Invoices & Receipts

The system provides configurable invoices and receipts.

Features include:

- Invoice generation
- Receipt printing
- PDF generation
- Automatic printing
- Open PDF after printing
- Configurable receipt width
- Currency configuration
- Logo display
- Customer name display
- Cashier name display
- Order number display

---

## 👤 User Management

The system includes user authentication and role management.

Features include:

- User login
- User accounts
- Role-based access
- Manager role
- Password hashing
- Session management

---

## 🕐 Shift Management

The POS includes shift management functionality.

Features include:

- Open shift
- Close shift
- Track sales during a shift
- Shift totals
- Cash management
- Shift closing records

---

## 💾 Database Backup

The system includes database backup functionality.

Features include:

- Database backups
- Backup storage
- Configurable backup settings
- Weekly backup option
- Backup management

---

## ⚙️ Application Settings

The application provides configurable settings for:

- Currency
- Currency symbol
- Date format
- Invoice settings
- Receipt width
- Automatic printing
- PDF behavior
- Logo visibility
- Customer name
- Cashier name
- Order number
- Backup settings

---

# 🌍 Arabic & RTL Support

SubZero POS was designed with Arabic-speaking businesses in mind.

The interface supports:

- Arabic user interface
- Right-to-left layout
- Arabic labels
- Arabic invoices
- Arabic date and time presentation
- Arabic business workflow
- Configurable currencies

The system can be adapted for different markets and currencies.

---

# 🧰 Technology Stack

## Programming Language

- C#

## Framework

- .NET
- Windows Presentation Foundation (WPF)

## User Interface

- WPF
- XAML
- Data Binding
- Value Converters
- RTL / Arabic UI

## Architecture

- MVVM
- Layered Architecture
- SOLID Principles
- Dependency Injection
- Separation of Concerns

## Database

- Microsoft SQL Server
- SQL Server Express
- Entity Framework Core
- EF Core Migrations
- Microsoft.Data.SqlClient

## Libraries

- CommunityToolkit.Mvvm
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Hosting
- Entity Framework Core

## Installer

- Inno Setup
- Automatic SQL Server Express detection
- Automatic SQL Server Express installation
- Application publishing
- Desktop shortcut creation
- 14-day trial information

## Development Tools

- Visual Studio
- SQL Server Management Studio
- Git
- GitHub

---

# 🏗️ Application Architecture

The project follows a layered architecture with a clear separation of responsibilities.

### Core Layer

The Core layer contains:

- Entities
- DTOs
- Interfaces
- Core application models
- Business contracts

### Data Layer

The Data layer is responsible for:

- Database access
- Entity Framework Core
- SQL Server
- Migrations
- Data services
- Data persistence

### WPF Layer

The WPF layer is responsible for:

- User interface
- Views
- ViewModels
- MVVM
- Navigation
- User interaction
- UI resources

The overall structure is:

SubZeroPOS
│
├── SubZeroPOS.Core
│   ├── Entities
│   ├── DTOs
│   └── Interfaces
│
├── SubZeroPOS.Data
│   ├── DbContext
│   ├── Services
│   ├── Migrations
│   └── Database Configuration
│
└── SubZeroPOS.WPF
    ├── Views
    ├── ViewModels
    ├── Converters
    ├── Session
    └── UI Resources

---

# 💻 System Requirements

Before installing SubZero POS, make sure the computer meets the following requirements.

## Operating System

- Windows 10 64-bit
- Windows 11 64-bit

## Recommended Hardware

- 4 GB RAM or more
- At least 2 GB available disk space
- Intel or AMD processor
- Screen resolution of 1280 × 720 or higher

## Software

The installer handles the main application dependencies.

The application uses:

- .NET
- Microsoft SQL Server Express

If SQL Server Express is not already installed, the installer can install the required SQL Server Express instance automatically.

## Internet Connection

An internet connection may be required during the first installation if SQL Server Express or another required component needs to be installed or downloaded.

After installation, the application operates locally using the installed SQL Server database.

---

# 📥 Installation Process

Installing SubZero POS is designed to be simple.

## Step 1 — Open GitHub Releases

Go to the Releases section of this GitHub repository.

Open the latest release.

For example:

SubZero POS v1.0.0

---

## Step 2 — Download the Installer

Inside the latest release, find the installer file.

For example:

SubZeroPOS_Setup_1.0.0.exe

Click the .exe file to download it.

Always download the installer from the official GitHub Release published by the developer.

---

## Step 3 — Run the Installer

After the download finishes:

1. Open the downloaded .exe file.
2. Windows may ask for administrator permission.
3. Select Yes.
4. The SubZero POS installer will open.

---

## Step 4 — Review the Trial Information

The installer displays information about the 14-day free trial.

Read the information and continue with the installation.

---

## Step 5 — Choose Installation Options

Follow the installer instructions.

The installer can create:

- Start Menu shortcut
- Desktop shortcut

Select the options you want.

---

## Step 6 — SQL Server Express Installation

The installer checks whether the required SQL Server Express instance already exists.

### If SQL Server Express is already installed

The installer continues without installing another SQL Server Express instance.

### If SQL Server Express is not installed

The installer automatically starts the SQL Server Express installation.

The SQL Server installation may take several minutes.

Do not close the installer while SQL Server is being installed.

---

## Step 7 — Finish Installation

After all required components have been installed:

1. Finish the installation.
2. Launch SubZero POS.
3. Log in.
4. Start exploring the system.

---

# 🧪 Testing the Application

If you are downloading SubZero POS only to test it, that's completely fine.

The 14-day trial is provided to allow users to evaluate the application.

A recommended testing flow is:

Login
↓
Dashboard
↓
Categories
↓
Products
↓
Inventory
↓
Customers
↓
Create Order
↓
Complete Sale
↓
Generate Invoice
↓
Print / PDF
↓
Review Shift
↓
Backup Database

---

# 🎁 14-Day Free Trial

The installer provides a 14-day free trial for evaluation.

The trial is useful for:

- Testing
- Evaluation
- Demonstrations
- Portfolio review
- Business evaluation

If you want to continue using SubZero POS after the evaluation period, contact the developer.

📱 WhatsApp: +256 706 819705

---

# 🖼️ Screenshots

Below are screenshots demonstrating different parts of SubZero POS.

## 1. Dashboard

![SubZero POS Dashboard](docs/images/01-dashboard.png)

The main dashboard provides an overview of the POS system and quick access to the main functions.

---

## 2. Point of Sale

![SubZero POS Point of Sale](docs/images/02-pos.png)

The Point of Sale screen allows users to create orders and complete sales efficiently.

---

## 3. Products

![SubZero POS Products](docs/images/03-products.png)

Product management allows users to manage products, prices, categories, quantities, and images.

---

## 4. Inventory

![SubZero POS Inventory](docs/images/04-inventory.png)

The inventory section helps businesses monitor and manage available stock.

---

## 5. Customers

![SubZero POS Customers](docs/images/05-customers.png)

Customer management provides access to customer information and order history.

---

## 6. Orders

![SubZero POS Orders](docs/images/06-orders.png)

The orders section allows users to review and manage sales orders.

---

## 7. Invoice

![SubZero POS Invoice](docs/images/07-invoice.png)

The invoice system provides printable and PDF-ready invoices.

---

## 8. Shift Management

![SubZero POS Shifts](docs/images/08-shifts.png)

Shift management allows cashiers and managers to open and close shifts and review shift totals.

---

## 9. Settings

![SubZero POS Settings](docs/images/09-settings.png)

The settings section allows the business to configure currencies, invoices, printing, backups, and other system options.

---

## 10. Login

![SubZero POS Login](docs/images/10-login.png)

The login screen provides access control for system users.

---

# 📸 Project Gallery

The screenshots demonstrate selected parts of the application, including:

- Dashboard
- Point of Sale
- Products
- Inventory
- Customers
- Orders
- Invoices
- Shifts
- Settings
- Login

These screenshots are included to provide a visual overview of the application and its user interface.

---

# 🗄️ Database

SubZero POS uses Microsoft SQL Server with Entity Framework Core for application data access.

The database manages information including:

- Users
- Roles
- Categories
- Products
- Customers
- Orders
- Order Items
- Shifts
- Settings
- Backups

---

# 🔐 Security

The application includes application-level security features such as:

- Authentication
- Role-based authorization
- Password hashing
- Session management
- Separation of application layers
- Controlled database access

Production deployments should always use appropriate security practices for the specific environment.

---

# 🖨️ Printing & PDF

The application supports configurable printing and PDF functionality.

Available options include:

- Receipt printing
- PDF invoices
- Automatic printing
- Open PDF after printing
- Configurable receipt width
- Logo display
- Customer information
- Cashier information
- Order number
- Currency

---

# 💾 Backup

Database backup functionality is included to help protect business data.

For production environments, it is recommended to keep multiple copies of important backups and store at least one copy separately from the main computer.

---

# 📁 Repository Structure

SubZeroPOS/
│
├── SubZeroPOS.Core/
│
├── SubZeroPOS.Data/
│
├── SubZeroPOS.WPF/
│
├── Images/
│
├── docs/
│   └── images/
│       ├── logo.png
│       ├── 01-dashboard.png
│       ├── 02-pos.png
│       ├── 03-products.png
│       ├── 04-inventory.png
│       ├── 05-customers.png
│       ├── 06-orders.png
│       ├── 07-invoice.png
│       ├── 08-shifts.png
│       ├── 09-settings.png
│       └── 10-login.png
│
└── README.md

---

# 🔄 Development Workflow

The project was developed through the following workflow:

Requirements
↓
Database Design
↓
Entity Models
↓
Entity Framework Core
↓
Services
↓
ViewModels
↓
WPF Views
↓
Testing
↓
Publishing
↓
Inno Setup
↓
Client Deployment

---

# 🎯 Project Goals

The main goals of SubZero POS are:

- Simplify daily sales operations
- Improve inventory management
- Reduce manual work
- Centralize business data
- Provide reliable invoices
- Support Arabic-speaking businesses
- Provide configurable business settings
- Make installation easy
- Provide a practical desktop POS solution

---

# 👨‍💻 Developer

## Alnoor Mahmoud

Software Developer / Junior Backend Developer

### Areas of Interest

- C#
- .NET
- Backend Development
- SQL Server
- Entity Framework Core
- REST APIs
- Database Design
- Software Architecture
- Desktop Applications

---

# 📞 Contact

For questions, demonstrations, production usage, or continuing after the trial:

📱 WhatsApp: +256 706 819705

---

# ⚠️ Disclaimer

SubZero POS is a real-world software application that has been delivered to real clients.

This public repository is intended for:

- Portfolio purposes
- Educational purposes
- Testing
- Evaluation
- Software engineering demonstration

Private client information, credentials, production configuration, and sensitive business data are not included in this repository.

When testing the application, do not use real customer or business-sensitive information.

---

# ⭐ Support the Project

If you find this project useful or interesting:

⭐ Star the repository

🍴 Fork the repository

💬 Share your feedback

📱 Contact the developer for more information about the production version.

---

# 📄 License

This project is provided for educational, portfolio, testing, and evaluation purposes.

Commercial redistribution or selling the application without permission from the developer is not permitted.

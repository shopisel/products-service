## 📦 Products Service

The **Products Service** is a core microservice of the Shopisel platform, responsible for managing product data aggregated from multiple retailers.

It acts as a centralized layer that stores, normalizes, and serves product information collected by external scraping services, enabling price comparison across different stores.

---

## 🚀 Purpose

This service provides a unified API to:

- Store product information (name, brand, category, etc.)
- Aggregate prices from different supermarkets
- Track price history over time
- Enable fast product search and filtering
- Support price comparison features in client applications

---

## 🧩 Role in the Architecture

Within the Shopisel ecosystem, this service works alongside:

- **Scraper Services** → Collect product and price data from retailers  
- **Products Service (this repo)** → Stores and organizes product data  
- **API Gateway / Frontend** → Consumes product data for users  

---

## 🛠️ Features

- 📦 Product catalog management  
- 💰 Multi-store price aggregation  
- 📊 Price history tracking  
- 🔎 Search and filtering capabilities  
- ⚡ RESTful API for product queries  

---

## 🏗️ Tech Stack

*(Adjust based on your actual stack)*

- .NET (ASP.NET Core)  
- Entity Framework Core  
- SQL Database (e.g., PostgreSQL / SQL Server)  
- Docker (optional)  


# SmartKitchen
SmartKitchen is a full-stack, multi-layered recipe web application built with ASP.NET Core Web APIs, MongoDB, and Blazor (Server + WASM). It supports searching recipes by name or ingredients, filtering by cuisine, diet, or cooking method, and includes AI-powered recipe generation using OpenAI.

The backend leverages MongoDB replica sets and multi-document transactions for consistency. It features custom cookie+JWT-based authentication, secure admin CRUD pages, and role-based access. The app is containerized using Docker and deployed to Azure Container Apps with Azure Blob Storage handling image uploads.

https://smart-kitchen-webapp.bravemoss-a3de7b6c.swedencentral.azurecontainerapps.io/

# Features:

- Dynamic recipe search by name or ingredients

- Filters by cuisine, diet, and cooking method

- AI-generated recipes with OpenAI

- Custom authentication & secure admin dashboard

- Azure deployment with HTTPS & blob storage

- Unit and integration testing with xUnit

# Technologies: 
C#, ASP.NET Core (Web APIs, Blazor Server, Blazor WebAssembly), MongoDB, Docker, Azure Blob Storage, Azure Container Apps, OpenAI API, xUnit, JavaScript Interop

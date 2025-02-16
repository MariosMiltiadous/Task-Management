# Task Management API - Setup Guide

## Prerequisites
Ensure you have the following installed:
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQLite (included with EF Core)

## Installation Steps

### Step 1️) Clone the Repository
Go to the folder you want to clone the repo.
Then type in cmd on your folder path: git clone <your-repository-url>
cd TaskManagement

### Step 2) Install Required Dependencies for Api project
# Entity Framework Core (SQLite) -- or from Nuget Package Manager
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design

# Caching
dotnet add package Microsoft.Extensions.Caching.Memory

# JSON Handling
dotnet add package Microsoft.AspNetCore.Mvc.NewtonsoftJson

# API Documentation
dotnet add package Swashbuckle.AspNetCore

### Step 3) Install Required Test Dependencies
dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet add package Moq
dotnet add package xunit

### Step 4) Install Bulk Update Supports
dotnet add package EFCore.BulkExtensions

### Step 5) Apply Migrations and run the Database
dotnet ef migrations add InitialCreate
dotnet ef database update

### Step 6) Run the API
dotnet run 
or through VS IDE

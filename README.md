# Smart Meal Planner

Smart Meal Planner is a fullstack microservice-based platform for recipes, meal planning, grocery lists, pantry inventory and grocery budget tracking.

## Goals
- Build a useful daily-life application
- Refresh practical .NET and Java Spring Boot skills
- Practice REST APIs, PostgreSQL, authentication and testing
- Practice Docker, CI/CD, Kubernetes and monitoring
- Create real project stories for future technical interviews

## Architecture
Frontend Dashboard
  - calls .NET Recipe & Budget API
  - calls Java Grocery & Meal Plan API
  - both services use PostgreSQL

No API Gateway/BFF is used in the first phase. It can be added later as an advanced improvement.

## Tech Stack
- Frontend: Angular or React
- .NET Backend: ASP.NET Core Web API, EF Core, xUnit
- Java Backend: Spring Boot, Spring Security, Spring Data JPA, JUnit
- Database: PostgreSQL
- Infrastructure: Docker, Docker Compose, GitHub Actions, Jenkins, Kubernetes
- Testing: Cypress, Playwright

## Main Features
- Recipe management
- Ingredient management
- Grocery list management
- Weekly meal planning
- Pantry inventory
- Expiry reminders
- Grocery budget reports
- Receipt upload

## Status
Work in progress.

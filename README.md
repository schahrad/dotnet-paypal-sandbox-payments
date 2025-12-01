# dotnet-paypal-sandbox-payments

# dotnet-paypal-sandbox-payments

A **.NET 8 API** that integrates with the **PayPal Orders v2 API (Sandbox)**.  
It demonstrates how to:

- Call external REST APIs with `HttpClient` and `async/await`
- Implement the OAuth2 client credentials flow (PayPal access token)
- Use strongly typed DTOs for JSON (request/response)
- Structure a small backend with **clean architecture**
- Write unit tests against external APIs using a mocked `HttpMessageHandler`

> ⚠️ This project is for learning and demonstration purposes only.  
> It uses **PayPal Sandbox** and is **not production-ready**.

---

## Contents

- [Features](#features)
- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Clone the repository](#clone-the-repository)
  - [Configure PayPal sandbox credentials](#configure-paypal-sandbox-credentials)
- [Running the API](#running-the-api)
- [API Endpoints](#api-endpoints)
  - [Create payment (POST /api/payments)](#create-payment-post-apipayments)
  - [Get payment (GET /api/paymentsid)](#get-payment-get-apipaymentsid)
  - [Capture payment (POST /api/paymentsidcapture)](#capture-payment-post-apipaymentsidcapture)
- [Testing](#testing)
- [What this project demonstrates](#what-this-project-demonstrates)
- [Next ideas & extensions](#next-ideas--extensions)
- [Disclaimer](#disclaimer)

---

## Features

- Create a **PayPal Order** (intent `CAPTURE`) in **sandbox** mode
- Retrieve order details by ID
- Capture an order server-side (demo of a typical two-step payment flow)
- Clear separation of:
  - Domain model
  - Application/use cases
  - Infrastructure (PayPal client)
  - API (minimal API)
- Error handling for:
  - OAuth token failures
  - PayPal HTTP errors (4xx, 5xx)
  - Mapping and deserialization errors
- Unit tests that:
  - Mock `HttpClient` to simulate PayPal responses
  - Validate mapping and error handling without real network calls

---

## Architecture

The solution is split into four projects:

```text
PayPalSandboxPayments.sln
  src/
    PayPalSandbox.Core/               # Domain model, IPaymentGateway abstraction
    PayPalSandbox.Application/        # Use cases (CreatePayment, GetPayment, CapturePayment)
    PayPalSandbox.Infrastructure.PayPal/  # PayPalPaymentGateway using HttpClient
    PayPalSandbox.Api/                # .NET 8 minimal API (presentation layer)
  tests/
    PayPalSandbox.Tests/              # Unit tests (xUnit + FluentAssertions)

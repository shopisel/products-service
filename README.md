# Product Service

The `products-service` manages categories and products in the Shopisel ecosystem.

## Responsibilities

- Manage product categories
- Create, update and delete products
- Query products by ids, category or name
- Enforce category integrity (category delete blocked when products exist)

## API Overview

Category endpoints:

- `GET /categories`
- `POST /categories`
- `PUT /categories/{categoryId}`
- `DELETE /categories/{categoryId}`

Product endpoints:

- `GET /products?ids=prod_1,prod_2`
- `GET /products?categoryId=cat_food`
- `GET /products?name=leite`
- `POST /products`
- `PUT /products/{productId}`
- `DELETE /products/{productId}`

`GET /products` requires at least one filter (`ids`, `categoryId`, or `name`).

## Authentication

This service does not require authentication.

## Project Structure

```text
src/
|-- ProductService/
|-- ProductService.Tests/
`-- ProductService.slnx
```

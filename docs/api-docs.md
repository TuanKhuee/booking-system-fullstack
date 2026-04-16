# 📘 Hotel Booking System — API Documentation

## Base URLs

| Service      | URL                                    |
|--------------|----------------------------------------|
| Auth Service | `https://localhost:{port_authservice}` |
| Room Service | `https://localhost:{port_roomservice}` |

## 🔐 Authentication

API uses **JWT Bearer Token**.

```
Authorization: Bearer <access_token>
```

JWT is issued by Auth Service. Access token expires in **15 minutes**.  
Some endpoints require the **Admin** role.

---

## 📑 Table of Contents

- [Auth Service](#-auth-service)
- [Room Service](#-room-service)
  - [Rooms API](#rooms-api)
  - [Room Types API](#room-types-api)
- [Error Response Format](#-error-response-format)
- [Data Models](#-data-models)

---

## 🔐 Auth Service

**Base Path:** `/api/auth`

| Method | Endpoint              | Auth | Status      | Description              |
|--------|-----------------------|------|-------------|--------------------------|
| POST   | `/api/auth/register`  | no   | ✅ Active   | Register a new account   |
| POST   | `/api/auth/login`     | no   | ✅ Active   | Login and receive JWT    |
| POST   | `/api/auth/refresh`   | no   | 🚧 Planned  | Refresh access token     |
| POST   | `/api/auth/logout`    | yes  | 🚧 Planned  | Logout, revoke token     |

---

#### `POST /api/auth/register`

Creates a new user account. Default role is `User`.

**Auth:** No

**Request Body**
```json
{
  "name": "Nguyen Van A",
  "email": "user@example.com",
  "phoneNumber": "0912345678",
  "password": "yourpassword"
}
```

| Field         | Type     | Description              |
|---------------|----------|--------------------------|
| `name`        | `string` | Full name                |
| `email`       | `string` | Email address (unique)   |
| `phoneNumber` | `string` | Phone number             |
| `password`    | `string` | Plain text password      |

**Response `200 OK`**
```
"User registered successfully"
```

**Errors:** `500 Internal Server Error` (duplicate email or DB error)

---

#### `POST /api/auth/login`

Authenticates a user and returns JWT tokens.

**Auth:** No

**Request Body**
```json
{
  "email": "user@example.com",
  "password": "yourpassword"
}
```

**Response `200 OK`**
```json
{
  "accessToken": "<jwt_token>",
  "refreshToken": "<refresh_token>"
}
```

| Field          | Type     | Description                          |
|----------------|----------|--------------------------------------|
| `accessToken`  | `string` | JWT, valid for 15 minutes            |
| `refreshToken` | `string` | Base64 GUID, valid for 7 days        |

**Errors:** `500 Internal Server Error` (invalid email or password)

---

## 🏨 Room Service

**Base Path:** `/api/rooms`

---

### Rooms API

| Method | Endpoint                            | Auth  | Description         |
|--------|-------------------------------------|-------|---------------------|
| GET    | `/api/rooms`                        | no    | Get all rooms       |
| GET    | `/api/rooms/{roomNumber}`           | no    | Get room by number  |
| GET    | `/api/rooms/roomtypes/{roomTypeId}` | no    | Get rooms by type   |
| POST   | `/api/rooms`                        | Admin | Create room         |
| PUT    | `/api/rooms/{roomNumber}`           | Admin | Update room         |
| DELETE | `/api/rooms/{roomNumber}`           | Admin | Delete room         |

---

#### `GET /api/rooms`

Returns all rooms with room type information.

**Auth:** No

**Response `200 OK`**
```json
[
  {
    "id": 1,
    "roomNumber": "101",
    "status": "Available",
    "roomTypeId": 2,
    "roomTypeName": "Deluxe",
    "price": 1500000,
    "description": "Deluxe room with ocean view",
    "capacity": 2
  }
]
```

| Field          | Type      | Description      |
|----------------|-----------|------------------|
| `id`           | `int`     | Room ID          |
| `roomNumber`   | `string`  | Room number      |
| `status`       | `string`  | Room status      |
| `roomTypeId`   | `int`     | Room type ID     |
| `roomTypeName` | `string`  | Room type name   |
| `price`        | `decimal` | Price per night  |
| `description`  | `string?` | Room description |
| `capacity`     | `int`     | Max capacity     |

---

#### `GET /api/rooms/{roomNumber}`

Returns details of a specific room.

**Auth:** No | **Path Param:** `roomNumber` (string)

**Response `200 OK`**
```json
{
  "id": 1,
  "roomNumber": "101",
  "roomTypeId": 2,
  "status": "Available",
  "roomType": {
    "id": 2,
    "name": "Deluxe",
    "description": "Deluxe room with ocean view",
    "price": 1500000,
    "capacity": 2
  }
}
```

**Errors:** `404 Not Found`

---

#### `GET /api/rooms/roomtypes/{roomTypeId}`

Returns all rooms belonging to a specific room type.

**Auth:** No | **Path Param:** `roomTypeId` (int)

**Response `200 OK`**
```json
[
  {
    "roomTypeName": "Deluxe",
    "price": 1500000,
    "description": "Deluxe room with ocean view",
    "rooms": [
      { "id": 1, "roomNumber": "101", "status": "Available" }
    ]
  }
]
```

---

#### `POST /api/rooms`

Creates a new room.

**Auth:** Admin

**Request Body**
```json
{
  "roomNumber": "201",
  "roomTypeId": 2,
  "status": "Available"
}
```

**Response `200 OK`**
```json
{
  "id": 5,
  "roomNumber": "201",
  "roomTypeId": 2,
  "status": "Available"
}
```

**Errors:** `401 Unauthorized` · `403 Forbidden`

---

#### `PUT /api/rooms/{roomNumber}`

Updates an existing room.

**Auth:** Admin | **Path Param:** `roomNumber` (string)

**Request Body**
```json
{
  "roomNumber": "201",
  "roomTypeId": 3,
  "status": "Maintenance"
}
```

**Response:** `200 OK` (empty body)

**Errors:** `401 Unauthorized` · `403 Forbidden` · `500 Internal Server Error` (room not found)

---

#### `DELETE /api/rooms/{roomNumber}`

Deletes a room by room number.

**Auth:** Admin | **Path Param:** `roomNumber` (string)

**Response:** `204 No Content`

**Errors:** `401 Unauthorized` · `403 Forbidden` · `500 Internal Server Error` (room not found)

---

### Room Types API

| Method | Endpoint           | Auth  | Description      |
|--------|--------------------|-------|------------------|
| POST   | `/api/rooms/types` | Admin | Create room type |

---

#### `POST /api/rooms/types`

Creates a new room type.

**Auth:** Admin

**Request Body**
```json
{
  "name": "Superior",
  "description": "Standard Superior room",
  "price": 800000,
  "capacity": 2
}
```

**Response `200 OK`**
```json
{
  "id": 3,
  "name": "Superior",
  "description": "Standard Superior room",
  "price": 800000,
  "capacity": 2
}
```

**Errors:** `401 Unauthorized` · `403 Forbidden`

---

## ❗ Error Response Format

```json
{
  "statusCode": 400,
  "message": "Room number already exists"
}
```

| Code | Meaning               |
|------|-----------------------|
| 400  | Bad Request           |
| 401  | Unauthorized          |
| 403  | Forbidden             |
| 404  | Not Found             |
| 500  | Internal Server Error |

---

## 📦 Data Models

### User (Auth Service)

| Field          | Type       | Description                  |
|----------------|------------|------------------------------|
| `Id`           | `int`      | Primary key                  |
| `Name`         | `string`   | Full name                    |
| `Email`        | `string`   | Email (used for login)       |
| `PhoneNumber`  | `string`   | Phone number                 |
| `PasswordHash` | `string`   | BCrypt hashed password       |
| `Role`         | `string`   | `User` (default) or `Admin`  |
| `CreatedAt`    | `DateTime` | Account creation timestamp   |

### RefreshToken (Auth Service)

| Field       | Type       | Description                |
|-------------|------------|----------------------------|
| `Id`        | `int`      | Primary key                |
| `UserId`    | `int`      | FK → User                  |
| `Token`     | `string`   | Base64 GUID token          |
| `ExpiresAt` | `DateTime` | Expiry (7 days from issue) |
| `IsRevoked` | `bool`     | Whether token is revoked   |

### Room (Room Service)

| Field        | Type       | Description         |
|--------------|------------|---------------------|
| `Id`         | `int`      | Primary key         |
| `RoomNumber` | `string`   | Room number         |
| `RoomTypeId` | `int`      | FK → RoomType       |
| `Status`     | `string`   | Room status         |
| `RoomType`   | `RoomType` | Navigation property |

### RoomType (Room Service)

| Field         | Type         | Description        |
|---------------|--------------|--------------------|
| `Id`          | `int`        | Primary key        |
| `Name`        | `string`     | Room type name     |
| `Description` | `string`     | Description        |
| `Price`       | `decimal`    | Price per night    |
| `Capacity`    | `int`        | Max capacity       |
| `Rooms`       | `List<Room>` | Rooms in this type |

### 🏷 Room Status Values

| Value         | Description            |
|---------------|------------------------|
| `Available`   | Room is available      |
| `Booked`      | Room is booked         |
| `Maintenance` | Room under maintenance |

---

*📅 Written by: 2026-04-13*

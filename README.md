
# 🏨 Accommodation Booking Platform

## 📝 Overview

The Accommodation Booking Platform is a comprehensive solution for managing bookings, amenities, cities, hotels, and user interactions related to accommodation services. This platform provides a set of APIs that facilitate the creation, retrieval, updating, and deletion of various entities related to hotel management and booking processes.

## 🌟 Features

- 👤 **User Management**: Register new users, log in with credentials or Google, manage bookings, and view recently visited hotels.
- 🏢 **Hotel Management**: Create, update, retrieve, and delete hotels. Manage hotel amenities and special offers.
- 🚪 **Room Management**: Create and manage rooms within hotels, including setting capacities and uploading images.
- 📅 **Booking Management**: Book rooms, manage payments through Stripe, and retrieve booking details and reports.
- 🛠️ **Amenity Management**: Create and manage hotel amenities.
- 🌍 **City Management**: Manage cities where hotels are located, including top visited cities.
- ✍️ **Review Management**: Submit and manage reviews for hotels.
- 💰 **Special Offers**: Create and manage special offers for hotels.

## 📚 API Documentation

For detailed API documentation, please refer to the [Swagger Documentation](https://app.swaggerhub.com/apis-docs/VOXAGOLDE2003/Accommodation-Booking_Platform/1.0)

## 🛠️ API Endpoints


### 🔑 User Authentication
- **Register User**: `POST /api/v1/users`
  - **Description**: Registers a new user.
  - **Request Body**: 
    ```json
    {
      "UserName": "string",
      "Email": "string",
      "Password": "string",
      "Thumbnail": "binary"
    }
    ```
  - **Responses**:
    - `201 Created`: The user was successfully created.
    - `400 Bad Request`: The request is invalid, possibly due to missing or incorrect fields.
    - `403 Forbidden`: The user is not authorized to perform this action.

- **User Login**: `POST /api/v1/sessions`
  - **Description**: Logs in a user.
  - **Request Body**:
    ```json
    {
      "Email": "string",
      "Password": "string"
    }
    ```
  - **Responses**:
    - `200 OK`: The user was successfully authenticated, and the session details are returned.
    - `400 Bad Request`: The request is invalid.
    - `403 Forbidden`: The user is not authorized to perform this action.

- **Google Login**:
  - **Initiate Google Sign-in**: `GET /api/v1/sessions/google`
    - **Description**: Initiates the Google sign-in process.
    - **Responses**:
      - `200 OK`: The Google sign-in process was initiated successfully.
  - **Handle Google Response**: `GET /api/v1/sessions/google-response`
    - **Description**: Handles the Google login response.
    - **Responses**:
      - `200 OK`: The user was successfully authenticated.
      - `400 Bad Request`: The Google sign-in process failed.
      - `403 Forbidden`: The user is not authorized to perform this action.

### 🏢 Hotel Management
- **Create Hotel**: `POST /api/v1/hotels`
  - **Description**: Creates a new hotel.
  - **Request Body**:
    ```json
    {
      "Name": "string",
      "Description": "string",
      "Address": "string",
      "CityId": "integer",
      "HotelType": "integer",
      "PricePerNight": "number",
      "Thumbnail": "binary",
      "Images": ["binary"]
    }
    ```
  - **Responses**:
    - `201 Created`: The hotel was successfully created.
    - `400 Bad Request`: The request is invalid, possibly due to missing or incorrect fields.
    - `401 Unauthorized`: The user is not authenticated.
    - `403 Forbidden`: The user is not authorized to perform this action.

- **Retrieve Hotels**: `GET /api/v1/hotels`
  - **Description**: Retrieves a list of hotels based on specified search criteria.
  - **Query Parameters**:
    - `city` (optional): The city where the hotel is located.
    - `country` (optional): The country where the hotel is located.
    - `hotelType` (optional): Filter by hotel type.
    - `minPrice` (optional): Minimum price for the hotel.
    - `maxPrice` (optional): Maximum price for the hotel.
    - `page` (optional): Page number for pagination.
    - `pageSize` (optional): Number of results per page.
  - **Responses**:
    - `200 OK`: Returns the list of hotels with pagination.
    - `400 Bad Request`: The request is invalid.

- **Retrieve Hotel by ID**: `GET /api/v1/hotels/{hotelId}`
  - **Description**: Retrieves the details of a specific hotel by its ID.
  - **Path Parameters**:
    - `hotelId`: The ID of the hotel to retrieve.
  - **Responses**:
    - `200 OK`: Returns the details of the specified hotel.
    - `404 Not Found`: The hotel with the specified ID was not found.

- **Update Hotel**: `PATCH /api/v1/hotels/{hotelId}`
  - **Description**: Updates the details of a specific hotel.
  - **Path Parameters**:
    - `hotelId`: The ID of the hotel to update.
  - **Request Body**:
    ```json
    {
      "Name": "string",
      "Description": "string",
      "Address": "string",
      "HotelType": "integer",
      "PricePerNight": "number",
      "Thumbnail": "binary",
      "Images": ["binary"]
    }
    ```
  - **Responses**:
    - `200 OK`: The hotel was successfully updated.
    - `400 Bad Request`: The request is invalid.
    - `401 Unauthorized`: The user is not authenticated.
    - `403 Forbidden`: The user is not authorized to perform this action.
    - `404 Not Found`: The hotel with the specified ID was not found.

- **Delete Hotel**: `DELETE /api/v1/hotels/{hotelId}`
  - **Description**: Deletes a specific hotel by its ID.
  - **Path Parameters**:
    - `hotelId`: The ID of the hotel to delete.
  - **Responses**:
    - `200 OK`: The hotel was successfully deleted.
    - `401 Unauthorized`: The user is not authenticated.
    - `403 Forbidden`: The user is not authorized to perform this action.
    - `404 Not Found`: The hotel with the specified ID was not found.

### 🚪 Room Management
- **Create Room**: `POST /api/v1/hotels/{hotelId}/rooms`
  - **Description**: Creates a new room in the specified hotel.
  - **Path Parameters**:
    - `hotelId`: The ID of the hotel where the room will be created.
  - **Request Body**:
    ```json
    {
      "RoomNumber": "string",
      "AdultCapacity": "integer",
      "ChildrenCapacity": "integer",
      "Thumbnail": "binary",
      "Images": ["binary"]
    }
    ```
  - **Responses**:
    - `201 Created`: The room was successfully created.
    - `400 Bad Request`: The request is invalid, possibly due to missing or incorrect fields.
    - `401 Unauthorized`: The user is not authenticated.
    - `403 Forbidden`: The user is not authorized to perform this action.
    - `404 Not Found`: The hotel with the specified ID was not found.

- **Delete Room**: `DELETE /api/v1/hotels/{hotelId}/rooms/{roomNumber}`
  - **Description**: Deletes a specific room from a hotel.
  - **Path Parameters**:
    - `hotelId`: The ID of the hotel where the room is located.
    - `roomNumber`: The number of the room to be deleted.
  - **Responses**:
    - `200 OK`: The room was successfully deleted.
    - `401 Unauthorized`: The user is not authenticated.
    - `403 Forbidden`: The user is not authorized to perform this action.
    - `404 Not Found`: The hotel or room with the specified IDs was not found.

### 📅 Booking Management
- **Create Booking**: `POST /api/v1/users/bookings`
  - **Description**: Creates a new room booking.
  - **Request Body**:
    ```json
    {
      "hotelId": "integer",
      "roomsNumbers": ["string"],
      "startDate": "string (date)",
      "endDate": "string (date)"
    }
    ```
  - **Responses**:
    - `201 Created`: The booking was successfully created.
    - `400 Bad Request`: The request is invalid, possibly due to missing or incorrect fields.
    - `401 Unauthorized`: The user is not authenticated.
    - `403 Forbidden`: The user is not authorized to perform this action.
    - `409 Conflict`: There is a conflict in the booking request, such as overlapping dates or unavailable rooms.

- **Confirm Booking Payment**: `POST /api/v1/users/bookings/payments`
  - **Description**: Confirms a booking payment via Stripe.
  - **Responses**:
    - `200 OK`: The payment was successfully confirmed.
    - `400 Bad Request`: The payment request is invalid.
    - `401 Unauthorized`: The user is not authenticated.
    - `403 Forbidden`: The user is not authorized to perform this action.

- **Retrieve User Bookings**: `GET /api/v1/users/{userId}/bookings`
  - **Description**: Retrieves a list of bookings made by a specific user.
  - **Path Parameters**:
    - `userId`: The ID of the user whose bookings are to be retrieved.
  - **Query Parameters**:
    - `startDate` (optional): Start date for filtering bookings.
    - `endDate` (optional): End date for filtering bookings.
    - `page` (optional): Page number for pagination.
    - `pageSize` (optional): Number of results per page.
  - **Responses**:
    - `200 OK`: Returns the list of bookings with pagination.
    - `401 Unauthorized`: The user is not authenticated.
    - `403 Forbidden`: The user is not authorized to perform this action.

- **Retrieve Booking by ID**: `GET /api/v1/users/{userId}/bookings/{bookingId}`
  - **Description**: Retrieves the details of a specific booking made by a user.
  - **Path Parameters**:
    - `userId`: The ID of the user whose booking is to be retrieved.
    - `bookingId`: The ID of the booking to be retrieved.
  - **Responses**:
    - `200 OK`: Returns the booking details.
    - `401 Unauthorized`: The user is not authenticated.
    - `403 Forbidden`: The user is not authorized to perform this action.
    - `404 Not Found`: The booking with the specified ID was not found.

- **Generate Booking Report**: `GET /api/v1/users/{userId}/bookings/{bookingId}/report`
  - **Description**: Generates a confirmation report for a specific booking.
  - **Path Parameters**:
    - `userId`: The ID of the user whose booking report is to be generated.
    - `bookingId`: The ID of the booking for which the report is to be generated.
  - **Responses**:
    - `200 OK`: Returns the booking confirmation report as a PDF.
    - `401 Unauthorized`: The user is not authenticated.
    - `403 Forbidden`: The user is not authorized to perform this action.
    - `404 Not Found`: The booking with the specified ID was not found.

### 🛠️ Amenity Management
- **Create Amenity**: `POST /api/v1/amenities`
  - **Description**: Creates a new amenity.
  - **Request Body**:
    ```json
    {
      "name": "string",
      "description": "string"
    }
    ```
  - **Responses**:
    - `201 Created`: The amenity was successfully created.
    - `400 Bad Request`: The request is invalid, possibly due to missing or incorrect fields.
    - `401 Unauthorized`: The user is not authenticated.
    - `403 Forbidden`: The user is not authorized to perform this action.

- **Retrieve Amenities**: 
  - **Paginated List**: `GET /api/v1/amenities`
    - **Description**: Retrieves a list of amenities with pagination.
    - **Query Parameters**:
      - `page` (optional): Page number for pagination.
      - `pageSize` (optional): Number of results per page.
    - **Responses**:
      - `200 OK`: Returns the list of amenities with pagination.
      - `400 Bad Request`: The request is invalid.

  - **By ID**: `GET /api/v1/amenities/{amenityId}`
    - **Description**: Retrieves an amenity by its ID.
    - **Path Parameters**:
      - `amenityId`: The ID of the amenity to retrieve.
    - **Responses**:
      - `200 OK`: Returns the details of the specified amenity.
      - `404 Not Found`: The amenity with the specified ID was not found.

- **Update Amenity**: `PUT /api/v1/amenities/{amenityId}`
  - **Description**: Updates an existing amenity.
  - **Path Parameters**:
    - `amenityId`: The ID of the amenity to update.
  - **Request Body**:
    ```json
    {
      "name": "string",
      "description": "string"
    }
    ```
  - **Responses**:
    - `200 OK`: The amenity was successfully updated.
    - `400 Bad Request`: The request is invalid.
    - `401 Unauthorized`: The user is not authenticated.
    - `403 Forbidden`: The user is not authorized to perform this action.
    - `404 Not Found`: The amenity with the specified ID was not found.

- **Delete Amenity**: `DELETE /api/v1/amenities/{amenityId}`
  - **Description**: Deletes an amenity by its ID.
  - **Path Parameters**:
    - `amenityId`: The ID of the amenity to delete.
  - **Responses**:
    - `200 OK`: The amenity was successfully deleted.
    - `401 Unauthorized`: The user is not authenticated.
    - `403 Forbidden`: The user is not authorized to perform this action.
    - `404 Not Found`: The amenity with the specified ID was not found.

### 🌍 City Management
- **Create City**: `POST /api/v1/cities`
  - **Description**: Creates a new city.
  - **Request Body**:
    ```json
    {
      "Name": "string",
      "Country": "string",
      "PostOffice": "string",
      "Thumbnail": "binary"
    }
    ```
  - **Responses**:
    - `201 Created`: The city was successfully created.
    - `400 Bad Request`: The request is invalid, possibly due to missing or incorrect fields.
    - `401 Unauthorized`: The user is not authenticated.
    - `403 Forbidden`: The user is not authorized to perform this action.

- **Retrieve Cities**: 
  - **Filtered List**: `GET /api/v1/cities`
    - **Description**: Retrieves a list of cities, optionally filtered by name and country.
    - **Query Parameters**:
      - `city` (optional): The name of the city to filter by.
      - `country` (optional): The name of the country to filter by.
      - `page` (optional): Page number for pagination.
      - `pageSize` (optional): Number of results per page.
    - **Responses**:
      - `200 OK`: Returns the list of cities.
      - `400 Bad Request`: The request is invalid.

  - **By ID**: `GET /api/v1/cities/{cityId}`
    - **Description**: Retrieves details of a specific city by its ID.
    - **Path Parameters**:
      - `cityId`: The ID of the city to retrieve.
    - **Responses**:
      - `200 OK`: Returns the details of the specified city.
      - `404 Not Found`: The city with the specified ID was not found.

- **Update City**: `PUT /api/v1/cities/{cityId}`
  - **Description**: Updates an existing city.
  - **Path Parameters**:
    - `cityId`: The ID of the city to update.
  - **Request Body**:
    ```json
    {
      "Name": "string",
      "Country": "string",
      "PostOffice": "string",
      "Thumbnail": "binary"
    }
    ```
  - **Responses**:
    - `200 OK`: The city was successfully updated.
    - `400 Bad Request`: The request is invalid.
    - `401 Unauthorized`: The user is not authenticated.
    - `403 Forbidden`: The user is not authorized to perform this action.
    - `404 Not Found`: The city with the specified ID was not found.

- **Delete City**: `DELETE /api/v1/cities/{cityId}`
  - **Description**: Deletes a city by its ID.
  - **Path Parameters**:
    - `cityId`: The ID of the city to delete.
  - **Responses**:
    - `200 OK`: The city was successfully deleted.
    - `401 Unauthorized`: The user is not authenticated.
    - `403 Forbidden`: The user is not authorized to perform this action.
    - `404 Not Found`: The city with the specified ID was not found.

- **Top Visited Cities**: `GET /api/v1/cities/top-visited-cities`
  - **Description**: Retrieves a list of the top visited cities.
  - **Responses**:
    - `200 OK`: Returns the list of top visited cities.

### ✍️ Review Management
- **Create Review**: `POST /api/v1/hotels/{hotelId}/reviews`
  - **Description**: Creates a new review for a specific hotel.
  - **Path Parameters**:
    - `hotelId`: The ID of the hotel to review.
  - **Request Body**:
    ```json
    {
      "comment": "string",
      "rating": "integer"
    }
    ```
  - **Responses**:
    - `201 Created`: The review was successfully created.
    - `400 Bad Request`: The request is invalid, possibly due to missing or incorrect fields.
    - `401 Unauthorized`: The user is not authenticated.
    - `403 Forbidden`: The user is not authorized to perform this action.
    - `404 Not Found`: The hotel with the specified ID was not found.

- **Delete Review**: `DELETE /api/v1/hotels/{hotelId}/reviews/{userId}`
  - **Description**: Deletes a specific review for a hotel.
  - **Path Parameters**:
    - `hotelId`: The ID of the hotel associated with the review.
    - `userId`: The ID of the user who created the review.
  - **Responses**:
    - `200 OK`: The review was successfully deleted.
    - `401 Unauthorized`: The user is not authenticated.
    - `403 Forbidden`: The user is not authorized to perform this action.
    - `404 Not Found`: The hotel or review with the specified IDs was not found.

### 💰 Special Offers
- **Create Special Offer**: `POST /api/v1/hotels/{hotelId}/special-offers`
  - **Description**: Creates a new special offer for a specific hotel.
  - **Path Parameters**:
    - `hotelId`: The ID of the hotel to which the special offer will be added.
  - **Request Body**:
    ```json
    {
      "discountPercentage": "integer",
      "expireDate": "string (date)",
      "offerType": "string"
    }
    ```
  - **Responses**:
    - `201 Created`: The special offer was successfully created.
    - `400 Bad Request`: The request is invalid, possibly due to missing or incorrect fields.
    - `401 Unauthorized`: The user is not authenticated.
    - `403 Forbidden`: The user is not authorized to perform this action.
    - `404 Not Found`: The hotel with the specified ID was not found.

- **Retrieve Top Special Offers**: `GET /api/v1/special-offers`
  - **Description**: Retrieves the top special feature deal offers.
  - **Responses**:
    - `200 OK`: Returns the list of top special offers.

## 🚀 Getting Started

### ✅ Prerequisites
- 🛠️ .NET Core SDK
- 💾 SQL Server or another supported database
- 💳 Stripe API keys for payment processing

### ⚙️ Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/KhaledSaleh122/Accommodation-Booking-Platform.git
   cd Accommodation-Booking-Platform
   ```

2. Install dependencies:
   ```bash
   dotnet restore
   ```

3. Set up the database:
   ```bash
   dotnet ef database update
   ```

4. Configure environment variables for Stripe, database connection, and other settings.

### ▶️ Running the Application

To run the application, use the following command:

```bash
dotnet run
```

The application will start on `http://localhost:5000` by default.

## 🧪 Running Tests

To ensure that everything is working as expected, you can run the tests provided in the repository:

```bash
dotnet test
```

This will execute all the unit and integration tests.

## 🚀 Deployment

To deploy the application, follow these steps:

1. Set up the production environment with the necessary dependencies (.NET Core, database, etc.).
2. Ensure all environment variables are configured for production (e.g., database connection strings, Stripe API keys).
3. Publish the application:
   ```bash
   dotnet publish -c Release -o ./publish
   ```
4. Deploy the contents of the `./publish` directory to your server or cloud service.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 📬 Contact

[Khaled Saleh](mailto:khaled.s.saleh@hotmail.com).

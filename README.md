# ASP.NET Core MongoDB CRUD Application

This project demonstrates a simple CRUD (Create, Read, Update, Delete) operation using MongoDB and ASP.NET Core. The application allows users to perform CRUD operations on car records in a MongoDB database.

## Prerequisites

Before running the application, make sure you have the following installed:

- [.NET 7 SDK or later](https://dotnet.microsoft.com/download)
- [MongoDB](https://www.mongodb.com/try/download/community) (if running locally)
- MongoDB connection string (if using MongoDB Atlas)

## Installation

1. Clone this repository:

   ```bash
   git clone https://github.com/yourusername/yourrepository.git
   ```

2. Navigate to the project directory:

   ```bash
   cd yourrepository
   ```

3. Install the required packages:
   ```bash
   dotnet add package MongoDB.Driver
   dotnet add package Microsoft.AspNetCore.OpenApi
   dotnet add package Swashbuckle.AspNetCore
   ```

## Setup

### MongoDB Connection String

1. In the `appsettings.json` file, configure your MongoDB connection string and database name:

   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     },
     "MongoDB": {
       "ConnectionString": "your-mongodb-connection-string",
       "DatabaseName": "your-database-name"
     },
     "AllowedHosts": "*"
   }
   ```

2. Alternatively, create a `appsettings.sample.json` file with placeholder values and rename it to `appsettings.json` for your own configuration.

## How to Run

1. Build and run the project:

   ```bash
   dotnet run
   ```

2. By default, the app will be hosted on `http://localhost:5000`.

## Endpoints

- `GET /showrecords`: Retrieves all car records from the MongoDB database.
- `POST /addrecord`: Adds a new car record to the database. Example payload:
  ```json
  {
    "Type": "SUV",
    "HP": 200,
    "HPl100": 8.5
  }
  ```
- `DELETE /deleterecord/{id}`: Deletes a car record by its ID.
- `PUT /updaterecord/{id}`: Updates an existing car record by its ID. Example payload:
  ```json
  {
    "Type": "Sedan",
    "HP": 150,
    "HPl100": 7.2
  }
  ```

## Additional Notes

- The project uses `CORS` to allow requests from any origin.
- MongoDB operations are done using the `MongoDB.Driver` package.
- Swagger is enabled for easy testing of the API. Navigate to `/swagger` to interact with the API.

## License

This project is licensed under the MIT License. See the `LICENSE` file for details.

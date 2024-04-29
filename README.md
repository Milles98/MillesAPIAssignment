# AdvertisementAPI
This is a RESTful API built with .NET 8.0, designed to manage advertisements. 

It provides endpoints for creating, retrieving, updating, and deleting ads. 

The API also supports partial updates via HTTP PATCH and includes authentication and authorization mechanisms.

Azure: https://advertisementapi.azurewebsites.net/swagger/index.html

### Available logins for testing JWT:
Admin:
    
    {
    
        "username": "AdsAdmin",
        
        "password": "AdsAdminPassword123!",
        
    }
    
User:
    
    {
    
        "username": "AdsUser",
        
        "password": "AdsUserPassword123!",
        
    }

## Usage
The API provides the following endpoints:

•	POST /ads: Create a new ad. The body of the request should be a JSON object with Title and Description fields.

•	GET /ads: Get a list of all ads.

•	GET /ads/{id}: Get the details of a specific ad.

•	PUT /ads/{id}: Update a specific ad. The body of the request should be a JSON object with the fields to be updated.

•	PATCH /ads/{id}: Partially update a specific ad. The body of the request should be a JSON array of patch operations.

•	DELETE /ads/{id}: Delete a specific ad.

•	POST /ads/login: Authenticate a user and retrieve a JWT token.

## Authentication and Authorization
The API uses JWT for authentication. To authenticate, send a POST request to /api/ads/login with a JSON object containing Username and Password fields. 

The response will include a JWT token which should be included in the Authorization header of subsequent requests, prefixed with Bearer.

### Partial Updates
The API supports partial updates via HTTP PATCH. 

To partially update an ad, send a PATCH request to /api/ads/{id} with a JSON array of patch operations in the request body. 

Each operation should be a JSON object with op, path, and value fields. The op field should be one of the following: add, remove, replace, move, copy, or test. 

The path field should be a string containing a path to the field to be operated on, and the value field should be the value to be used in the operation.

## Built With

•	.NET 8.0

•	Entity Framework Core

•	SQL Server


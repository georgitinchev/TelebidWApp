# WebAppTelebid

## Overview
This project is a simple web server implemented without using any frameworks or auto-generated code. It includes functionalities for user registration, login/logout, profile update, and CAPTCHA verification. The server is designed to run on `localhost:8080` and interacts with a remote SQL Server database.

## Features
- **User Registration**: Validates and saves user data (email, name, password) in a SQL Server database.
- **Login/Logout**: Authenticates users and manages session cookies.
- **Profile Update**: Allows users to update their name and password.
- **CAPTCHA**: Generates and verifies CAPTCHA codes without using external services.
- **Unit Tests**: Full unit test coverage for all functions.

## Technologies Used
- **C#**: The primary programming language used for the server.
- **SQL Server**: The relational database used to store user data.
- **HttpListener**: Used to handle HTTP requests and responses.
- **SHA256**: Used for password hashing.
- **System.Drawing**: Used to generate CAPTCHA images.

## Project Structure

- **Program.cs**  
  The entry point of the application. It sets up the HTTP listener and handles incoming requests.

- **UserRepository.cs**  
  Handles database operations related to user data, such as fetching, registering, and updating users.

- **RequestHandler.cs**  
  Handles different HTTP routes and their corresponding actions, such as registration, login, logout, profile update, and CAPTCHA generation.

- **AuthService.cs**  
  Provides authentication services, including password hashing and user verification.

- **DatabaseHelper.cs**  
  Manages database connections and operations, ensuring proper opening and closing of connections.

## How to Run

1. **Clone the Repository**:
   ```bash
   git clone <repository-url>
   cd <repository-directory>

## Run the Pre-built Version:
- Navigate to the `published` folder in the root of the repository.
- Use the provided shortcut to start the server.
- The server will start automatically on `http://localhost:8080`.

## Functionalities Implemented

### Registration
- **Validation**: Ensures email, name, and password are valid.
- **Database Storage**: Saves user data in the SQL Server database.
- **CAPTCHA**: Verifies CAPTCHA code before registration.

### Login/Logout
- **Authentication**: Verifies user credentials and manages session cookies.
- **Logout**: Clears session cookies.

### Profile Update
- **Update Data**: Allows users to update their name and password.
- **Validation**: Ensures the new data is valid and not already taken by another user.

### CAPTCHA
- **Generation**: Creates CAPTCHA codes and images.
- **Verification**: Validates user input against the generated CAPTCHA code.

## Pre-built Functions Used
- **HttpListener**: For handling HTTP requests and responses.
- **SHA256**: For hashing passwords.
- **System.Drawing**: For generating CAPTCHA images.
- **SqlConnection**: For database operations.

## File Descriptions
- **Program.cs**: Sets up the server and handles incoming requests.
- **UserRepository.cs**: Manages user-related database operations.
- **RequestHandler.cs**: Handles different HTTP routes and their actions.
- **AuthService.cs**: Provides authentication services.
- **DatabaseHelper.cs**: Manages database connections.

## Unit Tests
- **Unit Test Project**: There is a separate project that uses Moq for unit tests of the main classes. This ensures full unit test coverage for all functions.

## Conclusion
This project demonstrates a simple web server with essential functionalities for user management and CAPTCHA verification, implemented without using any frameworks or auto-generated code. The server interacts with a remote SQL Server database and provides full unit test coverage for all functions.

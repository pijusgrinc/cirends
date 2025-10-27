# Token Refresh Implementation - 60 Minute Expiration

## Overview
Implemented a complete token refresh system with 60-minute access token expiration and 7-day refresh token expiration.

## Changes Made

### 1. **TokenService.cs** - Updated Service
- Changed access token expiration from 7 days to **60 minutes** (`AddMinutes(60)`)
- Added `GenerateRefreshToken()` method using `RandomNumberGenerator` for secure tokens
- Added `GetPrincipalFromExpiredToken()` method to extract claims from expired tokens (validates token signature but ignores expiration)

### 2. **RefreshToken.cs** - New Model
Created a new model to store refresh tokens in the database:
- `Id`: Primary key
- `Token`: The refresh token string (unique)
- `ExpiryDate`: When the refresh token expires (7 days from creation)
- `IsRevoked`: Flag to revoke tokens on logout or suspicious activity
- `CreatedAt`: When the token was created
- `UserId`: Foreign key to User
- Navigation property to User

### 3. **User.cs** - Updated Model
Added navigation property: `public ICollection<RefreshToken> RefreshTokens`

### 4. **CirendsDbContext.cs** - Updated Context
- Added `DbSet<RefreshToken> RefreshTokens`
- Configured RefreshToken entity with:
  - Cascade delete on User deletion
  - Unique index on Token
  - Foreign key relationship with User

### 5. **UserDtos.cs** - Updated DTOs
- Added `RefreshToken` property to `AuthResponseDto`
- Created new `RefreshTokenRequestDto` with `Token` and `RefreshToken` properties

### 6. **AuthController.cs** - Updated Endpoints

#### Register Endpoint (Updated)
- Now generates and stores a refresh token
- Returns both access token and refresh token

#### Login Endpoint (Updated)
- Generates and stores a refresh token (expires in 7 days)
- Returns both access token and refresh token in response

#### Refresh Endpoint (New)
- **Route**: `POST /api/auth/refresh`
- **Request Body**: `{ token, refreshToken }`
- **Process**:
  1. Validates the expired access token
  2. Looks up the refresh token in database
  3. Checks if token is not revoked and not expired
  4. Verifies user is still active
  5. Issues new access token (60 minutes expiration)
  6. Issues new refresh token (7 days expiration)
  7. Revokes old refresh token
- **Response**: New access and refresh tokens

#### Logout Endpoint (New)
- **Route**: `POST /api/auth/logout`
- **Request Body**: `{ token, refreshToken }`
- **Process**:
  1. Finds the refresh token
  2. Marks it as revoked
- **Response**: Confirmation message

## Database Migration
Created migration: `AddRefreshTokenTable`
- Adds `RefreshTokens` table with all necessary columns
- Sets up foreign key relationship with Users table

## Token Lifecycle

### Access Token
- **Duration**: 60 minutes
- **Usage**: Sent with each API request in Authorization header
- **Contains**: User ID, Name, Role, Email claims

### Refresh Token
- **Duration**: 7 days
- **Usage**: Stored securely on client, used to get new access tokens
- **Can be revoked**: Marked as revoked on logout or manual revocation

## Client Usage Example

### Login
```bash
POST /api/auth/login
{
  "email": "user@example.com",
  "password": "password"
}

Response:
{
  "token": "eyJhbGc...",           # Access token (60 min)
  "refreshToken": "base64token...", # Refresh token (7 days)
  "user": { ... }
}
```

### Refresh Token (Before Access Token Expires)
```bash
POST /api/auth/refresh
{
  "token": "eyJhbGc...",           # Current access token
  "refreshToken": "base64token..."  # Current refresh token
}

Response:
{
  "token": "eyJhbGc...",           # New access token
  "refreshToken": "base64token...", # New refresh token
  "user": { ... }
}
```

### Logout
```bash
POST /api/auth/logout
{
  "token": "eyJhbGc...",
  "refreshToken": "base64token..."
}

Response:
{
  "message": "Logout successful"
}
```

## Security Features
1. ? Refresh tokens are stored securely in database
2. ? Can be revoked instantly
3. ? Old refresh tokens are automatically revoked when new ones are issued
4. ? Token validation checks user is still active
5. ? Signed with HMAC-SHA256
6. ? Unique constraint on refresh tokens
7. ? Proper error handling and logging

## Migration Steps
To apply these changes to your database:
```bash
cd CirendsAPI
dotnet ef database update
```

The migration has been created automatically when you ran:
```bash
dotnet ef migrations add AddRefreshTokenTable
```

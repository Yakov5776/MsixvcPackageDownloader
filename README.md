# MSIX Package Downloader

A .NET application that provides functionality to download MSIX packages from Xbox Live services.

## Features

- Authenticates with Xbox Live services using Windows Live authentication
- Fetches package information from Xbox Live package services
- Displays downloadable links for MSIX packages
- Supports both interactive and command-line modes
- Automatically handles token refresh and re-authentication

## Usage

### Interactive Mode

Run the application without arguments to enter interactive mode:

```bash
dotnet run
```

The application will guide you through the authentication process and then prompt you to enter Content IDs.

### Command Line Mode

Run the application with a Content ID as an argument for CLI mode:

```bash
dotnet run <ContentId>
```

For example:
```bash
dotnet run 9WZDNCRFJ3TJ
```

## Authentication

The application uses Xbox Live authentication. On first run, it will:

1. Display an authentication URL
2. Ask you to sign in through your browser
3. Request that you paste the resulting URL back into the application
4. Store authentication tokens for future use

Authentication tokens are stored in `token.json` and will be automatically refreshed when needed.

## Configuration

The application uses several endpoints and constants defined in `Configuration.cs`:

- Xbox Live Client ID: `00000000402b5328`
- Package Service Base URL: `https://packagespc.xboxlive.com/GetBasePackage/`
- Token storage: `token.json`

## File Exclusions

The application automatically excludes certain file types from processing:
- `.phf` files (Package Hash Files)
- `.xsp` files (Xbox Service Package files)

## Development

For development purposes, you can place an authentication URL in `authUrl.txt` to skip the manual authentication step.

## Requirements

- .NET 8.0 or later
- Internet connection for Xbox Live service authentication and API calls

## API Endpoints

The application currently uses the following Xbox Live endpoints:

- `https://packagespc.xboxlive.com/GetBasePackage/<ContentId>` - Get all available information about a given package
- Additional endpoints are documented in `OtherEndpoints.md`

## Error Handling

The application includes comprehensive error handling for:

- Invalid Content IDs
- Network request failures
- Authentication token expiration
- API response parsing errors

## License

This project is a proof-of-concept (POC) for accessing Xbox Live package endpoints.
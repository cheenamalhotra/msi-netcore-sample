# Sample Console App to test MSI Authentication with .Net Core App

This sample application can be used to test MSI Authentication from an **"Identity"** enabled Azure Resource. The application code acquires Access Token by making `HttpWebRequest` call to target Azure Services, and then creates `SqlConnection` instance with the access token acquired.

### Packages Referenced:
- Microsoft.Data.SqlClient 1.0.19189.1-Preview
- Newtonsoft.Json 12.0.2


> NOTE: This code to fetch access token should not be used in production environments, please refer [Microsoft Documentation](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/how-to-use-vm-token) to handle HTTP Error scenarios.

## Pre-Requisites
- .Net Core SDK 2.1
- Identity Feature enabled on Azure Resource where this application will run.
- Access provided to Azure Resource (Virtual Machine/App Service/Function App) to connect to target Azure database. 
    - Quick resolution: Run this query in Azure Database _(Login with Active Directory account)_:
    
        `CREATE USER [<AZURE_RESOURCE_NAME>] FROM EXTERNAL PROVIDER`

## Steps to run this application:
- Clone the repository
- Run below commands:
    - `dotnet restore`
    - `dotnet msbuild`
    - `dotnet .\bin\Debug\netcoreapp2.1\TestMSIAuthentication.dll <server> <database>`

        e.g. `dotnet .\bin\Debug\netcoreapp2.1\TestMSIAuthentication.dll myserver.database.windows.net MyAzureDB`

        Below response confirms connectivity established:

        ```bash
        PS C:\Users\dotnet\Documents\TestMSIAuthentication> dotnet bin\Debug\netcoreapp2.1\TestMSIAuthentication.dll <server> <database>
        Connected!
        Microsoft SQL Azure (RTM) - 12.0.2000.8
            Jul  3 2019 10:02:53
            Copyright (C) 2019 Microsoft Corporation
        ```
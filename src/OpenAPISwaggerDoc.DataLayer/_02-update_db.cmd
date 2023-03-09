dotnet tool update --global dotnet-ef --version 7.0.3
dotnet tool restore
dotnet build
dotnet ef --startup-project ../OpenAPISwaggerDoc.Web/ database update
pause
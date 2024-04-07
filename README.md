# XperienceCommunity.AspNetCore.HealthChecks

This project is a NuGet package specifically designed to integrate Kentico Health Checks into applications using the Microsoft.AspNetCore.Health framework. It provides a set of custom health checks that monitor various aspects of a Kentico application, ensuring its optimal performance and stability.

The health checks included in this package cover a wide range of Kentico functionalities, from site configuration and event logs to web farm and search tasks. By leveraging the Microsoft.AspNetCore.Health framework, these health checks can be easily added to any ASP.NET Core application, providing developers with immediate insights into the health of their Kentico applications.

This package is an essential tool for any developer working with Kentico in an ASP.NET Core environment, simplifying the process of monitoring and maintaining the health of their applications.

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes.

### Prerequisites

1. .NET 6/.NET 8 
2. Kentico Xperience 13 Application.

### Installing

This package provides a set of Kentico-specific health checks that you can easily add to your ASP.NET Core project. Here's how you can do it:

1. First, make sure you have the necessary dependencies installed. You will need the `Microsoft.Extensions.DependencyInjection` package for the `IServiceCollection` interface.

2. Install this package via NuGet.

3. In your `Startup.cs` file (or wherever you configure your services), use the `AddKenticoHealthChecks` extension method on your `IServiceCollection` instance. Here's an example:

#### Add all Kentico Health Checks
This method will add all the Kentico Health checks to your application.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddKenticoHealthChecks();
}
```

#### Add specific Kentico Health Checks
This method will allow you the most flexibility to add only the health checks you want for your application.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddHealthChecks()
            .AddCheck<SiteConfigurationHealthCheck>("Site Configuration Health Check");
}
```

#### Middleware Registration

In your `Startup.cs` file (or wherever you configure your application), use the `UseHealthChecks` extension method on your `IApplicationBuilder` instance. This registers the health checks as middleware.Here's an example:

```csharp
public void Configure(IApplicationBuilder app)
{    
    app.UseHealthChecks("/kentico-health");
}
```

#### Endpoint Registration

In your `Startup.cs` file (or wherever you configure your application), use the `MapHealthChecks` extension method on your `IEndpointRouteBuilder` instance. This registers the health checks as an endpoint.Here's an example:

```csharp
app.UseEndpoints(endpoints =>
{
    var defaultCulture = CultureInfo.GetCultureInfo("en-US");

    endpoints.Kentico().MapRoutes();

    endpoints.MapHealthChecks("/kentico-health");
}
```

## Health Checks

### Application Initialized Health Check

The `ApplicationInitializedHealthCheck` class is an implementation of the `IHealthCheck` interface. It is used to perform a health check on the application initialization. 

### Azure Search Task Health Check

The `AzureSearchTaskHealthCheck` is a health check implementation that checks the Azure Search Task for any errors.

### EventLogHealthCheck

The `EventLogHealthCheck` class is an implementation of the `IHealthCheck` interface. It is used to perform a health check on the event log by investigating the last 100 event log entries for errors. 

### LocalSearchTaskHealthCheck

The `LocalSearchTaskHealthCheck` class is an implementation of the `IHealthCheck` interface. It is responsible for checking the health of local search tasks and determining if any errors are present. This health check is used to monitor the status of search tasks in the application.

### SiteConfigurationHealthCheck

The `SiteConfigurationHealthCheck` class is an implementation of the `IHealthCheck` interface. It is responsible for checking the health of the site configuration in a CMS (Content Management System) application. 

### SitePresentationHealthCheck

The `SitePresentationHealthCheck` class is an implementation of the `IHealthCheck` interface. It is responsible for checking the health of the site presentation configuration in an ASP.NET Core application.

### SitePresentationHealthCheck

The `StagingTaskHealthCheck` class is an implementation of the `IHealthCheck` interface. It is responsible for checking the health of staging tasks in a Kentico Xperience CMS application.

### WebFarmHealthCheck

The `WebFarmHealthCheck` class is an implementation of the `IHealthCheck` interface provided by the `Microsoft.Extensions.Diagnostics.HealthChecks` namespace. It is used to perform health checks on the Kentico web farm servers.

### WebFarmTaskHealthCheck

The `WebFarmTaskHealthCheck` class is an implementation of the `IHealthCheck` interface. It is responsible for checking the health of the web farm server tasks. 

## Built With

* [Microsoft.AspNetCore.Health](https://www.nuget.org/packages/Microsoft.AspNetCore.Diagnostics.HealthChecks/) - The web framework used
* [Kentico Xperience 13](https://www.kentico.com) - Kentico Xperience
* [NuGet](https://nuget.org/) - Dependency Management

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/your/project/tags). 

## Authors

* **Brandon Henricks** - *Initial work* - [Brandon Henricks](https://github.com/brandonhenricks)

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

* Chris Blaylock
* Mike Wills
* Jordan Walters
* Alan Abair
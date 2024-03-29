# XperienceCommunity.AspNetCore.HealthChecks

This project is a NuGet package specifically designed to integrate Kentico Health Checks into applications using the Microsoft.AspNetCore.Health framework. It provides a set of custom health checks that monitor various aspects of a Kentico application, ensuring its optimal performance and stability.

The health checks included in this package cover a wide range of Kentico functionalities, from site configuration and event logs to web farm and search tasks. By leveraging the Microsoft.AspNetCore.Health framework, these health checks can be easily added to any ASP.NET Core application, providing developers with immediate insights into the health of their Kentico applications.

This package is an essential tool for any developer working with Kentico in an ASP.NET Core environment, simplifying the process of monitoring and maintaining the health of their applications.

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes.

### Prerequisites

1. .NET 6/.NET 8 Kentico Xperience Application.

### Installing

This package provides a set of Kentico-specific health checks that you can easily add to your ASP.NET Core project. Here's how you can do it:

1. First, make sure you have the necessary dependencies installed. You will need the `Microsoft.Extensions.DependencyInjection` package for the `IServiceCollection` interface.

2. Install this package via NuGet.

3. In your `Startup.cs` file (or wherever you configure your services), use the `AddKenticoHealthChecks` extension method on your `IServiceCollection` instance. Here's an example:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddKenticoHealthChecks();
}
```

## Built With

* [Microsoft.AspNetCore.Health](https://www.nuget.org/packages/Microsoft.AspNetCore.Diagnostics.HealthChecks/) - The web framework used
* [Kentico Xperience 13](https://www.kentico.com) - Kentico Xperience
* [NuGet](https://nuget.org/) - Dependency Management

## Contributing

Please read [CONTRIBUTING.md](https://gist.github.com/PurpleBooth/b24679402957c63ec426) for details on our code of conduct, and the process for submitting pull requests to us.

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/your/project/tags). 

## Authors

* **Brandon Henricks** - *Initial work* - [YourName](https://github.com/brandonhenricks)

See also the list of [contributors](https://github.com/your/project/contributors) who participated in this project.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

* Hat tip to anyone whose code was used
* Inspiration
* etc
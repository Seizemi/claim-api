using Xunit;

namespace Modules.Claims.Features.Integration.Tests.Infrastructure;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<IntegrationTestWebAppFactory>
{
    internal const string Name = "Integration Tests";
}

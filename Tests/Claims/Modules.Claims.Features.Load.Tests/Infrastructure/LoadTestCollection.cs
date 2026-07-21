using Xunit;

namespace Modules.Claims.Features.Load.Tests.Infrastructure;

[CollectionDefinition(Name)]
public sealed class LoadTestCollection : ICollectionFixture<LoadTestWebAppFactory>
{
    internal const string Name = "Load Tests";
}

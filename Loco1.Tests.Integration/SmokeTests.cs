using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Loco1.Tests.Integration;

public class SmokeTests
{
    [Fact]
    public async Task HomePage_Should_Load()
    {
        using var app = new WebApplicationFactory<Loco1.Web.Program>();
        var client = app.CreateClient();
        var res = await client.GetAsync("/");
        Assert.True(res.IsSuccessStatusCode);
    }
}
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PEIA.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var assemblyDir = Path.GetDirectoryName(typeof(CustomWebApplicationFactory).Assembly.Location)!;

        var webProjectDir = Path.GetFullPath(
            Path.Combine(assemblyDir, "..", "..", "..", "..", "..", "src", "PEIA.Web"));

        builder.UseContentRoot(webProjectDir);
        builder.UseEnvironment("Development");
    }
}

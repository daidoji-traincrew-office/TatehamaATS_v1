using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TatehamaATS_v1.Network;

public class DbInitWorker(IServiceProvider provider): IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = provider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<DbContext>();
        await context.Database.EnsureCreatedAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
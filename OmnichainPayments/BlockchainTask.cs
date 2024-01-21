using Cila.Models;
using MongoDB.Driver;

namespace Cila{

    public class BlockchainTask : IHostedService, IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private Timer _timer;

    public BlockchainTask(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(DoWork, null, TimeSpan.Zero, 
            TimeSpan.FromMinutes(1)); // Runs every 1 minute

        return Task.CompletedTask;
    }

    private async void DoWork(object state)
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var mongoClient = scope.ServiceProvider.GetRequiredService<IMongoClient>();
            var blockchainSender = scope.ServiceProvider.GetRequiredService<BlockchainSender>();

            var database = mongoClient.GetDatabase("omniassets");
            var collection = database.GetCollection<TransferData>("transferdatas");
            var transferDataList = await collection.Find(_ => true).ToListAsync();

            if (transferDataList.Any())
            {
                if (await blockchainSender.SendToBlockchain(transferDataList))
                {
                    // Delete the processed data
                    var idsToDelete = transferDataList.Select(d => d.Id).ToList();
                    await collection.DeleteManyAsync(d => idsToDelete.Contains(d.Id));
                }
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}

}
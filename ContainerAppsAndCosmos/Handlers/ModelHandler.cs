using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using Rebus.Handlers;
using Rebus.Pipeline;

namespace ContainerAppsAndCosmos.Handlers;

public class ModelHandler : IHandleMessages<Model>
{
    private readonly IMessageContext _messageContext;
    private readonly CosmosClient _client;

    public ModelHandler(IMessageContext messageContext,CosmosClient client)
    {
        _messageContext = messageContext;
        _client = client;
    }

    public async Task Handle(Model message)
    {
        try
        {
            Console.WriteLine("Start bus3 upsert");
            var dbName = Environment.GetEnvironmentVariable("DATABASE");
            var containerName = Environment.GetEnvironmentVariable("CONTAINER");
            var container = _client.GetContainer(dbName, containerName);
            var cancellationToken = _messageContext.IncomingStepContext.Load<CancellationToken>();
            await container.UpsertItemAsync(JObject.Parse(message.Data), cancellationToken: cancellationToken);
            Console.WriteLine("Finished bus3 upsert");

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }
    }
}
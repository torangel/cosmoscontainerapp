using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ContainerAppsAndCosmos;
using ContainerAppsAndCosmos.Handlers;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Rebus.Transport.InMem;

Console.WriteLine("Started");
var cosmosCn = Environment.GetEnvironmentVariable("COSMOS");
var dbName = Environment.GetEnvironmentVariable("DATABASE");
var containerName = Environment.GetEnvironmentVariable("CONTAINER");
var less = Environment.GetEnvironmentVariable("LESSDATA");
var useLessData = !String.IsNullOrEmpty(less) && less.ToLower().Equals("true");
var bus = Environment.GetEnvironmentVariable("BUS");
var useBus = !String.IsNullOrEmpty(bus) && bus.ToLower().Equals("true");


var cosmosClient = new CosmosClient(cosmosCn);


var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton(cosmosClient);
builder.Services.AddRebus(configure =>
{
    var configurer = configure.Logging(l => l.Console())
        .Routing(r => r.TypeBased().MapAssemblyOf<Model>("queue"))
        .Transport(t => t.UseInMemoryTransport(new InMemNetwork(true), "queue"));
    return configurer;
});

builder.Services.AutoRegisterHandlersFromAssemblyOf<Model>();

var app = builder.Build();

var data = useLessData ? File.ReadAllText("lessdata.json") : File.ReadAllText("data.json");
var item = JObject.Parse(data);
while (true)
{
    Console.WriteLine($"LessData: {useLessData}");
    Console.WriteLine($"UseBus: {useBus}");
  
    try
    {
        Console.WriteLine($"Start {Const.TAG} upsert");
        var container = cosmosClient.GetContainer(dbName, containerName);
        await container.UpsertItemAsync(item);
        await container.UpsertItemAsync(item);
        Console.WriteLine($"Finished {Const.TAG} upsert");
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }


    if (useBus)
        using (var scope = app.Services.CreateScope())
            await scope.ServiceProvider.GetRequiredService<IBus>().SendLocal(new Model() { Data = data });

    await Task.Delay(TimeSpan.FromSeconds(5));
}
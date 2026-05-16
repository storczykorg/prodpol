var builder = DistributedApplication.CreateBuilder(args);

var pgServer = builder.AddPostgres("pg")
    .WithPgAdmin(configureContainer: resourceBuilder =>
    {
        resourceBuilder.WithLifetime(ContainerLifetime.Persistent);
        resourceBuilder.WithHostPort(2137);
    })
    .WithDataVolume(isReadOnly: false)
    .WithLifetime(ContainerLifetime.Persistent);



var db = pgServer
    .AddDatabase("prodpol", "prodpol");

var backendService = builder
    .AddProject<Projects.StorczykProdpol>("StorczykProdpol")
    .WithReference(db, "postgresdb")
    .WaitFor(db)
    .WithHttpHealthCheck("api/status/health")
    .WithHttpEndpoint(2138)
    .WithHttpsEndpoint(2139);
var frontend = builder.AddViteApp("StorczykFrontend",
        "../Storczyk.Frontend")
    .WithReference(backendService)
    .WaitFor(backendService)
    .WithEnvironment("BROWSER", "none")
    .WithEnvironment("VITE_API_URL", backendService.GetEndpoint("http"))
    .WithHttpEndpoint(env: "VITE_PORT")
    .WithHttpHealthCheck("api/status/health")
    .WithExternalHttpEndpoints();
var workerService = builder.AddProject<Projects.StorczykWorker>("StorczykWorker")
    .WithReference(db, connectionName: "postgresdb")
    .WaitFor(db)
    .WaitFor(frontend);
builder.Build().Run();

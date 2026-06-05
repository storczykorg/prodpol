using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var pgServer = builder.AddPostgres("pg")
    .WithPgAdmin(resourceBuilder => { resourceBuilder.WithHostPort(2137); });


var db = pgServer
    .WithLifetime(ContainerLifetime.Session)
    .AddDatabase("prodpol", "prodpol");

var backendService = builder
    .AddProject<StorczykProdpol>("StorczykProdpol")
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
var workerService = builder.AddProject<StorczykWorker>("StorczykWorker")
    .WithReference(db, "postgresdb")
    .WaitFor(db)
    .WaitFor(frontend);
builder.Build().Run();
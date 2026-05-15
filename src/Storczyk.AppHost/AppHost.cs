var builder = DistributedApplication.CreateBuilder(args);

var db = builder.AddPostgres("pg")
    .AddDatabase("prodpol");


var backendService = builder
    .AddProject<Projects.StorczykProdpol>("StorczykProdpol")
    .WithReference(db)
    .WaitFor(db)
    .WithHttpHealthCheck("api/status/health");
//var workerService = builder.AddProject<Projects.StorczykWorker>("StorczykWorker")
//    .WithReference(db)
//    .WaitFor(db);
var frontend = builder.AddViteApp("StorczykFrontend",
        "../Storczyk.Frontend")
    .WithReference(backendService)
    .WaitFor(backendService)
    .WithEnvironment("BROWSER", "none")
    .WithEnvironment("VITE_API_URL", backendService.GetEndpoint("http"))
    .WithHttpEndpoint(env: "VITE_PORT")
    .WithHttpHealthCheck("api/status/health")
    .WithExternalHttpEndpoints();
builder.Build().Run();

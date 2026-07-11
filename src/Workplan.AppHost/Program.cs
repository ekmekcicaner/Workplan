var builder = DistributedApplication.CreateBuilder(args);

// AppHost kendi PostgreSQL container'ını ve volume'unu yönetir; docker-compose.yml'deki
// "db" servisinden tamamen bağımsızdır (ayrı port, ayrı volume) — ikisi çakışmadan
// aynı anda bile çalışabilir. Aspire = geliştirme, docker compose = tek komutla tam stack.
var postgresUser = builder.AddParameter(
    "postgres-user",
    "postgres",
    publishValueAsDefault: true);
var postgresPassword = builder.AddParameter(
    "postgres-password",
    "postgres",
    publishValueAsDefault: false,
    secret: true);

var postgres = builder.AddPostgres("postgres", postgresUser, postgresPassword)
    .WithImageTag("16-alpine")
    .WithHostPort(5434)
    .WithDataVolume("workplan-dev-db-data");

// Resource adları configuration anahtarlarında case-insensitive olduğundan
// "default" -> ConnectionStrings:Default olarak WebApi tarafından okunur.
var database = postgres.AddDatabase("default", "workplan");

var api = builder.AddProject<Projects.Workplan_WebApi>("api", launchProfileName: "http")
    .WithReference(database)
    .WaitFor(database)
    .WithEnvironment("SeedDemoData", "true")
    .WithHttpHealthCheck("/health/ready")
    .WithExternalHttpEndpoints();

builder.AddProject<Projects.Workplan_Client>("client", launchProfileName: "aspire")
    .WithReference(api)
    .WaitFor(api)
    .WithExternalHttpEndpoints();

builder.Build().Run();

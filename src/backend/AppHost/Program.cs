using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var messaging = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

var db = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .AddDatabase("conduit-db", databaseName: "conduit");

builder.AddProject<WebApi>("conduit-webapi")
    .WithReference(db)
    .WithReference(messaging)
    .WaitFor(db);

builder.Build().Run();

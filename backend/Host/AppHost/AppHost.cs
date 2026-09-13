using Aspire.Hosting.Yarp.Transforms;
using Yarp.ReverseProxy.Transforms;

const string apiPathPrefix = "/api";

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume(isReadOnly: false);
var conduitDb = postgres.AddDatabase("conduit-db");

var rabbitMq = builder.AddRabbitMQ("rabbitmq")
    .WithDataVolume(isReadOnly: false)
    .WithManagementPlugin();

var api = builder.AddProject<Projects.Conduit_Host_WebApi>("api")
    .WithEnvironment("DATABASE_PROVIDER", "postgresql")
    .WithHttpsEndpoint()
    .WithReference(conduitDb)
    .WaitFor(conduitDb)
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq);

#pragma warning disable ASPIRECERTIFICATES001
var viteApp = builder.AddViteApp("ui", "../../../frontend")
    .WithHttpsDeveloperCertificate()
    .WithReference(api);

var gateway = builder.AddYarp("gateway")
    .WithHttpsDeveloperCertificate()
    .WithConfiguration(yarp =>
    {
        yarp.AddRoute($"{apiPathPrefix}/{{**catch-all}}", api)
            .WithTransformPathRemovePrefix(apiPathPrefix)
            .WithTransformXForwarded()
            .WithTransformRequestHeader("X-Forwarded-Prefix", apiPathPrefix, append: false);
        if (builder.ExecutionContext.IsRunMode)
        {
            var viteAppCluster = yarp.AddCluster(viteApp);
            yarp.AddRoute("/{**catch-all}", viteAppCluster);
        }
    })
    .PublishWithStaticFiles(viteApp);
#pragma warning restore ASPIRECERTIFICATES001

builder.Build().Run();

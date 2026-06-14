var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume(isReadOnly: false);
var conduitDb = postgres.AddDatabase("conduit-db");

var api = builder.AddProject<Projects.Conduit>("api")
    .WithEnvironment("DATABASE_PROVIDER", "postgresql")
    .WithHttpsEndpoint()
    .WithReference(conduitDb)
    .WaitFor(conduitDb);

#pragma warning disable ASPIRECERTIFICATES001 // Der Typ dient nur zu Testzwecken und kann in zukünftigen Aktualisierungen geändert oder entfernt werden. Unterdrücken Sie diese Diagnose, um fortzufahren.
var viteApp = builder.AddViteApp("ui", "../../frontend")
    .WithHttpsDeveloperCertificate()
    .WithReference(api);

var gateway = builder.AddYarp("gateway")
    .WithHttpsDeveloperCertificate()
    .WithConfiguration(yarp =>
    {
        yarp.AddRoute("/api/{**catch-all}", api);
        if (builder.ExecutionContext.IsRunMode)
        {
            var viteAppCluster = yarp.AddCluster(viteApp);
            yarp.AddRoute("/{**catch-all}", viteAppCluster);
        }
    })
    .PublishWithStaticFiles(viteApp);
#pragma warning restore ASPIRECERTIFICATES001 // Der Typ dient nur zu Testzwecken und kann in zukünftigen Aktualisierungen geändert oder entfernt werden. Unterdrücken Sie diese Diagnose, um fortzufahren.

builder.Build().Run();

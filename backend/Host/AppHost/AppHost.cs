using Aspire.Hosting.Yarp.Transforms;
using Yarp.ReverseProxy.Transforms;

// The public path prefix under which the API is exposed by the gateway. Kept
// as a single constant so the route pattern and the X-Forwarded-Prefix header
// sent to the API (see Conduit/Program.cs) always stay in sync.
const string apiPathPrefix = "/api";

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume(isReadOnly: false);
var conduitDb = postgres.AddDatabase("conduit-db");

var api = builder.AddProject<Projects.Conduit_Host_WebApi>("api")
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
#pragma warning restore ASPIRECERTIFICATES001 // Der Typ dient nur zu Testzwecken und kann in zukünftigen Aktualisierungen geändert oder entfernt werden. Unterdrücken Sie diese Diagnose, um fortzufahren.

builder.Build().Run();

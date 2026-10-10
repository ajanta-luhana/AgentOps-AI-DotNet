using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AgentOps.Application.Common;
using AgentOps.Infrastructure;
using AgentOps.Worker.Execution;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddOptions<AiOptions>()
    .Bind(builder.Configuration.GetSection(AiOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<WorkerLifetimeService>();

await builder.Build().RunAsync();

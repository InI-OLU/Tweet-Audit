using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tweet_Audit.INFRASTRUCTURE;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<ArchiveTweetPathSettings>(builder.Configuration.GetSection("ArchiveSettings"));

using IHost host = builder.Build();
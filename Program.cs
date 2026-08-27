using CsvHelper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.Json;
using Tweet_Audit.APPLICATION;
using Tweet_Audit.APPLICATION.DTO;
using Tweet_Audit.APPLICATION.INTERFACE;
using Tweet_Audit.DOMAIN;
using Tweet_Audit.DOMAIN.Exceptions;
using Tweet_Audit.INFRASTRUCTURE;


try
{
    var builder = Host.CreateApplicationBuilder(args);
    builder.Services.Configure<ArchiveTweetPathSettings>(builder.Configuration.GetSection("ArchiveSettings"));
    builder.Services.Configure<AlignmentCriteria>(builder.Configuration.GetSection("criteria"));
    builder.Services.Configure<GeminiApiKey>(builder.Configuration.GetSection("GeminiApiKey"));
    builder.Services.Configure<UserName>(builder.Configuration.GetSection("UserName"));

    builder.Services.AddSingleton<ArchiveParser>();
    builder.Services.AddSingleton<PromptBuilder>();
    builder.Services.AddSingleton<IGeminiClient,GeminiClient>();
    builder.Services.AddSingleton<TweetAuditService>();
    builder.Services.AddSingleton<BatchAuditService>();
    builder.Services.AddSingleton<TweetUrlBuilder>();

    using IHost host = builder.Build();
    var auditService = host.Services.GetRequiredService<TweetAuditService>();
    var progress = new Progress<int>(percent =>
    {
        Console.WriteLine($"Auditing Tweets : {percent}%");
    });
    var result = await auditService.TaskServiceOrchestrator(progress);
    Console.WriteLine("Audit Finished");
    var csvOutput = new List<CsvOutput>();

    if (result is null || result.Count == 0)
    {
        Console.WriteLine("No verdicts to display.");
    }
    else
    {
        foreach (var verdict in result)
        {
            var obj = new CsvOutput
            {
                TweetUrl = verdict,
                DeleteStatus = false
            };
            csvOutput.Add(obj);
        }
    }
    string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    string fileName = $"TweetAudit_{DateTime.Now:yyyy-MM-dd_HHmmss}.csv";
    string outputPath = Path.Combine(documentsPath, fileName);

    using (var writer = new StreamWriter(outputPath))
    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
    {
        csv.WriteRecords(csvOutput);
    }

    Console.WriteLine($"CSV written to: {outputPath}");
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    string message = ex switch
    {
        FileNotFoundException => "Couldn't find your archive file...",
        JsonException => "Your config.json has invalid JSON...",
        FatalAuditException => $"Gemini API rejected the request with a non-retryable client error {ex.Message}.",
        OptionsValidationException => $"Config problem: {ex.Message}",
        _ => $"Unexpected error: {ex.Message}"
    };
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Details: {ex.InnerException.Message}");
    }
    Console.WriteLine(message);
    Console.ResetColor();
  
}

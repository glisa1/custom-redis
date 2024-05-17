// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;
using RedisLite.Server;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
CancellationToken token = cancellationTokenSource.Token;

try
{
	var builder = new ConfigurationBuilder()
		.SetBasePath(Directory.GetCurrentDirectory())
		.AddJsonFile("config.json", optional: false);

	IConfiguration config = builder.Build();

	var serverPort = config.GetValue<int>("runningPort");
	var serverHost = config.GetValue<string>("runningAddress");

	var serverConfig = new ServerConfig(serverHost, serverPort);

	var liteServer = new LiteHttpServer(serverConfig);

	Log.Information("Hello world!");
	await liteServer.StartAsync(token);
}
catch (Exception ex)
{
	cancellationTokenSource.Cancel();
	Log.Fatal(ex, "Application terminated unexpectedly.");
}
finally
{
	Log.CloseAndFlush();
}
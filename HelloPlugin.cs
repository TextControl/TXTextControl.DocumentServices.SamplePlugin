//-------------------------------------------------------------------------------------------------------------
// module:          TXTextControl.DocumentServices.SamplePlugin
// copyright:       © 2025 Text Control GmbH
// author:          T. Kummerow
//-------------------------------------------------------------------------------------------------------------
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TXTextControl.DocumentServices.Plugin.Abstractions;
using TXTextControl.DocumentServices.SamplePlugin.Services;

namespace TXTextControl.DocumentServices.SamplePlugin;

public class HelloPlugin : IPlugin {
	public string Name => "Text Control DS Server Sample Plugin";
	public string Description => "Adds a /plugin/hello endpoint and logs requests.";
	public string Version => "1.0.0";

	/// <summary>
	/// The base path of the plugin's web interface. This is used to create a 
	/// link in the DS Server plugin overview.
	/// </summary>
	public string UIBasePath => "/plugin-ui/sample-plugin";

	public void ConfigureServices(IServiceCollection services, PluginContext context) {
		// Register a singleton service to hold the greeting state.
		// This is just an example. Implementing ConfigureServices is optional.
		var greeting = context.Configuration["SamplePlugin:Greeting"] ?? "Hello (default)";
		services.AddSingleton(new GreetingState { Greeting = greeting });
	}

	public void ConfigureMiddleware(WebApplication app, PluginContext context) {
		// Retrieve the GreetingState service from the application's service provider.
		var state = app.Services.GetService<GreetingState>() ?? throw new InvalidOperationException("GreetingState service not registered.");

		// Register a middleware that logs requests to the /plugin/hello endpoint and returns a simple HTML page
		// at the /plugin-ui/sample-plugin path.
		// This is also just an example. Implementing ConfigureMiddleware is optional.
		app.Use(async (ctx, next) => {
			if (ctx.Request.Path.StartsWithSegments("/plugin/hello")) {
				var logger = ctx.RequestServices.GetRequiredService<ILogger<HelloPlugin>>();
				logger.LogInformation("{Name} intercepted request to /plugin/hello", Name);
			}
			await next();
		});
		app.MapGet(UIBasePath!, () => Results.Content(
			$"""
			<html>
				<head><title>Sample Plugin Config</title></head>
				<body>
					<h3>Hello from the sample plugin!</h3>
					<p>This is a plugin-provided UI. Greeting: "{state.Greeting}"</p>
				</body>
			</html>
			""",
			"text/html"
		));
	}

	public void OnStart(IServiceProvider services, PluginContext context) {
		// This method is called when the plugin is started. You can use this to 
		// initialize resources or perform startup logic. Implementing OnStart is optional.
		var logger = services.GetService<ILogger<HelloPlugin>>();
		var state = services.GetService<GreetingState>();
		logger?.LogInformation("{Name} (v{Version}) started. Greeting: {Greeting}", Name, Version, state?.Greeting);
	}

	public void OnStop() {
		// Cleanup logic if needed. This is also optional.
	}
}

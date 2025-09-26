# TXTextControl.DocumentServices.SamplePlugin

This repository contains a sample plugin for [Text Control DS Server](https://dsserver.io/) that demonstrates how to:

- Implement a controller-based plugin using the `TXTextControl.DocumentServices.Plugin.Abstractions` package
- Register custom middleware
- Read configuration settings
- Register plugin-specific services via dependency injection
- Serve a simple HTML configuration page

## 🧩 What It Does

The sample plugin:

- Registers a controller at `/plugin/hello`
- Returns a greeting text from configuration
- Logs each request to `/plugin/hello` using custom middleware
- Uses `IPlugin` lifecycle methods for setup and logging
- Provides a basic HTML configuration page at `/plugin-ui/sample-plugin` using `MapGet`

## 🚀 Getting Started

1. Build the plugin:

```bash
dotnet build
```

2. Create a subfolder inside the `Plugins/` folder of your DS Server installation (e.g. `Plugins/SamplePlugin/`).
3. Copy the resulting `TXTextControl.DocumentServices.SamplePlugin.dll` into that subfolder:

```plaintext
Plugins/
└── SamplePlugin/
    └── TXTextControl.DocumentServices.SamplePlugin.dll
```

4. Optionally extend your DS Server `appsettings.json`:

```json
"SamplePlugin": {
  "Greeting": "Hello from the sample plugin!"
}
```

5. Restart DS Server.

## 🧪 Try It Out

After deployment, you can access the plugin endpoint:

```perl
GET http://<your-ds-server>/plugin/hello
```

You should receive the configured greeting string, and the request will be logged by the plugin's middleware.

## ⚙️ Plugin UI

The plugin also provides a basic web-based configuration page at:

```text
http://<your-ds-server>/plugin-ui/sample-plugin
```

This page is rendered using `MapGet(...)` in the plugin's `ConfigureMiddleware` method, demonstrating how to serve a simple HTML UI from a plugin.

## ⚒️ Source Structure

* `HelloPlugin.cs` — Implements the IPlugin interface
* `Controllers/HelloController.cs` — A minimal API controller
* `Services/GreetingState.cs` — A singleton service registered by the plugin

## 📄 License

This sample is provided under the MIT License. See [LICENSE.md](./LICENSE.md) for details.

## 📣 About Text Control DS Server

[Text Control DS Server](https://www.dsserver.io/) is a powerful on-premise backend for generating, viewing, editing, and signing documents — complete with extensive mail merge and reporting capabilities — accessible via REST APIs or integrated custom logic through plugins like this one. [Try it out today](https://www.dsserver.io/getting-started/) and see how it can enhance your document processing workflows!

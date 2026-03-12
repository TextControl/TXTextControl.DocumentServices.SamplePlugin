# TXTextControl.DocumentServices.SamplePlugin

This repository contains a sample plugin for [Text Control DS Server](https://dsserver.io/) that demonstrates how to:

- Implement a controller-based plugin using the `TXTextControl.DocumentServices.Plugin.Abstractions` package
- Register custom middleware
- Read configuration settings
- Register plugin-specific services via dependency injection
- Consume the DS Server DI services `IDocumentProcessingService` and `IDocumentEditorSessionService`
- Serve a simple HTML configuration page

## 🧩 What It Does

The sample plugin exposes three groups of examples:

- `/plugin/hello` shows a minimal controller with plugin-owned state and middleware logging
- `/plugin/document-processing/*` shows how a plugin controller can use `IDocumentProcessingService`
- `/plugin/document-editor-sessions/*` shows how a plugin controller can work with active editor sessions through `IDocumentEditorSessionService`

## ✨ Included Service Examples

### 1. `IDocumentProcessingService`

The sample [`DocumentProcessingController`](./Controllers/DocumentProcessingController.cs) demonstrates:

- Converting an uploaded base64 document to another DS Server return format
- Retrieving document metadata and merge information
- Generating a barcode image through the barcode sub API

Available endpoints:

```text
POST /plugin/document-processing/convert
POST /plugin/document-processing/getdocumentinfo
GET  /plugin/document-processing/createbarcode?text=...
```

Example request:

```http
POST /plugin/document-processing/convert HTTP/1.1
Content-Type: application/json

{
  "document": "<base64 document>",
  "returnFormat": "PDF",
  "flattenFormFields": true
}
```

### 2. `IDocumentEditorSessionService`

The sample [`DocumentEditorSessionsController`](./Controllers/DocumentEditorSessionsController.cs) demonstrates:

- Resolving a live editor session by `connectionId`
- Loading a base64 document into the current editor session
- Changing the current paragraph background color in the active selection
- Reading the application field at the current input position
- Saving the current editor session back to a selected document format

Available endpoints:

```text
POST /plugin/document-editor-sessions/loaddocument?connectionId=<id>
POST /plugin/document-editor-sessions/setparagraphbackcolor?connectionId=<id>&color=%23FFF59D
GET  /plugin/document-editor-sessions/getcurrentapplicationfield?connectionId=<id>
GET  /plugin/document-editor-sessions/savedocument?connectionId=<id>&format=PDF
```

Example request:

```http
POST /plugin/document-editor-sessions/loaddocument?connectionId=<id> HTTP/1.1
Content-Type: application/json

{
  "document": "<base64 document>",
  "format": "HTML"
}
```

## ⚙️ Plugin UI

The plugin also provides a basic web-based configuration page at:

```text
http://<your-ds-server>/plugin-ui/sample-plugin
```

This page is rendered using `MapGet(...)` in the plugin's `ConfigureMiddleware` method, demonstrating how to serve a simple HTML UI from a plugin.

## 🚀 Getting Started

1. Build the plugin:

```bash
dotnet build
```

2. Create a subfolder inside the `Plugins/` folder of your DS Server installation (for example `Plugins/SamplePlugin/`).
3. Copy the resulting `TXTextControl.DocumentServices.SamplePlugin.dll` into that subfolder:

```text
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

After deployment, try the minimal sample endpoint:

```http
GET http://<your-ds-server>/plugin/hello
```

Then test the processing and editor-session samples with base64-encoded documents and, for the editor endpoints, a valid DS Server editor `connectionId`.

## ⚒️ Source Structure

- `HelloPlugin.cs` - Implements `IPlugin`, middleware setup and the sample plugin UI
- `Controllers/HelloController.cs` - Minimal greeting endpoint
- `Controllers/DocumentProcessingController.cs` - Sample usage of `IDocumentProcessingService`
- `Controllers/DocumentEditorSessionsController.cs` - Sample usage of `IDocumentEditorSessionService`
- `Services/GreetingState.cs` - Singleton service registered by the plugin
- `TXTextControl.DocumentServices.Plugin.Abstractions/` - Reference copy of the public plugin abstractions package

## 📄 License

This sample is provided under the MIT License. See [LICENSE.md](./LICENSE.md) for details.

## 📣 About Text Control DS Server

[Text Control DS Server](https://www.dsserver.io/) is a powerful on-premise backend for generating, viewing, editing, and signing documents — complete with extensive mail merge and reporting capabilities — accessible via REST APIs or integrated custom logic through plugins like this one. [Try it out today](https://www.dsserver.io/getting-started/) and see how it can enhance your document processing workflows!

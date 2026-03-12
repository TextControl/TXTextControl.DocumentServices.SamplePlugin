//-------------------------------------------------------------------------------------------------------------
// module:          TXTextControl.DocumentServices.SamplePlugin
// copyright:       © 2025 Text Control GmbH
// author:          T. Kummerow
//-------------------------------------------------------------------------------------------------------------
using Microsoft.AspNetCore.Mvc;
using TXTextControl.DocumentServices.DocumentEditor.Abstractions;
using TXTextControl.DocumentServices.DocumentEditor.Enums;
using TXTextControl.DocumentServices.DocumentEditor.Options;
using TXTextControl.DocumentServices.Plugin.Abstractions.DocumentEditor.Models;

namespace TXTextControl.DocumentServices.SamplePlugin.Controllers;

[ApiController]
[Route("plugin/document-editor-sessions/[action]")]
public class DocumentEditorSessionsController : ControllerBase {
	private readonly IDocumentEditorSessionService m_sessionService;

	public DocumentEditorSessionsController(IDocumentEditorSessionService sessionService) {
		m_sessionService = sessionService;
	}

	[HttpPost]
	public async Task<IActionResult> LoadDocument(
		[FromQuery] string connectionId,
		[FromBody] LoadDocumentRequest request,
		CancellationToken cancellationToken) {
		if (!TryGetSession(connectionId, out var session, out var notFoundResult)) {
			return notFoundResult;
		}

		if (!TryDecodeBase64(request.Document, out var documentBytes, out var badRequest)) {
			return badRequest;
		}

		if (!Enum.TryParse<DocumentFormat>(request.Format, true, out var documentFormat)) {
			return BadRequest($"Unknown document format '{request.Format}'.");
		}

		LoadOptions loadOptions = LoadOptions.FromDocumentFormat(documentFormat, documentBytes);
		await session.LoadAsync(loadOptions, cancellationToken);

		return Ok(new {
			ConnectionId = connectionId,
			Message = $"Loaded a {documentFormat} document into the editor session."
		});
	}

	[HttpPost]
	public async Task<IActionResult> SetParagraphBackColor(
		[FromQuery] string connectionId,
		[FromQuery] string color = "#FFF59D",
		CancellationToken cancellationToken = default) {
		if (!TryGetSession(connectionId, out var session, out var notFoundResult)) {
			return notFoundResult;
		}

		await session.Selection.ParagraphFormat.SetBackColorAsync(color, cancellationToken);

		return Ok(new {
			ConnectionId = connectionId,
			Message = $"Updated the current paragraph background color to '{color}'."
		});
	}

	[HttpGet]
	public async Task<IActionResult> GetCurrentApplicationField(
		[FromQuery] string connectionId,
		CancellationToken cancellationToken = default) {
		if (!TryGetSession(connectionId, out var session, out var notFoundResult)) {
			return notFoundResult;
		}

		IApplicationField? applicationField = await session.ApplicationFields.GetItemAsync(cancellationToken);
		if (applicationField is null) {
			return NotFound("No application field is available at the current input position.");
		}

		return Ok(new {
			ConnectionId = connectionId,
			Name = await applicationField.GetNameAsync(cancellationToken),
			TypeName = await applicationField.GetTypeNameAsync(cancellationToken),
			Text = await applicationField.GetTextAsync(cancellationToken)
		});
	}

	[HttpGet]
	public async Task<IActionResult> SaveDocument(
		[FromQuery] string connectionId,
		[FromQuery] string format = "TX",
		CancellationToken cancellationToken = default) {
		if (!TryGetSession(connectionId, out var session, out var notFoundResult)) {
			return notFoundResult;
		}

		if (!Enum.TryParse<DocumentFormat>(format, true, out var documentFormat)) {
			return BadRequest($"Unknown document format '{format}'.");
		}

		SavedDocument savedDocument = await session.SaveAsync(new SaveOptions(documentFormat), cancellationToken);
		return File(savedDocument.Content, GetContentType(savedDocument.Format), GetFileName("session-document", savedDocument.Format));
	}

	private bool TryGetSession(
		string connectionId,
		out IDocumentEditorSession session,
		out NotFoundObjectResult notFoundResult) {
		if (m_sessionService.TryGetSession(connectionId, out session!)) {
			notFoundResult = null!;
			return true;
		}

		session = null!;
		notFoundResult = NotFound($"No editor session was found for connection ID '{connectionId}'.");
		return false;
	}

	private static bool TryDecodeBase64(string base64, out byte[] documentBytes, out BadRequestObjectResult badRequest) {
		try {
			documentBytes = System.Convert.FromBase64String(base64);
			badRequest = null!;
			return true;
		}
		catch (FormatException) {
			documentBytes = Array.Empty<byte>();
			badRequest = new BadRequestObjectResult("The request payload must contain a valid base64 encoded document.");
			return false;
		}
	}

	private static string GetContentType(DocumentFormat documentFormat) => documentFormat switch {
		DocumentFormat.PDF or DocumentFormat.PDFA => "application/pdf",
		DocumentFormat.DOCX => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
		DocumentFormat.RTF => "application/rtf",
		DocumentFormat.HTML => "text/html",
		DocumentFormat.PlainText => "text/plain",
		_ => "application/octet-stream",
	};

	private static string GetFileName(string baseName, DocumentFormat documentFormat) => documentFormat switch {
		DocumentFormat.TX => $"{baseName}.tx",
		DocumentFormat.RTF => $"{baseName}.rtf",
		DocumentFormat.PlainText => $"{baseName}.txt",
		DocumentFormat.HTML => $"{baseName}.html",
		DocumentFormat.DOCX => $"{baseName}.docx",
		DocumentFormat.PDF => $"{baseName}.pdf",
		DocumentFormat.PDFA => $"{baseName}.pdf",
		DocumentFormat.XLSX => $"{baseName}.xlsx",
		_ => $"{baseName}.bin",
	};
}

public sealed record LoadDocumentRequest(string Document, string Format = "HTML");

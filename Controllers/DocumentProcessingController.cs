//-------------------------------------------------------------------------------------------------------------
// module:          TXTextControl.DocumentServices.SamplePlugin
// copyright:       © 2025 Text Control GmbH
// author:          T. Kummerow
//-------------------------------------------------------------------------------------------------------------
using Microsoft.AspNetCore.Mvc;
using TXTextControl.DocumentServices.DocumentProcessing.Abstractions;
using TXTextControl.DocumentServices.DocumentProcessing.Models;

namespace TXTextControl.DocumentServices.SamplePlugin.Controllers;

[ApiController]
[Route("plugin/document-processing/[action]")]
public class DocumentProcessingController : ControllerBase {
	private readonly IDocumentProcessingService m_processingService;

	public DocumentProcessingController(IDocumentProcessingService processingService) {
		m_processingService = processingService;
	}

	[HttpPost]
	public async Task<IActionResult> Convert([FromBody] ConvertDocumentRequest request, CancellationToken cancellationToken) {
		if (!TryDecodeBase64(request.Document, out var documentBytes, out var badRequest)) {
			return badRequest;
		}

		if (!Enum.TryParse<ReturnFormat>(request.ReturnFormat, true, out var returnFormat)) {
			return BadRequest($"Unknown return format '{request.ReturnFormat}'.");
		}

		byte[] convertedDocument = await m_processingService.ConvertAsync(
			documentBytes,
			returnFormat,
			request.FlattenFormFields,
			cancellationToken);

		return File(convertedDocument, GetContentType(returnFormat), GetFileName("converted", returnFormat));
	}

	[HttpPost]
	public async Task<IActionResult> GetDocumentInfo([FromBody] Base64DocumentRequest request, CancellationToken cancellationToken) {
		if (!TryDecodeBase64(request.Document, out var documentBytes, out var badRequest)) {
			return badRequest;
		}

		DocumentInfo documentInfo = await m_processingService.GetDocumentInfoAsync(documentBytes, cancellationToken);
		return Ok(documentInfo);
	}

	[HttpGet]
	public async Task<IActionResult> CreateBarcode(
		[FromQuery] string text = "https://www.textcontrol.com",
		CancellationToken cancellationToken = default) {
		byte[] image = await m_processingService.Barcodes.CreateAsync(new BarcodeSettings(text), ct: cancellationToken);
		return File(image, "image/png", "barcode.png");
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

	private static string GetContentType(ReturnFormat returnFormat) => returnFormat switch {
		ReturnFormat.PDF or ReturnFormat.PDFA => "application/pdf",
		ReturnFormat.DOC => "application/msword",
		ReturnFormat.DOCX => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
		ReturnFormat.RTF => "application/rtf",
		ReturnFormat.HTML => "text/html",
		ReturnFormat.TXT => "text/plain",
		_ => "application/octet-stream",
	};

	private static string GetFileName(string baseName, ReturnFormat returnFormat) => returnFormat switch {
		ReturnFormat.PDF => $"{baseName}.pdf",
		ReturnFormat.PDFA => $"{baseName}.pdf",
		ReturnFormat.DOC => $"{baseName}.doc",
		ReturnFormat.DOCX => $"{baseName}.docx",
		ReturnFormat.RTF => $"{baseName}.rtf",
		ReturnFormat.TX => $"{baseName}.tx",
		ReturnFormat.TXT => $"{baseName}.txt",
		ReturnFormat.HTML => $"{baseName}.html",
		_ => $"{baseName}.bin",
	};
}

public sealed record Base64DocumentRequest(string Document);

public sealed record ConvertDocumentRequest(
	string Document,
	string ReturnFormat = "PDF",
	bool FlattenFormFields = true);

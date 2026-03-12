//-------------------------------------------------------------------------------------------------------------
// module:          TXTextControl.DocumentServices.SamplePlugin
// copyright:       © 2026 Text Control GmbH
// author:          T. Kummerow
//-------------------------------------------------------------------------------------------------------------
using Microsoft.AspNetCore.Mvc;
using TXTextControl.DocumentServices.SamplePlugin.Services;

namespace TXTextControl.DocumentServices.SamplePlugin.Controllers;

[ApiController]
[Route("plugin/[controller]")]
public class HelloController : ControllerBase {
	private readonly GreetingState m_state;

	public HelloController(GreetingState state) {
		m_state = state;
	}

	[HttpGet]
	public string Get() => m_state.Greeting;
}

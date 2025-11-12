using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace ObituaryMvcApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ChatController : ControllerBase {
  private readonly ILogger<ChatController> _logger;
  private readonly IChatClient _chatClient;

  private readonly IConfiguration? _configuration;
  public ChatController(
    ILogger<ChatController> logger,
    IChatClient chatClient,
    IConfiguration configuration
  ) {
    _logger = logger;
    _chatClient = chatClient;
    _configuration = configuration;
  }

  [HttpPost(Name = "Chat")]
  public async Task<string> Chat([FromBody] string message) {
    // Get the MCP server URL from configuration (Aspire service discovery will provide this)
    var mcpServiceUri = _configuration?["services:mcpserver:http:0"] 
                        ?? _configuration?["AI:MCPServiceUri"] 
                        ?? "http://localhost:7218";
    
    // Remove /sse suffix if present, as MapMcp() handles routing
    mcpServiceUri = mcpServiceUri.TrimEnd('/').Replace("/sse", "");
    
    // Create MCP client connecting to our MCP server
    var mcpClient = await McpClientFactory.CreateAsync(
      new SseClientTransport(
          new SseClientTransportOptions {
              Endpoint = new Uri($"{mcpServiceUri}/sse")
          }
      )
    );
    // Get available tools from the MCP server
    var tools = await mcpClient.ListToolsAsync();

    // Set up the chat messages
    var messages = new List<ChatMessage> {
      new ChatMessage(ChatRole.System, "You are a helpful assistant.")
    };
    messages.Add(new(ChatRole.User, message));

    // Get streaming response and collect updates
    List<ChatResponseUpdate> updates = [];
    StringBuilder result = new StringBuilder();

    await foreach (var update in _chatClient.GetStreamingResponseAsync(
      messages,
      new() { Tools = [.. tools] }
    )) {
      result.Append(update);
      updates.Add(update);
    }
    
    // Add the assistant's responses to the message history
    messages.AddMessages(updates);
    return result.ToString();
  }
}

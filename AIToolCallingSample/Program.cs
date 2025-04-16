using AIToolCallingSample;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using System.ClientModel;

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

string? model = config["ModelName"] ?? throw new InvalidOperationException("ModelName is not defined in the secrets");
string key = config["OpenAIKey"] ?? throw new InvalidOperationException("OpenAIKey is not defined in the secrets");
string endpoint = config["Endpoint"] ?? throw new InvalidOperationException("Endpoint is not defined in the secrets");

// Create the IChatClient
IChatClient chatClient = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(key))
    .GetChatClient(model ?? "gpt-4o")
    .AsIChatClient();

var chatHistory = new List<ChatMessage>();

var options = new ChatOptions
{
    Tools = [new ImportOrchardCoreRecipeTool()]
};

while (true)
{
    Console.WriteLine("Your prompt:");
    var userPrompt = Console.ReadLine();
    chatHistory.Add(new ChatMessage(ChatRole.User, userPrompt));

    Console.WriteLine("AI Response:");
    var response = "";
    await foreach (var item in chatClient.GetStreamingResponseAsync(chatHistory, options, CancellationToken.None))
    {
        Console.Write(item.Text);
        response += item.Text;
    }
    chatHistory.Add(new ChatMessage(ChatRole.Assistant, response));
    Console.WriteLine();
}

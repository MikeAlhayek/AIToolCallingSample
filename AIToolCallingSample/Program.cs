using AIToolCallingSample;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using System.ClientModel;

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

var model = config["ModelName"] ?? throw new InvalidOperationException("ModelName is not defined in the secrets");
var key = config["OpenAIKey"] ?? throw new InvalidOperationException("OpenAIKey is not defined in the secrets");
var endpoint = config["Endpoint"] ?? throw new InvalidOperationException("Endpoint is not defined in the secrets");

var client = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(key))
    .GetChatClient(model)
    .AsIChatClient();

var builder = new ChatClientBuilder(client)
    .UseFunctionInvocation();

var chatHistory = new List<ChatMessage>();

var options = new ChatOptions
{
    Tools = [new ImportOrchardCoreRecipeTool()],
};

var chatClient = builder.Build();

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

using Microsoft.Extensions.AI;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AIToolCallingSample;

public sealed class ImportOrchardCoreRecipeTool : AIFunction
{
    public override string Name => "import_recipe";

    public override string Description => "Imports a dynamic OrchardCore JSON recipe";

    public override JsonElement JsonSchema { get; }

    public ImportOrchardCoreRecipeTool()
    {
        JsonSchema = JsonSerializer.Deserialize<JsonElement>(
            """
            {
                "type": "object",
                "properties": {
                    "recipe": {
                        "type": "string",
                        "description": "A JSON object representing an OrchardCore recipe"
                    }
                },
                "additionalProperties": false,
                "required": ["recipe"]
            }
            """, JsonSerializerOptions);
    }

    protected override ValueTask<object?> InvokeCoreAsync(AIFunctionArguments arguments, CancellationToken cancellationToken)
    {
        Console.WriteLine($"The {Name} function was invoked.");

        ArgumentNullException.ThrowIfNull(arguments);

        if (!arguments.TryGetValue("recipe", out var data))
        {
            return ValueTask.FromResult<object?>("No recipe was given.");
        }

        var recipe = ConvertToJsonObject(data!);

        if (recipe == null)
        {
            return ValueTask.FromResult<object?>("Unable to import recipe.");
        }

        return ValueTask.FromResult<object?>("Recipe was imported successfully.");
    }

    private static JsonObject? ConvertToJsonObject(object data)
    {
        JsonObject? recipe = null;

        if (data is JsonObject obj)
        {
            recipe = obj;
        }
        else
        {
            string? json;

            if (data is JsonElement jsonElement)
            {
                json = jsonElement.GetString();
            }
            else
            {
                json = data.ToString();
            }

            try
            {
                recipe = JsonSerializer.Deserialize<JsonObject>(json);
            }
            catch { }
        }

        return recipe;
    }
}

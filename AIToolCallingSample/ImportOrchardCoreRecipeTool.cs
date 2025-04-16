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
        var metadata = new JsonObject
        {
            {"type", "object"},
            {"properties", new JsonObject
                {
                    { "recipe", new JsonObject
                        {
                            {"type", "object" },
                            {"description", "A JSON object representing an OrchardCore recipe" },
                        }
                    }
                }
            },
            {"required", new JsonArray("recipe") },
            {"return_type", new JsonObject
                {
                    {"type", "boolean"},
                    {"description", "The result of the import process. True representing a successful import while false failed."},
                }
            },
        };

        JsonSchema = JsonSerializer.Deserialize<JsonElement>(metadata, JsonSerializerOptions);
    }

    protected override ValueTask<object?> InvokeCoreAsync(AIFunctionArguments arguments, CancellationToken cancellationToken)
    {
        Console.WriteLine($"The {Name} function was invoked.");

        ArgumentNullException.ThrowIfNull(arguments);

        if (!arguments.TryGetValue("recipe", out var data))
        {
            return ValueTask.FromResult<object?>(false);
        }

        var recipe = ConvertToJsonObject(data!);

        if (recipe == null)
        {
            return ValueTask.FromResult<object?>(false);
        }

        return ValueTask.FromResult<object?>(true);
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

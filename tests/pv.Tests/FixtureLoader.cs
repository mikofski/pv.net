using System.Text.Json;
using System.Text.Json.Serialization;

namespace pv.Tests;

public sealed class FixtureFile<TInputs, TExpected>
{
    [JsonPropertyName("pvlib_version")]
    public string PvlibVersion { get; init; } = string.Empty;

    [JsonPropertyName("cases")]
    public List<FixtureCase<TInputs, TExpected>> Cases { get; init; } = [];
}

public sealed class FixtureCase<TInputs, TExpected>
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("inputs")]
    public TInputs? Inputs { get; init; }

    [JsonPropertyName("expected")]
    public TExpected? Expected { get; init; }
}

public static class FixtureLoader
{
    private static readonly JsonSerializerOptions Options = CreateOptions();

    public static FixtureFile<TInputs, TExpected> Load<TInputs, TExpected>(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName);
        using var stream = File.OpenRead(path);
        return JsonSerializer.Deserialize<FixtureFile<TInputs, TExpected>>(stream, Options)
               ?? throw new InvalidDataException($"Fixture '{fileName}' is empty.");
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new DoubleNaNConverter());
        return options;
    }

    private sealed class DoubleNaNConverter : JsonConverter<double>
    {
        public override bool HandleNull => true;

        public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            reader.TokenType == JsonTokenType.Null ? double.NaN : reader.GetDouble();

        public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
        {
            if (double.IsNaN(value))
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteNumberValue(value);
        }
    }
}

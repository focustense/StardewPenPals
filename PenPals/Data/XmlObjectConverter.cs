using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace PenPals.Data;

// StardewValley.Object will fail JSON serialize due to cycles in the object graph - among other
// reasons, probably.
//
// One way around this would be to define an "ObjectData" class with only the fields we want, and
// this would probably even be the most efficient; but it is also going to be the most error-prone,
// and prone to breaking with future game updates or with other mods, for example if the object has
// some modData that's important.
//
// Using the XML serializer, which the game itself uses for savegame data, should give us the best
// guarantee of a perfect round-trip conversion, even if it is a little slower and takes up more
// space in the save file.
internal class XmlObjectConverter : JsonConverter<SObject>
{
    private static readonly XmlSerializer XmlSerializer = new(typeof(SObject));

    public override SObject? ReadJson(
        JsonReader reader,
        Type objectType,
        SObject? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        return reader.TokenType switch
        {
            JsonToken.Null => null,
            JsonToken.String => (SObject?)
                XmlSerializer.Deserialize(new StringReader((string)reader.Value!)),
            _ => throw new FormatException(
                $"Can't parse {typeof(SObject).FullName} from a {reader.TokenType} JSON token."
            ),
        };
    }

    public override void WriteJson(JsonWriter writer, SObject? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
            return;
        }
        var sb = new StringBuilder();
        XmlSerializer.Serialize(new StringWriter(sb), value);
        writer.WriteValue(sb.ToString());
    }
}

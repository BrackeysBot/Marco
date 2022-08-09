using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Marco.Data.ValueConverters;

internal sealed class StringListToBytesConverter : ValueConverter<List<string>, byte[]>
{
    public StringListToBytesConverter()
        : this(null)
    {
    }

    public StringListToBytesConverter(ConverterMappingHints? mappingHints)
        : base(v => ToBytes(v), v => FromBytes(v), mappingHints)
    {
    }

    private static List<string> FromBytes(byte[] buffer)
    {
        using var stream = new MemoryStream(buffer);
        using var reader = new BinaryReader(stream);

        int count = reader.ReadInt32();
        var result = new List<string>(count);

        for (var i = 0; i < count; i++)
            result.Add(reader.ReadString());

        return result;
    }

    private static byte[] ToBytes(List<string> list)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        if (list is not {Count: > 0})
        {
            writer.Write(0);
        }
        else
        {
            writer.Write(list.Count);
            foreach (string item in list)
                writer.Write(item);
        }

        return stream.ToArray();
    }
}

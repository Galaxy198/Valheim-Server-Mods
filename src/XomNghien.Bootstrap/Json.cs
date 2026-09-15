using System.IO;
using System.Runtime.Serialization.Json;

namespace XomNghien.Bootstrap;

internal static class Json
{
    public static T Read<T>(byte[] bytes)
    {
        // Thunderstore package manifests are commonly UTF-8 with a BOM.
        // DataContractJsonSerializer does not accept that marker when the
        // input is supplied as a byte stream, so skip it at the JSON boundary.
        var offset = bytes.Length >= 3
            && bytes[0] == 0xEF
            && bytes[1] == 0xBB
            && bytes[2] == 0xBF
            ? 3
            : 0;
        using var stream = new MemoryStream(bytes, offset, bytes.Length - offset, writable: false);
        return (T)new DataContractJsonSerializer(typeof(T)).ReadObject(stream)!;
    }

    public static T ReadFile<T>(string path) => Read<T>(File.ReadAllBytes(path));

    public static byte[] Write<T>(T value)
    {
        using var stream = new MemoryStream();
        new DataContractJsonSerializer(typeof(T)).WriteObject(stream, value);
        return stream.ToArray();
    }

    public static void WriteFile<T>(string path, T value)
    {
        var temporary = path + ".tmp";
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(temporary, Write(value));
        AtomicFile.Replace(temporary, path);
    }
}

using System.Text;
using System.Text.Json;

namespace Compartilhei.Application.Photos.Gallery;

public static class PhotoGalleryCursorCodec
{
    public static string Encode(PhotoGalleryCursor cursor)
    {
        var json = JsonSerializer.Serialize(cursor);

        return Convert.ToBase64String(
            Encoding.UTF8.GetBytes(json));
    }

    public static PhotoGalleryCursor? Decode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        try
        {
            var bytes = Convert.FromBase64String(value);
            var json = Encoding.UTF8.GetString(bytes);

            return JsonSerializer.Deserialize<PhotoGalleryCursor>(
                json);
        }
        catch (FormatException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static bool TryDecode(
        string cursor,
        out PhotoGalleryCursor? result)
    {
        result = null;

        try
        {
            var json = Encoding.UTF8.GetString(
                Convert.FromBase64String(cursor));

            result = JsonSerializer.Deserialize<PhotoGalleryCursor>(json);

            return result is not null;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
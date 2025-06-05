using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Media.Control;
using Windows.Storage.Streams;
using System.Text.Json;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length == 0 || args[0] == "-?" || args[0] == "--help")
        {
            Console.WriteLine("Usage: QuickSong.exe [option]");
            Console.WriteLine("Options:");
            Console.WriteLine("  -name         : Print the current song title");
            Console.WriteLine("  -artist       : Print the current artist name");
            Console.WriteLine("  -description  : Print \"title by artist\"");
            Console.WriteLine("  -icon         : Print album art as base64 data (png)");
            Console.WriteLine("  -all          : Print all data as json");
            Console.WriteLine("  -? or --help  : Show this help message");
            return;
        }

        var sessions = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
        var session = sessions.GetCurrentSession();
        if (session == null)
        {
            Console.WriteLine("No active media session");
            return;
        }

        var props = await session.TryGetMediaPropertiesAsync();
        string title = props.Title;
        string artist = props.Artist;

        switch (args[0].ToLower())
        {
            case "-name":
                Console.WriteLine(title);
                break;
            case "-artist":
                Console.WriteLine(artist);
                break;
            case "-description":
                Console.WriteLine($"{title} by {artist}");
                break;
            case "-icon":
                if (props.Thumbnail != null)
                {
                    string? base64Thumbnail = null;
                    if (props.Thumbnail != null)
                    {
                        using var stream = await props.Thumbnail.OpenReadAsync();
                        using var ms = new MemoryStream();
                        await stream.AsStreamForRead().CopyToAsync(ms);
                        base64Thumbnail = Convert.ToBase64String(ms.ToArray());
                    }

                    Console.WriteLine(base64Thumbnail);
                }
                else
                {
                    Console.WriteLine("No thumbnail available");
                }
                break;
            case "-all":
                {
                    string? base64Thumbnail = null;
                    if (props.Thumbnail != null)
                    {
                        using var stream = await props.Thumbnail.OpenReadAsync();
                        using var ms = new MemoryStream();
                        await stream.AsStreamForRead().CopyToAsync(ms);
                        base64Thumbnail = Convert.ToBase64String(ms.ToArray());
                    }

                    var allData = new
                    {
                        Title = title,
                        Artist = artist,
                        Description = $"{artist} - {title}",
                        ThumbnailBase64 = base64Thumbnail
                    };

                    string json = JsonSerializer.Serialize(allData);
                    Console.WriteLine(json);
                    break;
                }
            default:
                Console.WriteLine("Unknown argument");
                break;
        }
    }
}
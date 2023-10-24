using ImageMagick;
using System.IO;
using System.Net;

namespace Emma.Stream;

public class EmoteCache
{
    private readonly Dictionary<string, Image> Cache = new();

    public Image LoadTwitch(string key, int size)
    {
        if (!Cache.ContainsKey(key))
        {
            try
            {
                using var client = new WebClient();
                string tempfile = @"C:\Users\huggl\emotes\" + key + ".gif";

                if (!File.Exists(tempfile))
                {
                    string url = @"https://static-cdn.jtvnw.net/emoticons/v2/" + key + "/default/light/3.0";
                    client.DownloadFile(new Uri(url), tempfile);

                    using var collection = new MagickImageCollection(tempfile);
                    collection.Coalesce();

                    foreach (var image in collection)
                    {
                        image.Resize(size, size);
                    }

                    collection.Write(tempfile);
                }

                Cache[key] = Image.FromFile(tempfile);
            }
            catch
            {
                Cache[key] = new Bitmap(1, 1);
            }
        }

        return Cache[key];
    }

  
    public Image? LoadCustom(string key)
    {
        if (!Cache.ContainsKey(key))
        {
            string emoteFile = @"C:\Users\huggl\emotes\7tv\" + key + ".gif";

            if (File.Exists(emoteFile))
            {
                Cache[key] = Image.FromFile(emoteFile);
                return Cache[key];
            }

            return null;
        }

        return Cache[key];
    }


    public Image? Get(string key)
    {
        return Cache.GetValueOrDefault(key);
    }
}

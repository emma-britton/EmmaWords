﻿using ImageMagick;
using System.IO;
using System.Net.Http;

namespace Emma.Stream;

public class EmoteCache
{
    private readonly Dictionary<string, Image> Cache = [];

    public Image LoadTwitch(string key, uint size)
    {
        if (!Cache.ContainsKey(key))
        {
            try
            {
                using var client = new HttpClient();
                string tempfile = @"C:\Users\emmab\OneDrive\Emma\streaming\emotecache\" + key + ".gif";

                if (!File.Exists(tempfile))
                {
                    string url = @"https://static-cdn.jtvnw.net/emoticons/v2/" + key + "/default/light/3.0";
                    var response = client.GetAsync(url).Result;
                    response.EnsureSuccessStatusCode();
                    var fileBytes = response.Content.ReadAsByteArrayAsync().Result;
                    File.WriteAllBytes(tempfile, fileBytes);

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
        if (!Cache.TryGetValue(key, out Image? value))
        {
            string emoteFile = @"C:\Users\emmab\OneDrive\Emma\streaming\emotecache\7tv\" + key + ".gif";

            if (File.Exists(emoteFile))
            {
                Cache[key] = Image.FromFile(emoteFile);
                return Cache[key];
            }

            return null;
        }

        return value;
    }

    public Image? Get(string key)
    {
        return Cache.GetValueOrDefault(key);
    }
}

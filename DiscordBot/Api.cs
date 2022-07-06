using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace DiscordBot
{
    internal class Api
    {
        private Uri _Uri;

        public Api(string URL)
        {
            if (!Uri.TryCreate(URL, UriKind.Absolute, out _Uri))
            {
                throw new UriFormatException();
            }
        }

        public  void PostData(WebhookObject data)
        {
            using (WebClient wc = new WebClient())
            {
                wc.Headers.Add(HttpRequestHeader.ContentType, "application/json");

                wc.UploadStringAsync(_Uri, JsonConvert.SerializeObject(data));        
            }
        }

        public struct WebhookObject
        {
            public string content;
            public string username;
            public string avatar_url;
            public bool tts;
            public Embed[] embeds;
            public string payload_json;
        }

        public struct Embed
        {
            public string title;
            public string type;
            public string description;
            public string url;
            public int color;
            public Footer footer;
            public Image image;
            public Thumbnail thumbnail;
            public Video video;
            public Provider provider;
            public Author author;
            public Field[] fields;
        }

        public struct Field
        {
            public string name;
            public string value;
            public bool inline;
        }

        public struct Footer
        {
            public string text;
            public string icon_url;
            public string proxy_icon_url;
        }

        public struct Image
        {
            public string url;
            public string proxy_url;
            public int height;
            public int width;
        }

        public struct Thumbnail
        {
            public string url;
            public string proxy_url;
            public int height;
            public int width;
        }

        public struct Video
        {
            public string url;
            public int height;
            public int width;
        }

        public struct Provider
        {
            public string name;
            public string url;
        }

        public struct Author
        {
            public string name;
            public string url;
            public string icon_url;
            public string proxy_icon_url;
        }
    }
}

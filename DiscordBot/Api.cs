//#define Windows
using InfinityScript;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace DiscordBot
{
    internal class Api
    {
        private Uri _Uri;

        internal Api(string URL)
        {
            if (!Uri.TryCreate(URL, UriKind.Absolute, out _Uri))
            {
                throw new UriFormatException();
            }
        }

        internal void PostData(WebhookObject data)
        {
#if Windows
            var fullPath = System.IO.Path.Combine(Environment.SystemDirectory, "curl.exe");

            if (File.Exists(fullPath))
            {
                using(Process proc = new Process())
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = fullPath,
                        Arguments = $"-H \"Content-Type: application/json\" -v -s -d \"{JsonConvert.SerializeObject(data).Replace("\"", "\\\"")}\" {_Uri}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = false,
                        WorkingDirectory = Environment.SystemDirectory
                    };

                    proc.StartInfo = startInfo;
                    
                    proc.Start();
                    Log.Debug(proc.StandardOutput.ReadToEnd());
                }
            }
#else
            using (Process proc = new Process())
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "/usr/bin/curl",
                    Arguments = $"-s -H \"Content-Type: application/json\" -d \"{JsonConvert.SerializeObject(data).Replace("\"", "\\\"")}\" {_Uri}",
                    UseShellExecute = false,
                    CreateNoWindow = true,

                };

                proc.StartInfo = startInfo;

                proc.Start();
            }
#endif
        }

        internal struct WebhookObject
        {
            public string content;
            public string username;
        }
    }
}
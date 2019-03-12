using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfinityScript;
using System.Collections;
using Andromeda;
using System.Net;
using Newtonsoft.Json;

namespace Updater
{
    [Plugin]
    public class Main
    {
        private const string versionsStringUrl = "http://www.oofmate.test";

        private static byte[] downloadBytes(string url)
        {
            throw new NotImplementedException();
        }

        [EntryPoint]
        private static void Init()
            => Update();

        private static IEnumerator Update()
        {
            Log.Info("Checking for updates...");

            yield return Async.Detach();

            var versionBytes = downloadBytes(versionsStringUrl);

            var versions = JsonConvert.DeserializeObject<VersionInfo[]>(Encoding.UTF8.GetString(versionBytes));

            var md5 = System.Security.Cryptography.MD5.Create();

            var updates = versions.Where(update =>
            {
                var currentMd5 = md5.ComputeHash(System.IO.File.ReadAllBytes(update.FilePath));

                return currentMd5 != update.Md5Hash;
            }).ToList();

            if (updates.Any())
            {
                Log.Info("Updates found:");
                foreach (var update in versions)
                {
                    Log.Info($"Updating {System.IO.Path.GetFileNameWithoutExtension(update.FilePath)}.");

                    var bytes = downloadBytes(update.DownloadUrl);

                    if (md5.ComputeHash(versionBytes) != update.Md5Hash)
                    {
                        Log.Info($"Failed updating \"{update.FilePath}\"");
                        Log.Info($"Mismatching md5 hash");

                        continue;
                    }

                    var oldFilePath = $"{update.FilePath}.old";

                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);

                    System.IO.File.Move(update.FilePath, oldFilePath);

                    System.IO.File.WriteAllBytes(update.FilePath, downloadBytes(update.DownloadUrl));
                }
            }
            else
            {
                Log.Info("No updates found!");
            }

            yield return Async.Attach();

            Log.Info("Update checking complete");
        }
    }
}

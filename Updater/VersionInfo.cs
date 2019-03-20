using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Updater
{
    internal struct VersionInfo
    {
        public string FilePath;

        public byte[] Md5Hash;

        public string DownloadUrl;
    }
}

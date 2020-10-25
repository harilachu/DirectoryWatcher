using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using MT.DirectoryWatcher.Common;

namespace MT.DirectoryWatcher.Domain
{
    public class Sha256HashGenerator : IHashGenerator
    {
        private readonly ILogger<Sha256HashGenerator> _logger;

        public Sha256HashGenerator(ILogger<Sha256HashGenerator> logger)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public byte[] GetHashFromData(string data)
        {
            if (data == null) throw new ArgumentException(@"data is null", nameof(data));

            byte[] hashValue = new byte[256];
            try
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    var encodedData = Encoding.UTF8.GetBytes(data);
                    hashValue = sha256.ComputeHash(encodedData);
                }
            }
            catch (Exception e)
            {
                this._logger.LogError(e, e.Message);
            }

            return hashValue;
        }

        public byte[] GetHashFromFileStream(FileStream stream)
        {
            if (stream == null) throw new ArgumentException(@"stream is null", nameof(stream));

            byte[] hashValue = new byte[256];
            try
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    stream.Position = 0;
                    hashValue = sha256.ComputeHash(stream);
                }
            }
            catch (Exception e)
            {
                this._logger.LogError(e, e.Message);
            }

            return hashValue;
        }
    }
}

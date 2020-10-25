using System;
using System.IO;
using System.Linq;
using System.Reflection;
using MT.DirectoryWatcher.Common;
using Newtonsoft.Json;

namespace MT.DirectoryWatcher.Domain
{
    public static class DirectoryConfigurator
    {
        private const string ConstResourceName = "DirectoryList.json";

        public static DirectoryList GetDirectoryList()
        {
            DirectoryList directoryList;
            var filePath = GetExecutingDirectory() + "\\" + ConstResourceName;
            if (!File.Exists(filePath)) return null;

            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string json = reader.ReadToEnd();
                    directoryList = JsonConvert.DeserializeObject<DirectoryList>(json);
                }
            }

            return directoryList;
        }

        public static void AddOrUpdateDirectory(string appName, string directoryPath)
        {
            DirectoryList directoryList = GetDirectoryList();
            if (directoryList == null || string.IsNullOrEmpty(appName) || string.IsNullOrEmpty(directoryPath)) return;
            var filePath = GetExecutingDirectory() + "\\" + ConstResourceName;

            var appList = directoryList.DirectoryPath.Apps.ToList();
            if (appList.Any(x => x.AppName.Equals(appName, StringComparison.OrdinalIgnoreCase)))
            {
                var toBeUpdated = appList.FirstOrDefault(x => x.AppName.Equals(appName, StringComparison.OrdinalIgnoreCase));
                if (toBeUpdated != null)
                    toBeUpdated.Location = directoryPath;
            }
            else
            {
                Apps app = new Apps {AppName = appName, Location = directoryPath};
                appList.Add(app);
            }

            directoryList.DirectoryPath.Apps = appList.ToArray();
            WriteToStream(filePath, directoryList);
        }

        public static void RemoveDirectory(string appName)
        {
            DirectoryList directoryList = GetDirectoryList();
            var filePath = GetExecutingDirectory() + "\\" + ConstResourceName;

            var appList = directoryList.DirectoryPath.Apps.ToList();
            var toBeRemoved = appList.FirstOrDefault(x => x.AppName.Equals(appName, StringComparison.OrdinalIgnoreCase));
            if (toBeRemoved != null)
                appList.Remove(toBeRemoved);

            directoryList.DirectoryPath.Apps = appList.ToArray();
            WriteToStream(filePath, directoryList);
        }

        private static void WriteToStream(string filePath, DirectoryList directoryList)
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    string json = JsonConvert.SerializeObject(directoryList);
                    writer.Write(json);
                }
            }
        }

        public static DirectoryInfo GetExecutingDirectory()
        {
            var location = new Uri(Assembly.GetEntryAssembly().GetName().CodeBase);
            return new FileInfo(location.AbsolutePath).Directory;
        }
    }
}

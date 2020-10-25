using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using MT.DirectoryWatcher.Common;

namespace MT.DirectoryWatcher.Backend
{
    public class FileHashInfo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string FileId { get; set; }

        public string FileName { get; set; }

        public string ParentDirectory { get; set; }

        public string DirectoryPath { get; set; }

        [BsonIgnoreIfNull]
        public string OldHash { get; set; }

        [BsonIgnoreIfNull]
        public string NewHash { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [BsonRepresentation(BsonType.String)] 
        public ChangeType FileChange { get; set; }
    }
}

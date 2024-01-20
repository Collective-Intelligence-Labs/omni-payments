using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Cila.Models {
    public class TransferData
    {
        public ObjectId Id { get; set; }
        public string encodedData { get; set; }

        [BsonElement("__v")]
        public int Version { get; set; }
    }

    public class TransferDataRequest
    {
        public List<string> Cmds { get; set; }
    }

}
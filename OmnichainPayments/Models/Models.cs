using MongoDB.Bson;

namespace Cila.Models {
    public class TransferData
    {
        public ObjectId Id { get; set; }
        public string EncodedData { get; set; }
    }

    public class TransferDataRequest
    {
        public List<string> Cmds { get; set; }
    }

}
namespace Bitcoins
{
    public class Transaction
    {
        public string Id { get; set; }
        public long Sum { get; set; }

        public string SourceUserId { get; set; }

        public string RefTransactionId { get; set; }

        public string Signature { get; set; }

        public string Hash { get; set; }
    }
}

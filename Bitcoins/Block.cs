using System.Collections.Generic;

namespace Bitcoins
{
    public class Block
    {
        public string Id { get; set; }
        public byte[] Counter { get; set; }
        public bool IsConfirmed { get; set; }
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
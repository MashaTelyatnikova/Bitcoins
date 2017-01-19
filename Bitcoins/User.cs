using System.Collections.Generic;

namespace Bitcoins
{
    public class User
    {
        public string Id { get; set; }

        public long E { get; set; }

        public long D { get; set; }

        public long P { get; set; }

        public List<Block> Blocks { get; set; }
    }
}

using System;

namespace WiredPlayers.model
{
    public class BankOperationModel
    {
        public String source { get; set; }
        public String receiver { get; set; }
        public String type { get; set; }
        public int amount { get; set; }
        public String day { get; set; }
        public String time { get; set; }
    }
}

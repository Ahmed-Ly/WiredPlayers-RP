using System;

namespace WiredPlayers.model
{
    public class BankOperationModel
    {
        public String source { get; internal set; }
        public String receiver { get; internal set; }
        public String type { get; internal set; }
        public int amount { get; internal set; }
        public String day { get; internal set; }
        public String time { get; internal set; }

        public BankOperationModel() { }
    }
}

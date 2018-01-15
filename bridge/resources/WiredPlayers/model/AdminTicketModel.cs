using System;

namespace WiredPlayers.model
{
    public class AdminTicketModel
    {
        public int playerId { get; internal set; }
        public String question { get; internal set; }

        public AdminTicketModel() { }
    }
}

using System;

namespace WiredPlayers.model
{
    public class ChannelModel
    {
        public int id { get; internal set; }
        public int owner { get; internal set; }
        public String password { get; internal set; }

        public ChannelModel() { }
    }
}

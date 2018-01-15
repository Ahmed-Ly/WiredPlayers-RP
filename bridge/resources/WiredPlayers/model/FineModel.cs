using System;

namespace WiredPlayers.model
{
    public class FineModel
    {
        public String officer { get; internal set; }
        public String target { get; internal set; }
        public int amount { get; internal set; }
        public String reason { get; internal set; }
        public String date { get; internal set; }

        public FineModel() { }
    }
}

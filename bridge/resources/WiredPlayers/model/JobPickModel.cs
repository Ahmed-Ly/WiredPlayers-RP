using GTANetworkAPI;
using System;

namespace WiredPlayers.model
{
    public class JobPickModel
    {
        public int job { get; internal set; }
        public Vector3 position { get; internal set; }
        public String description { get; internal set; }

        public JobPickModel() { }

        public JobPickModel(int job, Vector3 position, String description)
        {
            this.job = job;
            this.position = position;
            this.description = description;
        }
    }
}

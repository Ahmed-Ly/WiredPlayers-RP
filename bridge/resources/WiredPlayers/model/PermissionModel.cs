using System;

namespace WiredPlayers.model
{
    public class PermissionModel
    {
        public int playerId { get; internal set; }
        public String command { get; internal set; }
        public String option { get; internal set; }

        public PermissionModel() { }
    }
}

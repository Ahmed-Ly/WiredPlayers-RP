using GTANetworkAPI;
using System;

namespace WiredPlayers.model
{
    public class InteriorModel
    {
        public String captionMessage { get; set; }
        public Vector3 entrancePosition { get; set; }
        public Vector3 exitPosition { get; set; }
        public String iplName { get; set; }
        public int blipId { get; set; }
        public Blip blip { get; set; }
        public TextLabel textLabel { get; set; }
        public String blipName { get; set; }

        public InteriorModel(String captionMessage, Vector3 entrancePosition, Vector3 exitPosition, String iplName, int blipId, String blipName)
        {
            this.captionMessage = captionMessage;
            this.entrancePosition = entrancePosition;
            this.exitPosition = exitPosition;
            this.iplName = iplName;
            this.blipId = blipId;
            this.blipName = blipName;
        }
    }
}

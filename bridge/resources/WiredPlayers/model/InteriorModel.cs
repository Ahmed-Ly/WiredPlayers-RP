using GTANetworkAPI;
using System;

namespace WiredPlayers.model
{
    public class InteriorModel
    {
        public String captionMessage { get; internal set; }
        public Vector3 entrancePosition { get; internal set; }
        public Vector3 exitPosition { get; internal set; }
        public String iplName { get; internal set; }
        public int blipId { get; internal set; }
        public Blip blip { get; internal set; }
        public TextLabel textLabel { get; internal set; }
        public String blipName { get; internal set; }

        public InteriorModel() { }

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

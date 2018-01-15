using System;

namespace WiredPlayers.model
{
    public class EmergencyWarnModel
    {
        public String patient { get; internal set; }
        public String paramedic { get; internal set; }
        public String time { get; internal set; }

        public EmergencyWarnModel() { }

        public EmergencyWarnModel(String patient, String paramedic, String time)
        {
            this.patient = patient;
            this.paramedic = paramedic;
            this.time = time;
        }
    }
}

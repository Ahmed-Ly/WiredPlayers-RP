using System;

namespace WiredPlayers.model
{
    public class EmergencyWarnModel
    {
        public String patient { get; set; }
        public String paramedic { get; set; }
        public String time { get; set; }

        public EmergencyWarnModel(String patient, String paramedic, String time)
        {
            this.patient = patient;
            this.paramedic = paramedic;
            this.time = time;
        }
    }
}

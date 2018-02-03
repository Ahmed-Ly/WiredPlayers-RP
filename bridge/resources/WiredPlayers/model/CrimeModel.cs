using System;

namespace WiredPlayers.model
{
    public class CrimeModel
    {
        public String crime { get; set; }
        public int jail { get; set; }
        public int fine { get; set; }
        public String reminder { get; set; }

        public CrimeModel(String crime, int jail, int fine, String reminder)
        {
            this.crime = crime;
            this.jail = jail;
            this.fine = fine;
            this.reminder = reminder;
        }
    }
}
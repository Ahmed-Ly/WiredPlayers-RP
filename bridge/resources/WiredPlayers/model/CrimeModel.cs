using System;

namespace WiredPlayers.model
{
    public class CrimeModel
    {
        public String crime { get; internal set; }
        public int jail { get; internal set; }
        public int fine { get; internal set; }
        public String reminder { get; internal set; }

        public CrimeModel() { }

        public CrimeModel(String crime, int jail, int fine, String reminder)
        {
            this.crime = crime;
            this.jail = jail;
            this.fine = fine;
            this.reminder = reminder;
        }
    }
}
using System;

namespace WiredPlayers.model
{
    public class FactionModel
    {
        public String descriptionMale { get; internal set; }
        public String descriptionFemale { get; internal set; }
        public int faction { get; internal set; }
        public int rank { get; internal set; }
        public int salary { get; internal set; }

        public FactionModel() { }

        public FactionModel(String descriptionMale, String descriptionFemale, int faction, int rank, int salary)
        {
            this.descriptionMale = descriptionMale;
            this.descriptionFemale = descriptionFemale;
            this.faction = faction;
            this.rank = rank;
            this.salary = salary;
        }
    }
}

using System;

namespace WiredPlayers.model
{
    public class FactionModel
    {
        public String descriptionMale { get; set; }
        public String descriptionFemale { get; set; }
        public int faction { get; set; }
        public int rank { get; set; }
        public int salary { get; set; }

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

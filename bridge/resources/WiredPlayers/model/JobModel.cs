using System;

namespace WiredPlayers.model
{
    public class JobModel
    {
        public String descriptionMale { get; set; }
        public String descriptionFemale { get; set; }
        public int job { get; set; }
        public int salary { get; set; }

        public JobModel(String descriptionMale, String descriptionFemale, int job, int salary)
        {
            this.descriptionMale = descriptionMale;
            this.descriptionFemale = descriptionFemale;
            this.job = job;
            this.salary = salary;
        }
    }
}

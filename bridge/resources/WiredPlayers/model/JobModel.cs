using System;

namespace WiredPlayers.model
{
    public class JobModel
    {
        public String descriptionMale { get; internal set; }
        public String descriptionFemale { get; internal set; }
        public int job { get; internal set; }
        public int salary { get; internal set; }

        public JobModel() { }

        public JobModel(String descriptionMale, String descriptionFemale, int job, int salary)
        {
            this.descriptionMale = descriptionMale;
            this.descriptionFemale = descriptionFemale;
            this.job = job;
            this.salary = salary;
        }
    }
}

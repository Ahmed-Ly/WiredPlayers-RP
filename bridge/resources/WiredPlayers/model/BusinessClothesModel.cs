using System;

namespace WiredPlayers.model
{
    public class BusinessClothesModel
    {
        public int type { get; set; }
        public String description { get; set; }
        public int bodyPart { get; set; }
        public int clothesId { get; set; }
        public int sex { get; set; }
        public int products { get; set; }

        public BusinessClothesModel(int type, String description, int bodyPart, int clothesId, int sex, int products)
        {
            this.type = type;
            this.description = description;
            this.bodyPart = bodyPart;
            this.clothesId = clothesId;
            this.sex = sex;
            this.products = products;
        }
    }
}

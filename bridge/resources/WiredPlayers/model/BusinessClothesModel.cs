using System;

namespace WiredPlayers.model
{
    public class BusinessClothesModel
    {
        public int type { get; internal set; }
        public String description { get; internal set; }
        public int bodyPart { get; internal set; }
        public int clothesId { get; internal set; }
        public int sex { get; internal set; }
        public int products { get; internal set; }

        public BusinessClothesModel() { }

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

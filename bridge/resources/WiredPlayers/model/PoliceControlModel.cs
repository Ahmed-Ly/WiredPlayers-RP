using GTANetworkAPI;
using System;

namespace WiredPlayers.model
{
    public class PoliceControlModel
    {
        public int id { get; internal set; }
        public String name { get; internal set; }
        public int item { get; internal set; }
        public Vector3 position { get; internal set; }
        public Vector3 rotation { get; internal set; }
        public GTANetworkAPI.Object controlObject { get; internal set; }

        public PoliceControlModel() { }

        public PoliceControlModel(int id, String name, int item, Vector3 position, Vector3 rotation)
        {
            this.id = id;
            this.name = name;
            this.item = item;
            this.position = position;
            this.rotation = rotation;
        }

        public PoliceControlModel Copy()
        {
            PoliceControlModel policeControlModel = new PoliceControlModel();
            policeControlModel.id = id;
            policeControlModel.name = name;
            policeControlModel.item = item;
            policeControlModel.position = position;
            policeControlModel.rotation = rotation;
            return policeControlModel;
        }
    }
}

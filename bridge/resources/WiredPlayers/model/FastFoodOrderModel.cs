using GTANetworkAPI;

namespace WiredPlayers.model
{
    public class FastFoodOrderModel
    {
        public int id { get; internal set; }
        public int pizzas { get; internal set; }
        public int hamburgers { get; internal set; }
        public int sandwitches { get; internal set; }
        public Vector3 position { get; internal set; }
        public float distance { get; internal set; }
        public double limit { get; internal set; }
        public bool taken { get; internal set; }

        public FastFoodOrderModel() { }
    }
}

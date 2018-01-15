using System;

namespace WiredPlayers.model
{
    public class ContactModel
    {
        public int id { get; internal set; }
        public int owner { get; internal set; }
        public int contactNumber { get; internal set; }
        public String contactName { get; internal set; }

        public ContactModel() { }
    }
}

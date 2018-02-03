using System;

namespace WiredPlayers.model
{
    public class AccountModel
    {
        public String socialName { get; set; }
        public String forumName { get; set; }
        public int status { get; set; }
        public int lastCharacter { get; set; }

        public AccountModel() { }

        public AccountModel(String socialName, String forumName, int status, int lastCharacter)
        {
            this.socialName = socialName;
            this.forumName = forumName;
            this.status = status;
            this.lastCharacter = lastCharacter;
        }
    }
}

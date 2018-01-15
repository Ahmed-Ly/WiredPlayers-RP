using System;

namespace WiredPlayers.model
{
    public class AccountModel
    {
        public String socialName { get; internal set; }
        public String forumName { get; internal set; }
        public int status { get; internal set; }
        public int lastCharacter { get; internal set; }

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

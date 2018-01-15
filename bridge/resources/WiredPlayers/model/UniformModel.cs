namespace WiredPlayers.model
{
    public class UniformModel
    {
        public int type { get; internal set; }
        public int factionJob { get; internal set; }
        public int characterSex { get; internal set; }
        public int uniformSlot { get; internal set; }
        public int uniformDrawable { get; internal set; }
        public int uniformTexture { get; internal set; }

        public UniformModel() { }

        public UniformModel(int type, int factionJob, int characterSex, int uniformSlot, int uniformDrawable, int uniformTexture)
        {
            this.type = type;
            this.factionJob = factionJob;
            this.characterSex = characterSex;
            this.uniformSlot = uniformSlot;
            this.uniformDrawable = uniformDrawable;
            this.uniformTexture = uniformTexture;
        }
    }
}

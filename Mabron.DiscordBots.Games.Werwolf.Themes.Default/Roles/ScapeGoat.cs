namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Roles
{
    public class ScapeGoat : VillagerBase
    {
        public bool WasKilledByVillage { get; set; } = false;

        public bool HasRevenge { get; set; } = false;

        public bool HasDecided { get; set; } = false;

        public ScapeGoat(Theme theme) : base(theme)
        {
        }

        public override string Name => "Sündenbock";

        public override Role CreateNew()
        {
            return new ScapeGoat(Theme);
        }
    }
}

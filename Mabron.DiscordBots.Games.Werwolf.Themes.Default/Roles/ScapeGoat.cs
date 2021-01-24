namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Roles
{
    public class ScapeGoat : VillagerBase
    {
        public bool WasKilledByVillage { get; set; } = false;

        public bool HasRevenge { get; set; } = false;

        public bool HasDecided { get; set; } = false;

        public override void Reset()
        {
            base.Reset();
            WasKilledByVillage = false;
            HasRevenge = false;
            HasDecided = false;
        }

        public ScapeGoat(Theme theme) : base(theme)
        {
        }

        public override string Name => "Sündenbock";

        public override string Description => "Wird dieser durch die Dorfwahl getötet, darf er entscheiden, wer die darauf folgende Runde wählen darf.";

        public override Role CreateNew()
        {
            return new ScapeGoat(Theme);
        }
    }
}

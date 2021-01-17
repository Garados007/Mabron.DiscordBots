namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Roles
{
    public class Girl : VillagerBase
    {
        public Girl(Theme theme) : base(theme)
        {
        }

        public override string Name => "Mädchen";

        public override string Description => "Darf in der Nacht \"blinzeln\" um zu erkennen wer die Werwölfe sind.";

        public override Role CreateNew()
        {
            return new Girl(Theme);
        }
    }
}

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Roles
{
    public class Healer : VillagerBase
    {
        public Healer(Theme theme) : base(theme)
        {
        }

        public override string Name => "Heiler";

        public override string Description => "Der Heiler kann eine Person vor den Werwölfen schützen";

        public override Role CreateNew()
        {
            return new Healer(Theme);
        }
    }
}

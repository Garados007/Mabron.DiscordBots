namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Roles
{
    public class Werwolf : WerwolfBase
    {
        public Werwolf(Theme theme) : base(theme)
        {
        }

        public override string Name => "Werwolf";

        public override string Description => "Ein Werwolf der in der Nacht die Dorfbewohner umbringt.";

        public override Role CreateNew()
        {
            return new Werwolf(Theme);
        }
    }
}

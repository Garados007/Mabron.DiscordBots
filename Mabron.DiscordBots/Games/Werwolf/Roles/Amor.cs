namespace Mabron.DiscordBots.Games.Werwolf.Roles
{
    public class Amor : VillagerBase
    {
        public override string Name => "Amor";

        public override string Description => "Amor verbindet zwei Liebenden fürs Leben";

        public override Role CreateNew()
        {
            return new Amor();
        }
    }
}

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Stages
{
    public class NightStage : Stage
    {
        public override string LanguageId => "night";

        public override string BackgroundId =>
            $"/content/games/werwolf/img/{typeof(DefaultTheme).FullName}/background-night.png";

        public override string ColorTheme => "#000911";
    }
}

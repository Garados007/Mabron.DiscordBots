namespace Mabron.DiscordBots.Games.Werwolf.Phases
{
    public abstract class ActionPhaseBase : Phase
    {
        public override bool CanExecute(GameRoom game)
            => true;

        public override bool IsGamePhase => false;

        public abstract void Execute(GameRoom game);

        public sealed override void Init(GameRoom game)
        {
            base.Init(game);
            Execute(game);
        }

        public override bool CanMessage(GameRoom game, Role role)
        {
            return false;
        }
    }
}

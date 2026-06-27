namespace MyProject.View
{
    public class GameActionsObserver : ActionsObserverBase
    {
        GameActions.MainActions MainActions => gameActions.Main;
        readonly GameActions gameActions = new();

        public override void Enable()
        {
            MainActions.Enable();
        }

        public override void Disable()
        {
            MainActions.Disable();
        }

        public override void Dispose()
        {
            gameActions.Dispose();
        }
    }
}

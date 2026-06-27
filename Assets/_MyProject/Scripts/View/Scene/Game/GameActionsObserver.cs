using R3;

namespace MyProject.View
{
    public class GameActionsObserver : ActionsObserverBase
    {
        public Observable<Unit> Quit;

        GameActions.MainActions MainActions => gameActions.Main;
        readonly GameActions gameActions = new();

        public GameActionsObserver()
        {
            Quit = ObservePerformed(MainActions.Quit).Select(_ => Unit.Default);
        }

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

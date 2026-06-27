using R3;

namespace MyProject.View
{
    public class TitleActionsObserver : ActionsObserverBase
    {
        public Observable<Unit> StartGame;

        TitleActions.MainActions MainActions => titleActions.Main;
        readonly TitleActions titleActions = new();

        public TitleActionsObserver()
        {
            StartGame = ObservePerformed(MainActions.StartGame).Select(_ => Unit.Default);
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
            titleActions.Dispose();
        }
    }
}

using R3;

namespace MyProject.View
{
    public class ResultActionsObserver : ActionsObserverBase
    {
        public Observable<Unit> Retry;
        public Observable<Unit> Quit;

        ResultActions.MainActions MainActions => resultActions.Main;
        readonly ResultActions resultActions = new();

        public ResultActionsObserver()
        {
            Retry = ObservePerformed(MainActions.Retry).Select(_ => Unit.Default);
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
            resultActions.Dispose();
        }
    }
}

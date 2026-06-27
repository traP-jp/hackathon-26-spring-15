namespace MyProject.View
{
    public class ResultActionsObserver : ActionsObserverBase
    {
        ResultActions.MainActions MainActions => resultActions.Main;
        readonly ResultActions resultActions = new();

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

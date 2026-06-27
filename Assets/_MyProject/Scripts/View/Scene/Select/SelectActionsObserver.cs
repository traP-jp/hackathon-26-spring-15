namespace MyProject.View
{
    public class SelectActionsObserver : ActionsObserverBase
    {
        SelectActions.MainActions MainActions => selectActions.Main;
        readonly SelectActions selectActions = new();

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
            selectActions.Dispose();
        }
    }
}

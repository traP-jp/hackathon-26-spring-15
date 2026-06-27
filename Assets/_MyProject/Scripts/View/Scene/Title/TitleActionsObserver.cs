namespace MyProject.View
{
    public class TitleActionsObserver : ActionsObserverBase
    {
        TitleActions.MainActions MainActions => titleActions.Main;
        readonly TitleActions titleActions = new();

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

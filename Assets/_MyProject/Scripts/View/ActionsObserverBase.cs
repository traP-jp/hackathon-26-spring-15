using System;
using R3;
using UnityEngine.InputSystem;

namespace MyProject.View
{
    public abstract class ActionsObserverBase : IDisposable
    {
        public abstract void Enable();
        public abstract void Disable();

        public abstract void Dispose();

        protected static Observable<InputAction.CallbackContext> ObserveStarted(InputAction action)
        {
            return Observable.FromEvent<InputAction.CallbackContext>(
                h => action.started += h,
                h => action.started -= h);
        }

        protected static Observable<InputAction.CallbackContext> ObservePerformed(InputAction action)
        {
            return Observable.FromEvent<InputAction.CallbackContext>(
                h => action.performed += h,
                h => action.performed -= h);
        }

        protected static Observable<InputAction.CallbackContext> ObserveCanceled(InputAction action)
        {
            return Observable.FromEvent<InputAction.CallbackContext>(
                h => action.canceled += h,
                h => action.canceled -= h);
        }
    }
}

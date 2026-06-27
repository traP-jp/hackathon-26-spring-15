using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MyProject.View
{
    [RequireComponent(typeof(Collider2D))]
    [DisallowMultipleComponent]
    public class PointerClickObserver : MonoBehaviour, IPointerClickHandler
    {
        public Observable<PointerEventData> Clicked => clicked;
        readonly Subject<PointerEventData> clicked = new();

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            clicked.OnNext(eventData);
        }

        void OnDestroy()
        {
            clicked.OnCompleted();
            clicked.Dispose();
        }
    }
}

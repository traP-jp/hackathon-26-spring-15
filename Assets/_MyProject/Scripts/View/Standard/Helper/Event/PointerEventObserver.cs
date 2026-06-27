using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MyProject.View
{
    /// <summary>
    /// マウスポインターのイベントを観測し、各種イベントを発行するクラス。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [DisallowMultipleComponent]
    public class PointerEventObserver : MonoBehaviour,
        IPointerClickHandler,
        IScrollHandler,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        IInitializePotentialDragHandler,
        IPointerMoveHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IDropHandler
    {
        /// <summary>
        /// ポインターがクリックされたときに発行されます。
        /// </summary>
        public Observable<PointerEventData> Clicked => clicked;
        readonly Subject<PointerEventData> clicked = new();

        /// <summary>
        /// マウスホイールでスクロールされたときに発行されます。
        /// </summary>
        public Observable<PointerEventData> Scrolled => scrolled;
        readonly Subject<PointerEventData> scrolled = new();

        /// <summary>
        /// ポインターがこのオブジェクトの領域に入ったときに発行されます。
        /// </summary>
        public Observable<PointerEventData> PointerEntered => pointerEntered;
        readonly Subject<PointerEventData> pointerEntered = new();

        /// <summary>
        /// ポインターがこのオブジェクトの領域から出たときに発行されます。
        /// </summary>
        public Observable<PointerEventData> PointerExited => pointerExited;
        readonly Subject<PointerEventData> pointerExited = new();

        /// <summary>
        /// ポインター押下が開始されたときに発行されます。
        /// </summary>
        public Observable<PointerEventData> PointerDown => pointerDown;
        readonly Subject<PointerEventData> pointerDown = new();

        /// <summary>
        /// ポインター押下が解除されたときに発行されます。
        /// </summary>
        public Observable<PointerEventData> PointerUp => pointerUp;
        readonly Subject<PointerEventData> pointerUp = new();

        /// <summary>
        /// ドラッグ初期化時に発行されます。
        /// </summary>
        public Observable<PointerEventData> InitializePotentialDrag => initializePotentialDrag;
        readonly Subject<PointerEventData> initializePotentialDrag = new();

        /// <summary>
        /// ポインター移動時に発行されます。
        /// </summary>
        public Observable<PointerEventData> PointerMoved => pointerMoved;
        readonly Subject<PointerEventData> pointerMoved = new();

        /// <summary>
        /// ドラッグ開始時に発行されます。
        /// </summary>
        public Observable<PointerEventData> BeginDrag => beginDrag;
        readonly Subject<PointerEventData> beginDrag = new();

        /// <summary>
        /// ドラッグ中に継続して発行されます。
        /// </summary>
        public Observable<PointerEventData> Dragged => dragged;
        readonly Subject<PointerEventData> dragged = new();

        /// <summary>
        /// ドラッグ終了時に発行されます。
        /// </summary>
        public Observable<PointerEventData> EndDrag => endDrag;
        readonly Subject<PointerEventData> endDrag = new();

        /// <summary>
        /// ドロップされたときに発行されます。
        /// </summary>
        public Observable<PointerEventData> Dropped => dropped;
        readonly Subject<PointerEventData> dropped = new();

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
            => Publish(clicked, eventData);

        void IScrollHandler.OnScroll(PointerEventData eventData)
            => Publish(scrolled, eventData);

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
            => Publish(pointerEntered, eventData);

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
            => Publish(pointerExited, eventData);

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
            => Publish(pointerDown, eventData);

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
            => Publish(pointerUp, eventData);

        void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData eventData)
            => Publish(initializePotentialDrag, eventData);

        void IPointerMoveHandler.OnPointerMove(PointerEventData eventData)
            => Publish(pointerMoved, eventData);

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
            => Publish(beginDrag, eventData);

        void IDragHandler.OnDrag(PointerEventData eventData)
            => Publish(dragged, eventData);

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
            => Publish(endDrag, eventData);

        void IDropHandler.OnDrop(PointerEventData eventData)
            => Publish(dropped, eventData);

        void OnDestroy()
        {
            CompleteAndDispose
            (
                clicked,
                scrolled,
                pointerEntered,
                pointerExited,
                pointerDown,
                pointerUp,
                initializePotentialDrag,
                pointerMoved,
                beginDrag,
                dragged,
                endDrag,
                dropped
            );
        }

        static void Publish(Subject<PointerEventData> subject, PointerEventData eventData)
        {
            subject.OnNext(eventData);
        }

        static void CompleteAndDispose(params Subject<PointerEventData>[] subjects)
        {
            foreach (var subject in subjects)
            {
                subject.OnCompleted();
                subject.Dispose();
            }
        }
    }
}

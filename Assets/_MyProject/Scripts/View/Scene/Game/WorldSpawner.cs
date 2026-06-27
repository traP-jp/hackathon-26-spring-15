using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MyProject.View
{
    public class WorldSpawner : ViewBase
    {
        [SerializeField]
        GameObject nearBackground;
        [SerializeField]
        Transform nearBackgroundParent;
        [SerializeField]
        float nearBackgroundWidth = 19.2f;
        [SerializeField]
        GameObject middleBackground;
        [SerializeField]
        Transform middleBackgroundParent;
        [SerializeField]
        float middleBackgroundWidth = 19.2f;
        [SerializeField]
        GameObject farBackground;
        [SerializeField]
        Transform farBackgroundParent;
        [SerializeField]
        float farBackgroundWidth = 19.2f;
        [SerializeField]
        Transform player;
        [SerializeField]
        Transform floorCollider;
        [SerializeField]
        List<GameObject> sampleGameObjects = new();

        ParallaxBackground nearParallaxBackground;
        ParallaxBackground middleParallaxBackground;
        ParallaxBackground farParallaxBackground;

        class ParallaxBackground
        {
            class BackgroundInstance
            {
                public GameObject GameObject { get; }
                public int Index { get; set; }

                public BackgroundInstance(GameObject gameObject, int index)
                {
                    GameObject = gameObject;
                    Index = index;
                }
            }

            readonly Camera camera;
            readonly List<BackgroundInstance> backgrounds = new List<BackgroundInstance>();
            readonly float width;
            readonly uint margin;
            readonly float factor;
            readonly float offsetY;
            readonly Transform parent;
            readonly Action<GameObject> onPlaced;

            /// <summary>
            /// 背景のインスタンスを生成
            /// </summary>
            /// <param name="camera">背景が追従するカメラ</param>
            /// <param name="background">背景のPrefab</param>
            /// <param name="parent">生成した背景を配置する親</param>
            /// <param name="width">背景のループ幅</param>
            /// <param name="margin">左右にいくつ背景を複製するか</param>
            /// <param name="factor">視差の強さ。0に近づくほどカメラに追従して変化し、1に近づくほどスクロールが遅くなる</param>
            /// <param name="offsetY">Y方向の背景のオフセット</param>
            /// <param name="onPlaced">背景を初期配置または再配置したときの処理</param>
            public ParallaxBackground(Camera camera, GameObject background, Transform parent, float width, uint margin, float factor, float offsetY, Action<GameObject> onPlaced = null)
            {
                if (width <= 0f)
                {
                    throw new InvalidOperationException("background width must be greater than 0.");
                }

                this.width = width;
                this.camera = camera;
                this.margin = margin;
                this.factor = factor;
                this.offsetY = offsetY;
                this.parent = parent;
                this.onPlaced = onPlaced;

                Vector3 cameraPosition = GetCameraPosition();
                int cameraIndex = GetIndex(cameraPosition.x * (1f - factor));

                for (int i = 0; i < 1 + margin * 2; i++)
                {
                    int index = cameraIndex - (int)margin + i;
                    GameObject gameObject = Instantiate(background, parent);
                    var instance = new BackgroundInstance(gameObject, index);
                    SetPosition(instance, cameraPosition);
                    onPlaced?.Invoke(gameObject);
                    backgrounds.Add(instance);
                }
            }

            public void Update()
            {
                Vector3 cameraPosition = GetCameraPosition();
                int cameraIndex = GetIndex(cameraPosition.x * (1f - factor));
                int firstIndex = cameraIndex - (int)margin;
                int lastIndex = cameraIndex + (int)margin;

                for (int i = 0; i < backgrounds.Count; i++)
                {
                    bool placed = false;
                    while (backgrounds[i].Index < firstIndex)
                    {
                        backgrounds[i].Index += backgrounds.Count;
                        placed = true;
                    }

                    while (backgrounds[i].Index > lastIndex)
                    {
                        backgrounds[i].Index -= backgrounds.Count;
                        placed = true;
                    }

                    SetPosition(backgrounds[i], cameraPosition);
                    if (placed)
                    {
                        onPlaced?.Invoke(backgrounds[i].GameObject);
                    }
                }
            }

            Vector3 GetCameraPosition()
            {
                return parent == null
                    ? camera.transform.position
                    : parent.InverseTransformPoint(camera.transform.position);
            }

            int GetIndex(float x)
            {
                return (int)MathF.Round(x / width);
            }

            float IndexToPosition(int index)
            {
                return index * width;
            }

            void SetPosition(BackgroundInstance background, Vector3 cameraPosition)
            {
                background.GameObject.transform.localPosition = new Vector3(
                    IndexToPosition(background.Index),
                    offsetY,
                    0f
                ) + cameraPosition * factor;
            }
        }

        public override void Initialize()
        {
            Camera camera = Camera.main;
            nearParallaxBackground = new ParallaxBackground(
                camera,
                nearBackground,
                nearBackgroundParent,
                nearBackgroundWidth,
                1,
                0f,
                0f,
                RandomizeNearBackground
            );
            middleParallaxBackground = new ParallaxBackground(
                camera,
                middleBackground,
                middleBackgroundParent,
                middleBackgroundWidth,
                1,
                0.5f,
                0f
            );
            farParallaxBackground = new ParallaxBackground(
                camera,
                farBackground,
                farBackgroundParent,
                farBackgroundWidth,
                1,
                0.8f,
                0f
            );

            HideSampleGameObjects();
            gameObject.SetActive(false);
        }

        public override void Show()
        {
            gameObject.SetActive(true);
        }

        public override void Hide()
        {
            gameObject.SetActive(false);
        }

        public override UniTask ShowAsync(CancellationToken ct)
        {
            Show();
            return UniTask.CompletedTask;
        }

        public override UniTask HideAsync(CancellationToken ct)
        {
            Hide();
            return UniTask.CompletedTask;
        }

        void Update()
        {
            nearParallaxBackground.Update();
            middleParallaxBackground.Update();
            farParallaxBackground.Update();
        }

        void LateUpdate()
        {
            var position = floorCollider.position;
            position.x = player.position.x;
            floorCollider.position = position;
        }

        void HideSampleGameObjects()
        {
            foreach (GameObject sampleGameObject in sampleGameObjects)
            {
                sampleGameObject.SetActive(false);
            }
        }

        static void RandomizeNearBackground(GameObject background)
        {
            background.GetComponent<NearBackgroundView>().RandomizeGridVisibility();
        }
    }
}

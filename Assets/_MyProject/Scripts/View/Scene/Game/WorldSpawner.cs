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
            readonly Camera camera;
            readonly List<GameObject> backgrounds = new List<GameObject>();
            readonly float width;
            readonly uint margin;
            readonly float factor;
            readonly float offsetY;
            readonly Transform parent;

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
            public ParallaxBackground(Camera camera, GameObject background, Transform parent, float width, uint margin, float factor, float offsetY)
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

                Vector3 cameraPosition = GetCameraPosition();
                int cameraIndex = GetIndex(cameraPosition.x * (1f - factor));

                for (int i = 0; i < 1 + margin * 2; i++)
                {
                    GameObject gameObject = Instantiate(background, parent);
                    gameObject.transform.localPosition = new Vector3(
                        IndexToPosition(cameraIndex - (int)margin + i),
                        offsetY,
                        0
                    );
                    backgrounds.Add(gameObject);
                }
            }

            public void Update()
            {
                Vector3 cameraPosition = GetCameraPosition();
                int cameraIndex = GetIndex(cameraPosition.x * (1f - factor));

                for (int i = 0; i < backgrounds.Count; i++)
                {
                    backgrounds[i].transform.localPosition = new Vector3(
                      IndexToPosition(cameraIndex - (int)margin + i),
                      offsetY,
                      0f
                    ) + cameraPosition * factor;
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
                0f
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
    }
}

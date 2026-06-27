using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldSpawner : MonoBehaviour
{
    [SerializeField]
    Transform playerTransform;

    [SerializeField]
    GameObject floor;

    //左右に生成する床の数
    uint floorMargin = 2;
    List<GameObject> floors;
    float floorWidth;

    [SerializeField]
    GameObject nearBackground;
    [SerializeField]
    GameObject farBackground;

    ParallaxBackground nearParallaxBackground;
    ParallaxBackground farParallaxBackground;

    class ParallaxBackground
    {
        readonly Camera camera;
        readonly List<GameObject> backgrounds = new List<GameObject>();
        readonly float width;
        readonly uint margin;
        readonly float factor;
        readonly float offsetY;

        /// <summary>
        /// 背景のインスタンスを生成
        /// </summary>
        /// <param name="camera">背景が追従するカメラ</param>
        /// <param name="background">背景のPrefab</param>
        /// <param name="margin">左右にいくつ背景を複製するか</param>
        /// <param name="factor">視差の強さ。0に近づくほどカメラに追従して変化し、1に近づくほどスクロールが遅くなる</param>
        /// <param name="offsetY">Y方向の背景のオフセット</param>
        public ParallaxBackground(Camera camera, GameObject background, uint margin, float factor, float offsetY)
        {
            SpriteRenderer spriteRenderer = background.GetComponent<SpriteRenderer>();

            if (spriteRenderer == null)
            {
                Debug.LogError("background does not have SpriteRenderer.");
            }
            else
            {
                width = spriteRenderer.sprite.rect.width / spriteRenderer.sprite.pixelsPerUnit * background.transform.localScale.x;
            }

            this.camera = camera;
            this.margin = margin;
            this.factor = factor;
            this.offsetY = offsetY;

            int cameraIndex = GetIndex(camera.transform.position.x * (1f - factor));

            for (int i = 0; i < 1 + margin * 2; i++)
            {
                GameObject gameObject = Instantiate(
                    background,
                    new Vector3(
                        IndexToPosition(cameraIndex - (int)margin + i),
                        offsetY,
                        0
                    ),
                    Quaternion.identity
                );
                backgrounds.Add(gameObject);
            }
        }

        public void Update()
        {
            int cameraIndex = GetIndex(camera.transform.position.x * (1f - factor));

            for (int i = 0; i < backgrounds.Count; i++)
            {
                backgrounds[i].transform.position = new Vector3(
                  IndexToPosition(cameraIndex - (int)margin + i),
                  offsetY,
                  0f
                ) + camera.transform.position * factor;
            }
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

    void Awake()
    {
        Camera camera = Camera.main;
        nearParallaxBackground = new ParallaxBackground(
            camera,
            nearBackground,
            1,
            0f,
            0f
        );
        farParallaxBackground = new ParallaxBackground(
            camera,
            farBackground,
            1,
            0.5f,
            0f
        );
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        floors = new List<GameObject>();
        for (int i = 0; i < 1 + floorMargin * 2; i++)
        {
            floors.Add(Instantiate(floor));
        }

        BoxCollider2D collider = floor.GetComponent<BoxCollider2D>();

        if(collider == null)
        {
            Debug.LogError("A BoxCollider is not attached.");
        }
        else
        {
            floorWidth = Vector2.Scale(collider.size, floor.transform.localScale).x;
            int playerIndex = GetIndex(playerTransform.position.x);
            for (int i = 0; i < floors.Count; i++)
            {
                Transform transform = floors[i].transform;
                transform.position = new Vector3(
                    IndexToPosition(playerIndex - (int)floorMargin + i),
                    transform.position.y,
                    transform.position.z
                );
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        GenerateFloor();
        nearParallaxBackground.Update();
        farParallaxBackground.Update();
    }

    void GenerateFloor()
    {
        int playerIndex = GetIndex(playerTransform.position.x);
        int currentCenter = GetIndex(floors[(int)floorMargin].transform.position.x);
        //プレイヤーが属するIndexが変化したら床の位置を更新する
        if(playerIndex < currentCenter)
        {
            int offset = currentCenter - playerIndex;
            int firstIndex = GetIndex(floors.First().transform.position.x);
            for(int i = 0; i < offset; i++)
            {
                GameObject last = floors.Last();
                floors.RemoveAt(floors.Count - 1);

                last.transform.position = new Vector3(
                    IndexToPosition(firstIndex - i - 1),
                    last.transform.position.y,
                    last.transform.position.z
                );

                floors.Insert(0, last);
            }
        }
        else if(currentCenter < playerIndex)
        {
            int offset = playerIndex - currentCenter;
            int lastIndex = GetIndex(floors.Last().transform.position.x);
            for(int i = 0; i < offset; i++)
            {
                GameObject first = floors.First();
                floors.RemoveAt(0);

                first.transform.position = new Vector3(
                    IndexToPosition(lastIndex + i + 1),
                    first.transform.position.y,
                    first.transform.position.z
                );

                floors.Add(first);
            }
        }
    }

    int GetIndex(float x)
    {
        return (int)MathF.Round(x / floorWidth);
    }

    float IndexToPosition(int index)
    {
        return index * floorWidth;
    }
}

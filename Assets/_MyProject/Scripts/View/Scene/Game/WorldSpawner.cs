using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldSpawner : MonoBehaviour
{
    [SerializeField]
    Transform transform;

    [SerializeField]
    GameObject floor;

    //左右に生成する床の数
    uint floorMargin = 2;
    List<GameObject> floors;
    float floorWidth;
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
            int playerIndex = GetIndex(transform.position.x);
            for (int i = 0; i < floors.Count; i++)
            {
                Transform transform = floors[i].transform;
                transform.position = new Vector3(
                    IndexToPosition(playerIndex - (int)floorMargin + i),
                    transform.position.x,
                    transform.position.z
                );
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        GenerateFloor();
    }

    void GenerateFloor()
    {
        int playerIndex = GetIndex(transform.position.x);
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
        return (int)MathF.Floor(x / floorWidth);
    }

    float IndexToPosition(int index)
    {
        return index * floorWidth;
    }
}

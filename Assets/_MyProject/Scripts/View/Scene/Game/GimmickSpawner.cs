using System.Collections.Generic;
using UnityEngine;

public class GimmickSpawner : MonoBehaviour
{
    [SerializeField] private Transform player_transform;
    [SerializeField] private GameObject wall_prefab;
    private int wall_number_count = 0;
    [SerializeField] private int wall_distant = 5;
    private List<GameObject> walls;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        walls = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (player_transform.position.x >= wall_number_count * wall_distant)
        {
            Vector3 spawnPosition = new Vector3(player_transform.position.x + 15, 0, 0);
            GameObject newwall = Instantiate(wall_prefab, spawnPosition, Quaternion.identity, transform);
            walls.Add(newwall);

            if (walls.Count >= 10)
            {
                Destroy(walls[0]);
                walls.RemoveAt(0);
            }

            wall_number_count += 1;
        }
    }
}


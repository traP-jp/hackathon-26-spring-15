using System;
using UnityEngine;

public class GimmickView : MonoBehaviour
{
    [SerializeField] private int gimmick_damage = 10;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerView>(out PlayerView playerview))
        {
            playerview.Damage(gimmick_damage);
        }
    }
}

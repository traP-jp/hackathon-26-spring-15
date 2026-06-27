using UnityEngine;

public class GimmickView : MonoBehaviour
{
    [SerializeField] private GimmickType _type;
    [SerializeField] private int _damage = 10;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.TryGetComponent<PlayerView>(out var player))
        {
            player.Damage(_damage, _type);
        }
    }
}

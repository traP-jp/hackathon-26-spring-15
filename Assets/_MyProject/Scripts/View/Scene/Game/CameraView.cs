using UnityEngine;
using Unity.Cinemachine;

public class CameraView : MonoBehaviour
{
    [SerializeField] private GameObject _player;
    private CinemachineImpulseSource impulseSource;

    void Start()
    {
        // 衝撃を発生させるためのソースを取得
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    void Update()
    {
        // カメラのX軸をプレイヤーに追従させる
        if (_player != null)
        {
            transform.position = new Vector3(_player.transform.position.x, transform.position.y, transform.position.z);
        }
    }

    // 被弾した時に外部からこのメソッドを呼ぶだけでOK
    public void ShakeCamera()
    {
        if (impulseSource != null)
        {
            impulseSource.GenerateImpulse();
        }
    }
}

using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(CinemachineImpulseSource))]
public class CameraFollowView : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] Transform followTarget;

    CinemachineImpulseSource impulseSource;

    void Awake()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    void LateUpdate()
    {
        var position = followTarget.position;
        position.x = player.position.x;
        followTarget.position = position;
    }

    public void ShakeCamera()
    {
        impulseSource.GenerateImpulse();
    }
}

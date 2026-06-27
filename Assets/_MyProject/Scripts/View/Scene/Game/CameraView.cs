using UnityEngine;
using Cinemachine;

public class CameraView : MonoBehaviour
{
    [SerializeField] GameObject _player;
    [SerializeField] private CinemachineCamera virtualCamera;
    private CinemachineBasicMultiChannelPerlin noise;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        noise = virtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(_player.transform.position.x, transform.position.y, transform.position.z);
    }

    public void Shake(float amplitude, float frequency, float duration)
    {
        if (noise == null) return;

        noise.AmplitudeGain = amplitude;
        noise.FrequencyGain = frequency;
        Invoke(nameof(StopShake), duration);
    }

     void StopShake()
    {
        noise.AmplitudeGain = 0f;
        noise.FrequencyGain = 0f;
    }
}

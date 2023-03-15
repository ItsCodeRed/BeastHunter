using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(Camera))]
public class CamFollow : MonoBehaviour
{
    public float horizontalBorder;
    public float verticalBorder;
    public float maxX = 50;
    public float minX = -50;
    public float maxY = 10;
    public float minY = -10;

    [SerializeField] private CinemachineVirtualCamera vCam;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0;
    }

    public Vector2 GetCameraSize()
    {
        return new Vector2(cam.orthographicSize * Screen.width / Screen.height, cam.orthographicSize);
    }

    void Update()
    {
        if (Player.instance == null) return;

        vCam.Follow = Player.instance.transform;

        Vector2 pos = transform.position;
        Vector2 offset = (Vector2)Player.instance.transform.position - pos;
        Vector2 camSize = GetCameraSize();

        if (Mathf.Abs(offset.x) > camSize.x - horizontalBorder)
        {
            pos.x += offset.x - Mathf.Sign(offset.x) * (camSize.x - horizontalBorder);
        }
        if (Mathf.Abs(offset.y) > camSize.y - verticalBorder)
        {
            pos.y += offset.y - Mathf.Sign(offset.y) * (camSize.y - verticalBorder);
        }
        pos = new Vector2(Mathf.Clamp(pos.x, minX, maxX), Mathf.Clamp(pos.y, minY, maxY));

        transform.position = (Vector3)pos - Vector3.forward * 3;
    }

    public void ShakeCamera(float power, float time)
    {
        StartCoroutine(ShakeCameraRoutine(power, time));
    }

    private IEnumerator ShakeCameraRoutine(float power, float time)
    {
        float timer = time;
        var noise = vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        while (timer > 0)
        {
            timer -= Time.deltaTime;

            noise.m_AmplitudeGain = power * (timer / time);

            yield return null;
        }

        noise.m_AmplitudeGain = 0;
    }
}

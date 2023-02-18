using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CamFollow : MonoBehaviour
{
    public float horizontalBorder;
    public float verticalBorder;
    public float maxX = 50;
    public float minX = -50;
    public float maxY = 10;
    public float minY = -10;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    public Vector2 GetCameraSize()
    {
        return new Vector2(cam.orthographicSize * Screen.width / Screen.height, cam.orthographicSize);
    }

    void Update()
    {
        if (Player.instance == null) return;

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
}

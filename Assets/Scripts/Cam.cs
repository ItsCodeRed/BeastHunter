using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class Cam : MonoBehaviour
{
    public Vector2 levelCenter;
    public Vector2 levelSize;

    [SerializeField] private float minZoom = 20;
    [SerializeField] private float maxZoom = 60;
    [SerializeField] private float zoomSpeed = 3;
    private Vector3 dragOrigin;

    private Camera camComponent;

    private void Awake()
    {
        camComponent = GetComponent<Camera>();
    }

    private void Update()
    {
        if (Input.GetMouseButton(2) || (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftShift)))
        {
            transform.Translate((Vector2)(dragOrigin - camComponent.ScreenToWorldPoint(Input.mousePosition)));
        }

        dragOrigin = camComponent.ScreenToWorldPoint(Input.mousePosition);

        camComponent.orthographicSize = Mathf.Clamp(camComponent.orthographicSize - Input.mouseScrollDelta.y * zoomSpeed, minZoom, maxZoom);
    }

    private void LateUpdate()
    {
        Vector3 restrictedCamPosition = transform.position;
        Vector2 levelCenterToCam = (Vector2)transform.position - levelCenter;

        float screenRatio = Screen.width / (float)Screen.height;

        if (Mathf.Abs(levelCenterToCam.x) + camComponent.orthographicSize * screenRatio > levelSize.x / 2)
        {
            restrictedCamPosition.x = Mathf.Clamp(levelCenterToCam.x, -levelSize.x / 2 + camComponent.orthographicSize * screenRatio, levelSize.x / 2 - camComponent.orthographicSize * screenRatio);
        }

        if (Mathf.Abs(levelCenterToCam.y) + camComponent.orthographicSize > levelSize.y / 2)
        {
            restrictedCamPosition.y = Mathf.Clamp(levelCenterToCam.y, -levelSize.y / 2 + camComponent.orthographicSize, levelSize.y / 2 - camComponent.orthographicSize);
        }

        transform.position = restrictedCamPosition;
    }
}

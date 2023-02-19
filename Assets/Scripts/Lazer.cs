using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lazer : MonoBehaviour
{
    public int damage;
    public Vector2 knockback;
    public float hitLength;
    public float stun;

    public Vector2 dir;
    public GameObject lazerSegmentPrefab;
    public float segmentLength;

    private List<GameObject> lazerSegments = new List<GameObject>();

    private void FixedUpdate()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir.normalized, 999);

        int lazerSegmentNum = Mathf.CeilToInt(hit.distance / segmentLength);
        if (lazerSegments.Count > lazerSegmentNum)
        {
            for (int i = lazerSegments.Count - 1; i >= lazerSegmentNum; i--)
            {
                Destroy(lazerSegments[i]);
                lazerSegments.RemoveAt(i);
            }
        }
        else if (lazerSegments.Count < lazerSegmentNum)
        {
            for (int i = lazerSegments.Count; i < lazerSegmentNum; i++)
            {
                lazerSegments.Add(Instantiate(lazerSegmentPrefab, transform));
            }
        }

        for (int i = 0; i < lazerSegmentNum; i++)
        {
            lazerSegments[i].transform.position = transform.position + (Vector3)dir.normalized * ((hit.distance - segmentLength / 2f) * (i + 1) / (float)lazerSegmentNum);
        }

        if (hit.collider.CompareTag("Player"))
        {
            int modifier = Mathf.CeilToInt(damage * GameManager.instance.attackMultiplier * (GameManager.instance.dayNum - 1));
            Player.instance.TakeDamage(damage + modifier, Vector2.Scale(knockback, new Vector2(Mathf.Sign(transform.lossyScale.x), 1)), hitLength, stun);
        }
    }
}

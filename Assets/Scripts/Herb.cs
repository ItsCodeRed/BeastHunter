using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Herb : MonoBehaviour
{
    public float collectDist;
    public GameObject hintText;

    void Update()
    {
        bool inRange = Vector2.Distance(Player.instance.transform.position, transform.position) < collectDist;
        hintText.SetActive(inRange);

        if (inRange && Input.GetKeyDown(KeyCode.F))
        {
            Player.instance.CollectHerb(gameObject);
        }
    }
}

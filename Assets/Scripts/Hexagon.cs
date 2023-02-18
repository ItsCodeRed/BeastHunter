using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Hexagon : MonoBehaviour
{
    public int id;
    private SpriteRenderer sprite;
    public bool isAdjecent = false;
    public UnityEvent onClick = new UnityEvent();

    public float darkenValue = 0.8f;

    private Color startColor;

    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        startColor = sprite.color;
    }

    private void OnMouseOver()
    {
        if (isAdjecent && !WorldMapManager.instance.confirmActionMenu.activeInHierarchy && GameManager.instance.playingGame 
            && (WorldMapManager.instance.map[id].type != ZoneType.Village || (GameManager.instance.meatAmount > 0 && GameManager.instance.villageFood < GameManager.instance.villageFoodMax)))
        {
            sprite.color = startColor * new Color(darkenValue, darkenValue, darkenValue);
            if (Input.GetMouseButtonDown(0))
            {
                WorldMapManager.instance.TravelTo(id);
                onClick.Invoke();
            }
        }
    }

    private void OnMouseExit()
    {
        sprite.color = startColor;
    }
}

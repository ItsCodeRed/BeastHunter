using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MonsterPiece : MonoBehaviour
{
    public TMP_Text nameText;
    public SpriteRenderer sprite;

    public float bigSize;
    public float smallSize;

    public void Initialize(MonsterInstance monster)
    {
        nameText.text = monster.monsterAsset.name + (monster.monsterAsset.isBig && monster.currentHealth < monster.monsterAsset.maxHealth + (GameManager.instance.dayNum - 1) * GameManager.instance.healthIncrement ? " (DAMAGED)" : "");
        sprite.sprite = monster.monsterAsset.monsterSprite;
        transform.localScale *= monster.monsterAsset.isBig ? bigSize : smallSize;
    }
}

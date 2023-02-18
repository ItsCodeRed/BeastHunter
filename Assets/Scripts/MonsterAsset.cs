using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Big Monster", menuName = "New Big Monster")]
public class MonsterAsset : ScriptableObject
{
    public string monsterName;
    public Sprite monsterSprite;
    public Enemy monsterPrefab;
    public int maxHealth;
    public int meat;
    public Vector2 monsterOffset;
    public List<MonsterAsset> additionalMonsters;

    public bool isBig = false;
}

public class MonsterInstance
{
    public MonsterAsset monsterAsset = null;
    public int currentHealth;

    public MonsterInstance(MonsterAsset asset)
    {
        monsterAsset = asset;
        currentHealth = asset.maxHealth + (GameManager.instance.dayNum - 1) * GameManager.instance.healthIncrement;
    }
}

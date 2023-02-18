using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MonsterManager : MonoBehaviour
{
    public static MonsterManager instance;

    public Transform bigMonsterSpawn;
    public List<Transform> monsterSpawns;

    public Enemy bossMonster;
    public List<Enemy> allEnemies;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        foreach (MonsterInstance monster in GameManager.instance.monsters)
        {
            Vector2 spawnPoint = monster.monsterAsset.isBig ? bigMonsterSpawn.transform.position : monsterSpawns[Random.Range(0, monsterSpawns.Count)].position;

            Enemy enemy = Instantiate(monster.monsterAsset.monsterPrefab, spawnPoint, Quaternion.identity);
            allEnemies.Add(enemy);
            enemy.monsterInstance = monster;
            enemy.health = monster.monsterAsset.maxHealth;

            if (monster.monsterAsset.isBig)
            {
                bossMonster = enemy;
                int maxHealth = monster.monsterAsset.maxHealth + (GameManager.instance.dayNum - 1) * GameManager.instance.healthIncrement;

                UIManager.instance.bossText.text = monster.monsterAsset.monsterName.ToUpper();
                UIManager.instance.bossHealthBar.maxValue = maxHealth;
                UIManager.instance.bossHealthBar.value = maxHealth;
                enemy.health = monster.currentHealth;

                if (monster.monsterAsset.additionalMonsters.Count > 0)
                {
                    foreach (MonsterAsset additionalMonster in monster.monsterAsset.additionalMonsters)
                    {
                        Enemy additionalEnemy = Instantiate(additionalMonster.monsterPrefab, monsterSpawns[Random.Range(0, monsterSpawns.Count)].position, Quaternion.identity);
                        allEnemies.Add(additionalEnemy);
                        bossMonster.additionalEnemies.Add(additionalEnemy);
                        additionalEnemy.monsterInstance = new MonsterInstance(additionalMonster);
                        additionalEnemy.health = monster.monsterAsset.maxHealth;
                    }
                }
            }
        }
    }

    private void Update()
    {
        if (UIManager.instance.bossHealthBar != null)
        {
            if (bossMonster.health > 0)
            {
                UIManager.instance.bossHealthBar.value = bossMonster.health;
            }
            else
            {
                UIManager.instance.bossHealthBar.gameObject.SetActive(false);
            }
        }
    }
}

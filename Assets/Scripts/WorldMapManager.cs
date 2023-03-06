using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using TMPro;
using System.Linq;

public class WorldMapManager : MonoBehaviour
{
    public static WorldMapManager instance = null;

    public string[] grassMovingScenes;
    public string[] rockMovingScenes;
    public string[] grassFightScenes;
    public string[] rockFightScenes;

    public MonsterAsset[] bigMonsters;
    public MonsterAsset[] smallMonsters;

    public int bigMonsterCap;
    public int smallMonsterTotalCap;

    public int grassSmallMonsterCap;
    public int rockSmallMonsterCap;

    public int villageFoodPerDay;

    [SerializeField] private TMP_Text dayText;

    public MonsterPiece monsterPiecePrefab;
    public GameObject playerPiece;
    public GameObject confirmActionMenu;
    public TMP_Text confirmText;
    public Button confirmButton;
    public GameObject zoneArrowPrefab;

    public Slider villageFoodBar;
    public TMP_Text villageFoodPercentage;

    public string confirmMovementText;
    public string confirmFightText;
    public string confirmFoodText;

    public List<Zone> map = new List<Zone>();
    public List<int> mapConnections = new List<int>();

    public List<Zone> mapStart = new List<Zone>();
    private List<GameObject> objs = new List<GameObject>();

    public List<Zone> FindMapConnections(int id)
    {
        List<Zone> zoneConnections = new List<Zone>();

        for (int i = 0; i < mapConnections.Count; i++)
        {
            int num = mapConnections[i];
            if (num == id)
            {
                if (i % 2 == 0)
                {
                    zoneConnections.Add(map[mapConnections[i + 1]]);
                }
                else
                {
                    zoneConnections.Add(map[mapConnections[i - 1]]);
                }
            }
        }

        return zoneConnections;
    }

    private void Awake()
    {
        instance = this;

        for (int i = 0; i < map.Count; i++)
        {
            map[i].gameObject.id = i;
            map[i].gameObject.onClick.AddListener(new UnityAction(GameManager.instance.buttonClickEvent.Invoke));
        }
    }

    public void CleanUp()
    {
        foreach(GameObject obj in objs)
        {
            Destroy(obj);
        }

        objs.Clear();

        map = mapStart;

        foreach (Zone zone in map)
        {
            zone.monster = null;
            zone.smallMonsters = new List<MonsterInstance>();
        }
    }

    public void StartGame()
    {
        Zone currentZone = map[GameManager.instance.zoneId];
        Vector3 currentZonePos = currentZone.gameObject.transform.position;

        playerPiece.transform.position = currentZone.gameObject.transform.position;

        villageFoodBar.maxValue = GameManager.instance.villageFoodMax;

        if (GameManager.instance.dayNum == 0)
        {
            villageFoodBar.value = GameManager.instance.villageFood;
            villageFoodPercentage.text = (GameManager.instance.villageFood * 100) / GameManager.instance.villageFoodMax + "%";
            GameManager.instance.dayNum++;

            for (int i = 0; i < smallMonsterTotalCap; i++)
            {
                SpawnMonsterOnMap(new MonsterInstance(smallMonsters[UnityEngine.Random.Range(0, smallMonsters.Length)]));
            }

            for (int i = 0; i < bigMonsterCap; i++)
            {
                SpawnMonsterOnMap(new MonsterInstance(bigMonsters[UnityEngine.Random.Range(0, bigMonsters.Length)]));
            }
        }
        else
        {
            List<Zone> previousMap = GameManager.instance.previousMap;

            GameManager.instance.ChangeVillageFood(-villageFoodPerDay);
            villageFoodBar.value = GameManager.instance.villageFood;
            villageFoodPercentage.text = (GameManager.instance.villageFood * 100) / GameManager.instance.villageFoodMax + "%";

            for (int i = 0; i < previousMap.Count(); i++)
            {
                previousMap[i].gameObject = map[i].gameObject;
            }

            map = previousMap;

            List<Zone> skipZones = new List<Zone>();

            bool bigMonsterDied = false;

            for (int i = 0; i < map.Count(); i++)
            {
                Zone zone = map[i];
                if (zone.monster != null && !skipZones.Contains(zone))
                {
                    if (zone.monster.currentHealth <= 0)
                    {
                        bigMonsterDied = true;
                        zone.monster = null;
                    }
                    else
                    {
                        List<Zone> zones = FindMapConnections(i).Where(x => x.type != ZoneType.Village && x.monster == null && x != currentZone && x.gameObject.id != GameManager.instance.zoneId).ToList();
                        if (zones.Count > 0)
                        {
                            Zone chosenZone = zones[UnityEngine.Random.Range(0, zones.Count)];
                            chosenZone.monster = zone.monster;

                            if (chosenZone.monster.currentHealth >= chosenZone.monster.monsterAsset.maxHealth + (GameManager.instance.dayNum - 2) * GameManager.instance.healthIncrement)
                            {
                                chosenZone.monster.currentHealth += GameManager.instance.healthIncrement;
                            }

                            skipZones.Add(chosenZone);
                            SpawnMonsterOnMap(chosenZone.monster, chosenZone);

                            zone.monster = null;
                        }
                        else
                        {
                            if (zone.monster.currentHealth >= zone.monster.monsterAsset.maxHealth + (GameManager.instance.dayNum - 2) * GameManager.instance.healthIncrement)
                            {
                                zone.monster.currentHealth += GameManager.instance.healthIncrement;
                            }

                            SpawnMonsterOnMap(zone.monster, zone);
                        }
                    }
                }

                int smallMonstersBefore = zone.smallMonsters.Count();
                zone.smallMonsters = zone.smallMonsters.Where(x => x.currentHealth > 0).ToList();
                int smallMonstersNow = zone.smallMonsters.Count();

                if (smallMonstersNow < smallMonstersBefore)
                {
                    for (int j = 0; j < smallMonstersBefore - smallMonstersNow; j++)
                    {
                        SpawnMonsterOnMap(new MonsterInstance(smallMonsters[UnityEngine.Random.Range(0, smallMonsters.Length)]));
                    }
                }

                foreach (MonsterInstance monster in zone.smallMonsters)
                {
                    SpawnMonsterOnMap(monster, zone);
                }
            }

            if (bigMonsterDied)
            {
                SpawnMonsterOnMap(new MonsterInstance(bigMonsters[UnityEngine.Random.Range(0, bigMonsters.Length)]));
            }
        }

        dayText.text = "Day " + GameManager.instance.dayNum;

        List<Zone> connectedZones = FindMapConnections(GameManager.instance.zoneId);
        foreach (Zone zone in connectedZones)
        {
            GameObject zoneArrow = Instantiate(zoneArrowPrefab, (currentZonePos + zone.gameObject.transform.position) / 2, Quaternion.identity);
            objs.Add(zoneArrow);
            zoneArrow.transform.right = (zone.gameObject.transform.position - currentZonePos).normalized;
            zone.gameObject.isAdjecent = true;
        }

        GameManager.instance.previousMap = map;
    }

    public void SpawnMonsterOnMap(MonsterInstance monster)
    {
        if (monster.monsterAsset.isBig)
        {
            Zone[] choosableZones = map.Where(x => x.type != ZoneType.Village && x.monster == null && x.gameObject.id != GameManager.instance.zoneId).ToArray();
            Zone chosenZone = choosableZones[UnityEngine.Random.Range(0, choosableZones.Count())];
            MonsterPiece smallPiece = Instantiate(monsterPiecePrefab, chosenZone.gameObject.transform.position + (Vector3)monster.monsterAsset.monsterOffset, Quaternion.identity);
            objs.Add(smallPiece.gameObject);
            smallPiece.Initialize(monster);
            chosenZone.monster = monster;
        }
        else
        {
            Zone[] choosableZones = map.Where(x =>
            {
                if (x.gameObject.id == GameManager.instance.zoneId) return false;

                switch (x.type)
                {
                    case ZoneType.Village:
                        return false;
                    case ZoneType.Grass:
                        return x.smallMonsters.Count < grassSmallMonsterCap;
                    case ZoneType.Rocky:
                        return x.smallMonsters.Count < rockSmallMonsterCap;
                }

                return false;
            }).ToArray();
            Zone chosenZone = choosableZones[UnityEngine.Random.Range(0, choosableZones.Count())];
            MonsterPiece smallPiece = Instantiate(monsterPiecePrefab, chosenZone.gameObject.transform.position + (Vector3)monster.monsterAsset.monsterOffset, Quaternion.identity);
            objs.Add(smallPiece.gameObject);
            smallPiece.Initialize(monster);
            chosenZone.smallMonsters.Add(monster);
        }
    }

    public void SpawnMonsterOnMap(MonsterInstance monster, Zone zone)
    {
        MonsterPiece piece = Instantiate(monsterPiecePrefab, zone.gameObject.transform.position + (Vector3)(Vector3)monster.monsterAsset.monsterOffset, Quaternion.identity);
        piece.Initialize(monster);
        objs.Add(piece.gameObject);
    }

    public void TravelTo(int id)
    {
        confirmActionMenu.SetActive(true);

        confirmButton.onClick.RemoveAllListeners();

        if (map[id].type == ZoneType.Village)
        {
            confirmText.text = confirmFoodText;
            confirmButton.onClick.AddListener(() =>
            {
                int foodBefore = GameManager.instance.villageFood;
                GameManager.instance.villageFood = Mathf.Min(GameManager.instance.villageFood + GameManager.instance.meatAmount, GameManager.instance.villageFoodMax + villageFoodPerDay);
                GameManager.instance.meatAmount -= GameManager.instance.villageFood - foodBefore;
                GameManager.instance.dayNum++;
                SceneManager.LoadScene("WorldMap");
            });
        }
        else if (map[id].monster == null)
        {
            confirmText.text = confirmMovementText;
            confirmButton.onClick.AddListener(() =>
            {
                GameManager.instance.targetZoneId = id;
                GameManager.instance.monsters = map[id].smallMonsters;
                string[] chosenScenes = map[id].type == ZoneType.Grass ? grassMovingScenes : rockMovingScenes;
                SceneManager.LoadScene(chosenScenes[UnityEngine.Random.Range(0, chosenScenes.Length)]);
            });
        }
        else
        {
            confirmText.text = confirmFightText.Replace("[monster]", map[id].monster.monsterAsset.monsterName);
            confirmButton.onClick.AddListener(() =>
            {
                GameManager.instance.targetZoneId = id;
                string[] chosenScenes = map[id].type == ZoneType.Grass ? grassFightScenes : rockFightScenes;
                List<MonsterInstance> allMonsters = map[id].smallMonsters;
                allMonsters.Add(map[id].monster);
                GameManager.instance.monsters = allMonsters;
                SceneManager.LoadScene(chosenScenes[UnityEngine.Random.Range(0, chosenScenes.Length)]);
            });
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}

[Serializable]
public class Zone
{
    public Hexagon gameObject;
    public ZoneType type;
    public MonsterInstance monster = null;
    public List<MonsterInstance> smallMonsters = new List<MonsterInstance>();
}

public enum ZoneType
{
    Village,
    Grass,
    Rocky
}

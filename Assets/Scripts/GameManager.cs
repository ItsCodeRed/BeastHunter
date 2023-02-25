using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int meatAmount;
    public int meatMax;
    public int potionAmount;
    public int potionMax;
    public int antidoteAmount;
    public int antidoteMax;

    [HideInInspector]
    public List<Zone> previousMap = new List<Zone>();

    public int zoneId = 0;
    public int dayNum = 1;

    public int healthIncrement = 25;
    public float attackMultiplier = 0.1f;

    public int villageFood;
    public int villageFoodMax;

    public int targetZoneId = 0;

    public bool playingGame = false;

    public UnityEvent buttonClickEvent = new UnityEvent();

    public List<MonsterInstance> monsters = new List<MonsterInstance>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
        AddClicks();
    }

    private void OnSceneLoad(Scene arg0, LoadSceneMode arg1)
    {
        AddClicks();
    }

    private void AddClicks()
    {
        List<GameObject> rootObjectsInScene = new List<GameObject>();
        Scene scene = SceneManager.GetActiveScene();
        scene.GetRootGameObjects(rootObjectsInScene);

        for (int i = 0; i < rootObjectsInScene.Count; i++)
        {
            Button[] allComponents = rootObjectsInScene[i].GetComponentsInChildren<Button>(true);
            for (int j = 0; j < allComponents.Length; j++)
            {
                allComponents[j].onClick.AddListener(new UnityAction(buttonClickEvent.Invoke));
            }
        }
    }

    public void ProgressDay()
    {
        dayNum += 1;
        zoneId = targetZoneId;
        previousMap[zoneId].smallMonsters = monsters.Where(x => !x.monsterAsset.isBig).ToList();
        previousMap[zoneId].monster = monsters.Where(x => x.monsterAsset.isBig).FirstOrDefault();
    }

    public void ChangeMeat(int meat)
    {
        meatAmount = Mathf.Clamp(meatAmount + meat, 0, meatMax);
        Inventory.instance.ChangeMeatAmount(meatAmount);
    }

    public void ChangePotion(int potions)
    {
        potionAmount = Mathf.Clamp(potionAmount + potions, 0, potionMax);
        Inventory.instance.ChangePotionAmount(potionAmount);
    }

    public void ChangeAntidote(int antidote)
    {
        antidoteAmount = Mathf.Clamp(antidoteAmount + antidote, 0, antidoteMax);
        Inventory.instance.ChangeAntidoteAmount(antidoteAmount);
    }

    public void ChangeVillageFood(int food)
    {
        villageFood = Mathf.Clamp(villageFood + food, 0, villageFoodMax);
    }

    public void CleanUp()
    {
        dayNum = 0;
        zoneId = 0;
        villageFood = villageFoodMax;
        meatAmount = 0;
        Inventory.instance.ChangeMeatAmount(0);
        potionAmount = 0;
        Inventory.instance.ChangePotionAmount(0);
        antidoteAmount = 0;
        Inventory.instance.ChangeAntidoteAmount(0);
        targetZoneId = 0;
        previousMap = new List<Zone>();
        monsters = new List<MonsterInstance>();
    }
}

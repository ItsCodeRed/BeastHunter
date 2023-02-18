using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public MonsterManager monsterManager;

    public static UIManager instance;

    private Animation anim;

    public Slider bossHealthBar;
    public TMP_Text bossText;

    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text summaryText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private Slider healthBar;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private TMP_Text staminaText;
    [SerializeField] private Slider staminaBar;

    [SerializeField] private Color healthColor;
    [SerializeField] private Color poisonColor;

    private void Awake()
    {
        instance = this;
        anim = GetComponent<Animation>();
    }

    private void Start()
    {
        dayText.text = "Day " + GameManager.instance.dayNum;
    }

    private void Update()
    {
        healthBar.value = Player.instance.health;
        healthBarFill.color = Player.instance.isPoisoned ? poisonColor : healthColor;
        healthText.text = Player.instance.health.ToString();
        staminaBar.value = Player.instance.stamina;
        staminaBar.maxValue = Player.instance.maxStamina;
        staminaText.text = Player.instance.stamina.ToString();
    }

    public void ProgressDay()
    {
        GameManager.instance.ProgressDay();

        SceneManager.LoadScene("WorldMap");
    }

    public void DayFinishScreen(string summary)
    {
        Player.instance.movement.enabled = false;
        Player.instance.movement.body.sharedMaterial = Player.instance.movement.corpseMat;
        Player.instance.spear.GetComponent<Spear>().enabled = false;
        anim.Play("DayComplete");
        summaryText.text = summary;
        if (bossHealthBar != null)
        {
            bossHealthBar.gameObject.SetActive(false);
        }
        foreach(Enemy enemy in MonsterManager.instance.allEnemies)
        {
            if (!enemy.isDead)
            {
                enemy.animator.Play("Idle");
            }
            enemy.enabled = false;
        }
    }
}

[Serializable]
public struct InventorySlot
{
    public TMP_Text nameText;
    public TMP_Text amountText;
    public Image icon;
}

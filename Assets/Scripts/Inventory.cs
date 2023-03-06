using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    [SerializeField] private Sprite selectFrameSprite;
    [SerializeField] private Sprite frameSprite;

    [SerializeField] private TMP_Text meatSlot;
    [SerializeField] private Image meatFrame;
    [SerializeField] private TMP_Text potionSlot;
    [SerializeField] private Image potionFrame;
    [SerializeField] private TMP_Text antidoteSlot;
    [SerializeField] private Image antidoteFrame;

    public bool canSelect = true;
    public InventoryItems selectedItem;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        meatSlot.text = "x" + GameManager.instance.meatAmount.ToString();
        potionSlot.text = "x" + GameManager.instance.potionAmount.ToString();
        antidoteSlot.text = "x" + GameManager.instance.antidoteAmount.ToString();

        if (canSelect)
        {
            ChangeSelectFrame();
        }
    }

    private void Update()
    {
        if (Input.mouseScrollDelta.y != 0 && canSelect)
        {
            int index = Input.mouseScrollDelta.y < 0 ? (((int)selectedItem) + 1) % 3 : (selectedItem == 0 ? 2 : ((int)selectedItem) - 1);
            selectedItem = (InventoryItems)index;
            ChangeSelectFrame();
        }
    }

    private void ChangeSelectFrame()
    {
        meatFrame.sprite = frameSprite;
        potionFrame.sprite = frameSprite;
        antidoteFrame.sprite = frameSprite;

        switch (selectedItem)
        {
            case InventoryItems.potion:
                potionFrame.sprite = selectFrameSprite;
                break;
            case InventoryItems.antidote:
                antidoteFrame.sprite = selectFrameSprite;
                break;
            case InventoryItems.meat:
                meatFrame.sprite = selectFrameSprite;
                break;
            default:
                break;
        }
    }

    public void ChangeMeatAmount(int meatAmount)
    {
        meatSlot.text = "x" + meatAmount;
    }

    public void ChangePotionAmount(int potionAmount)
    {
        potionSlot.text = "x" + potionAmount;
    }

    public void ChangeAntidoteAmount(int antidoteAmount)
    {
        antidoteSlot.text = "x" + antidoteAmount;
    }
}

public enum InventoryItems
{
    potion = 0,
    antidote,
    meat,
}

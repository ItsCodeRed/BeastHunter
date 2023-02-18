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

    public bool canSelect = true;
    public bool selectingPotion = true;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        meatSlot.text = "x" + GameManager.instance.meatAmount.ToString();
        potionSlot.text = "x" + GameManager.instance.potionAmount.ToString();

        if (canSelect)
        {
            ChangeSelectFrame();
        }
    }

    private void Update()
    {
        if (Input.mouseScrollDelta.y != 0 && canSelect)
        {
            selectingPotion = !selectingPotion;
            ChangeSelectFrame();
        }
    }

    private void ChangeSelectFrame()
    {
        if (selectingPotion)
        {
            meatFrame.sprite = frameSprite;
            potionFrame.sprite = selectFrameSprite;
        }
        else
        {
            meatFrame.sprite = selectFrameSprite;
            potionFrame.sprite = frameSprite;
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
}

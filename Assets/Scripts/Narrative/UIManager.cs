using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("��ɫ�¼�SO")]
    public CharacterEvent_SO onHealthChange;
    public CurrencyData currencyData;

    [Header("��ɫUI")]
    public PlayerStateBar playerStateBar;
    public PlayerCurrencyAmount playerCurrencyAmount;

    [Header("ƿ�����")]
    public BottleData bottleData;
    public Transform bottleParent;
    public GameObject bottlePrefab;

    private List<GameObject> bottleInstances = new List<GameObject>();

    private void OnEnable()
    {
        onHealthChange.OnEventRaised += HealthChange;
        currencyData.OnCurrencyChanged += CurrencyChange;
        bottleData.OnBottleAmountChanged += UpdateBottles;
    }

    private void OnDisable()
    {
        onHealthChange.OnEventRaised -= HealthChange;
        currencyData.OnCurrencyChanged -= CurrencyChange;
        bottleData.OnBottleAmountChanged -= UpdateBottles;
    }

    private void Start()
    {
        currencyData.UpdateAmount();
        UpdateBottles(bottleData.currentAmount);

        // 补刷新：血量切换场景后推初始值到UI
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Character ch = player.GetComponent<Character>();
            if (ch != null)
            {
                float pct = ch.currentHealth / ch.maxHealth;
                playerStateBar.OnHealthChange(pct);
            }
        }
    }

    private void HealthChange(Character character)
    {
        float persent = character.currentHealth / character.maxHealth;
        playerStateBar.OnHealthChange(persent);
    }

    private void CurrencyChange(int amount)
    {
        playerCurrencyAmount.UpdateCurrency(amount);
    }

    private void UpdateBottles(int amount)
    {
        // ����֮ǰ��ƿ��ʵ��
        foreach (var bottle in bottleInstances)
        {
            Destroy(bottle);
        }
        bottleInstances.Clear();

        // �����µ�ƿ��ʵ��
        for (int i = 0; i < bottleData.maxAmount; i++)
        {
            GameObject bottle = Instantiate(bottlePrefab, bottleParent);
            bottleInstances.Add(bottle);

            Image bottleImage = bottle.GetComponent<Image>();
            if (i < amount)
            {
                // ��ƿ״̬
                bottleImage.sprite = bottleData.fullBottleSprite;
            }
            else
            {
                // ��ƿ״̬
                bottleImage.sprite = bottleData.emptyBottleSprite;
            }
        }
    }
}
using GameArchitecture.Core;
using UnityEngine;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [Header("�̵���Ʒ")]
    public List<ShopItemData> shopItems = new List<ShopItemData>();

    [Header("UI����")]
    public GameObject shopPanel;
    public ShopItemSlot[] itemSlots;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadAllItemData();
        InitializeShopUI();
        CloseShop();

        Debug.Log("�̵��������ʼ����ɡ���Ʒ����: " + shopItems.Count);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseShop();
        }
    }

    private void LoadAllItemData()
    {
        foreach (ShopItemData item in shopItems)
        {
            item.LoadItemData();
            Debug.Log($"�Ѽ�����Ʒ: {item.itemName}, ����: {item.currentAmount}");
        }
    }

    private void InitializeShopUI()
    {
        for (int i = 0; i < itemSlots.Length && i < shopItems.Count; i++)
        {
            itemSlots[i].SetItem(shopItems[i]);
        }
    }

    public void OpenShop()
    {
        shopPanel.SetActive(true);
        Time.timeScale = 0;
        PlayerController player = Blackboard.PlayerTransform?.GetComponent<PlayerController>();
        if (player != null)
        {
            player.inputControl.GamePlay.Disable();
        }
        Debug.Log("�̵��Ѵ�!");
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
        Time.timeScale = 1;
        PlayerController player = Blackboard.PlayerTransform?.GetComponent<PlayerController>();
        if (player != null)
        {
            player.inputControl.GamePlay.Enable();
        }
        Debug.Log("�̵��ѹر�!");
    }

    public bool PurchaseItem(ShopItemData item)
    {
        int currentCurrency = PlayerItemManager.Instance.GetCurrencyAmount("Gold");

        Debug.Log($"=== ���Թ���: {item.itemName} ===");
        Debug.Log($"��ǰ���: {currentCurrency}");
        Debug.Log($"��Ʒ�۸�: {item.price}");
        Debug.Log($"��ǰ����: {item.currentAmount}/{item.maxAmount}");

        if (currentCurrency >= item.price && item.currentAmount < item.maxAmount)
        {
            bool spendSuccess = PlayerItemManager.Instance.SpendCurrency("Gold", item.price);
            Debug.Log($"�۳���ҳɹ�: {spendSuccess}");

            if (spendSuccess)
            {
                item.AddItem(1);
                Debug.Log($"����ɹ�! ������: {item.currentAmount}");
                UpdateShopUI();
                return true;
            }
            else
            {
                Debug.Log("����ʧ��! ���δ�۳���");
                return false;
            }
        }
        else
        {
            if (currentCurrency < item.price)
            {
                Debug.Log("����ʧ��! ��Ҳ��㡣");
            }
            if (item.currentAmount >= item.maxAmount)
            {
                Debug.Log("����ʧ��! �Ѵ����������");
            }
            return false;
        }
    }

    public void UseItem(ShopItemData item)
    {
        Debug.Log($"=== ����ʹ����Ʒ: {item.itemName} ===");
        Debug.Log($"��ǰ����: {item.currentAmount}");

        if (item.UseItem())
        {
            Debug.Log($"ʹ�óɹ�! ������: {item.currentAmount}");
            UpdateShopUI();
        }
        else
        {
            Debug.Log("ʹ��ʧ��! û����Ʒ��ʹ�á�");
        }
    }

    public void UpdateShopUI()
    {
        Debug.Log("���ڸ����̵�UI...");
        foreach (ShopItemSlot slot in itemSlots)
        {
            if (slot.currentItem != null)
            {
                slot.UpdateAmount();
                Debug.Log($"�Ѹ��²�λ: {slot.currentItem.itemName}, ����: {slot.currentItem.currentAmount}");
            }
        }
    }

    public ShopItemData GetItemByName(string itemName)
    {
        foreach (ShopItemData item in shopItems)
        {
            if (item.itemName == itemName)
            {
                return item;
            }
        }
        return null;
    }
}

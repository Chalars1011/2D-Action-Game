using UnityEngine;

[CreateAssetMenu(fileName = "New Shop Item", menuName = "Inventory/Shop Item")]
public class ShopItemData : ScriptableObject
{
    public string itemName;
    public string description;
    public Sprite icon;
    public int price;
    public KeyCode hotkey;

    [HideInInspector] public int currentAmount;
    public int maxAmount;

    public void AddItem(int amount)
    {
        Debug.Log($"物品数据.AddItem: {itemName}, 增加数量: {amount}, 之前: {currentAmount}");
        currentAmount = Mathf.Min(currentAmount + amount, maxAmount);
        Debug.Log($"物品数据.AddItem: {itemName}, 之后: {currentAmount}");
        SaveItemData();
    }

    public bool UseItem()
    {
        Debug.Log($"物品数据.UseItem: {itemName}, 当前数量: {currentAmount}");
        if (currentAmount > 0)
        {
            currentAmount--;
            Debug.Log($"物品数据.UseItem: {itemName}, 使用后: {currentAmount}");
            SaveItemData();
            return true;
        }
        return false;
    }

    public void SaveItemData()
    {
        PlayerPrefs.SetInt($"ShopItem_{itemName}_Amount", currentAmount);
        PlayerPrefs.Save();
        Debug.Log($"物品数据.SaveItemData: {itemName} 已保存，数量: {currentAmount}");
    }

    public void LoadItemData()
    {
        currentAmount = PlayerPrefs.GetInt($"ShopItem_{itemName}_Amount", 0);
        Debug.Log($"物品数据.LoadItemData: {itemName} 已加载，数量: {currentAmount}");
    }
}

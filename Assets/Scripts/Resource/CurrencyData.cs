using UnityEngine;

[CreateAssetMenu(fileName = "NewCurrencyData", menuName = "Inventory/Currency Data")]
public class CurrencyData : ScriptableObject
{
    public string currencyName;
    public int currentAmount;
    public int maxAmount;
    public Sprite icon;

    // 货币变化事件
    public event System.Action<int> OnCurrencyChanged;

    // 添加货币
    public void AddCurrency(int amount)
    {
        if (amount < 0)
        {
          //  Debug.LogError("Cannot add negative amount of currency.");
            return;
        }

        currentAmount = Mathf.Clamp(currentAmount + amount, 0, maxAmount);
        OnCurrencyChanged?.Invoke(currentAmount);
    }

    //刷新
    public void UpdateAmount() 
    {
        OnCurrencyChanged?.Invoke(currentAmount);
    }

    // 消耗货币
    public bool SpendCurrency(int amount)
    {
        if (amount < 0)
        {
         //   Debug.LogError("Cannot spend negative amount of currency.");
            return false;
        }

        if (currentAmount >= amount)
        {
            currentAmount -= amount;
            OnCurrencyChanged?.Invoke(currentAmount);
            return true;
        }
        return false;
    }
}
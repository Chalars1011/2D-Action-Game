using UnityEngine;

[CreateAssetMenu(fileName = "NewBottleData", menuName = "Inventory/Bottle Data")]
public class BottleData : ScriptableObject
{
    public string bottleName;
    public int currentAmount;
    public int maxAmount;
    public Sprite fullBottleSprite;
    public Sprite emptyBottleSprite;

    // 瓶子效果类型枚举
    public enum BottleEffectType
    {
        Heal,
        ManaRestore,
        EnergyRestore
        // 可以根据需要添加更多效果类型
    }

    public BottleEffectType effectType;
    public int effectAmount;

    // 瓶子数量变化事件
    public event System.Action<int> OnBottleAmountChanged;

    // 添加瓶子
    public void AddBottle(int amount)
    {
        if (amount < 0)
        {
            Debug.LogError("Cannot add negative amount of bottles.");
            return;
        }

        currentAmount = Mathf.Clamp(currentAmount + amount, 0, maxAmount);
        OnBottleAmountChanged?.Invoke(currentAmount);
    }

    public void UpdateAmount()
    {
        OnBottleAmountChanged?.Invoke(currentAmount);
    }

    // 使用瓶子
    public bool UseBottle()
    {
        if (currentAmount > 0)
        {
            currentAmount--;
            OnBottleAmountChanged?.Invoke(currentAmount);
            ApplyEffect();
            return true;
        }
        return false;
    }

    // 应用瓶子效果
    private void ApplyEffect()
    {
        switch (effectType)
        {
            case BottleEffectType.Heal:
                // 查找玩家角色（假设玩家标签为"Player"）
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null)
                {
                    Character playerCharacter = playerObject.GetComponent<Character>();
                    if (playerCharacter != null)
                    {
                        playerCharacter.ApplyHealEffect(effectAmount);
                        return;
                    }
                }
                Debug.LogError("Player character not found when applying heal effect.");
                break;
            case BottleEffectType.ManaRestore:
                // 实现回蓝逻辑
                Debug.Log($"Restored {effectAmount} mana!");
                break;
            case BottleEffectType.EnergyRestore:
                // 实现恢复能量逻辑
                Debug.Log($"Restored {effectAmount} energy!");
                break;
        }
    }

    // 增加最大血瓶量
    public void IncreaseMaxBottles(int increaseAmount)
    {
        if (increaseAmount < 0)
        {
            Debug.LogError("Cannot increase max bottles by a negative amount.");
            return;
        }

        maxAmount += increaseAmount;
        // 确保当前血瓶数量不超过新的最大血瓶数量
        currentAmount = Mathf.Clamp(currentAmount, 0, maxAmount);
        OnBottleAmountChanged?.Invoke(currentAmount);
    }

    // 增加瓶子的回复量
    public void IncreaseEffectAmount(int increaseAmount)
    {
        if (increaseAmount < 0)
        {
            Debug.LogError("Cannot increase effect amount by a negative amount.");
            return;
        }

        effectAmount += increaseAmount;
    }
}
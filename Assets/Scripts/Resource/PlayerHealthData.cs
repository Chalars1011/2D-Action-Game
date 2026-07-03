using UnityEngine;

[CreateAssetMenu(fileName = "PlayerHealthData", menuName = "ScriptableObjects/PlayerHealthData")]
public class PlayerHealthData : ScriptableObject
{
    public float currentHealth;
    public float maxHealth;

    // 运行时重置用（新游戏开始时调用）
    public void ResetToMax()
    {
        currentHealth = maxHealth;
    }
}

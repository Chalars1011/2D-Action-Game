using UnityEngine;
using GameArchitecture.Narrative;

/// <summary>
/// 玩家状态追踪器——实现 IStateTracked，上报玩家需要持久化的数据。
/// 挂在 Player 上。
/// </summary>
public class PlayerStateTracker : MonoBehaviour, IStateTracked
{
    string IStateTracked.StateId => "Player";

    string IStateTracked.CaptureState()
    {
        var c = GetComponent<Character>();
        return JsonUtility.ToJson(new PlayerData
        {
            posX = transform.position.x, posY = transform.position.y,
            health = c != null ? c.currentHealth : 100f,
            maxHealth = c != null ? c.maxHealth : 100f,
            mana = c != null ? c.currentMana : 100f,
            maxMana = c != null ? c.maxMana : 100f,
            faceDir = transform.localScale.x > 0 ? 1 : -1
        });
    }

    void IStateTracked.RestoreState(string json)
    {
        var d = JsonUtility.FromJson<PlayerData>(json);
        transform.position = new Vector3(d.posX, d.posY, transform.position.z);
        transform.localScale = new Vector3(d.faceDir, transform.localScale.y, transform.localScale.z);

        var c = GetComponent<Character>();
        if (c != null)
        {
            c.currentHealth = d.health;
            c.maxHealth = d.maxHealth;
            c.currentMana = d.mana;
            c.maxMana = d.maxMana;
        }
    }

    private void Awake()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.RegisterTracker(this);
    }

    [System.Serializable]
    private class PlayerData
    {
        public float posX, posY, health, maxHealth, mana, maxMana;
        public int faceDir;
    }
}

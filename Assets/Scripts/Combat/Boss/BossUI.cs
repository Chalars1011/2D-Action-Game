using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossUI : MonoBehaviour
{
    public Image tongguan;
    public Image Red;         // 血条前景图
    public Image Red_Slow;    // 血条背景图（缓慢变化效果）
    public BossBase boss;     // 敌人Boss引用

    private Character bossCharacter;    // 缓存敌人角色组件
    private float lastHealth;           // 上次记录的血量

    void Start()
    {
        // 缓存组件引用（避免重复查找）
        if (boss != null)
        {
            bossCharacter = boss.GetComponent<Character>();
            if (bossCharacter == null)
                Debug.LogError("BossUI: 未找到Character组件");
            else
                lastHealth = bossCharacter.currentHealth;
        }
        else
        {
            Debug.LogError("BossUI: 未指定boss对象");
        }
    }

    void Update()
    {
        if (bossCharacter == null) return;

       
        float currentHealth = bossCharacter.currentHealth/bossCharacter.maxHealth;

        // 更新前景血条（实时变化）
        Red.fillAmount = currentHealth;

        // 更新背景血条（缓慢变化效果）
        if (Red_Slow.fillAmount > currentHealth)
        {
            Red_Slow.fillAmount -= Time.deltaTime * 0.3f;
            // 防止低于前景血条
            Red_Slow.fillAmount = Mathf.Max(Red_Slow.fillAmount, Red.fillAmount);
        }
        else if (Red_Slow.fillAmount < currentHealth)
        {
            Red_Slow.fillAmount += Time.deltaTime * 0.1f;
            Red_Slow.fillAmount = Mathf.Min(Red_Slow.fillAmount, Red.fillAmount);
        }

        if (bossCharacter.currentHealth <= 0) 
        {
            tongguan.gameObject.SetActive(true);
        }
        // 记录当前血量用于下次更新
        lastHealth = currentHealth;
    }
}
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 排行榜系统 —— 在 feature/rank-system 分支上开发
/// 这个分支上的代码不影响 master 分支的同事
/// </summary>
public class RankSystem : MonoBehaviour
{
    public List<RankEntry> topPlayers = new List<RankEntry>();

    void Start()
    {
        Debug.Log("排行榜系统初始化——这是第一版，还没做完");
    }

    public void RefreshRank()
    {
        Debug.Log("排行榜刷新中...功能待完善");
    }
}

[System.Serializable]
public class RankEntry
{
    public string playerName;
    public int score;
    public int rank;
}

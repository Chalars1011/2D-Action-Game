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
        // 模拟从服务器拉取排行榜数据
        topPlayers.Clear();
        topPlayers.Add(new RankEntry { playerName = "测试玩家1", score = 9999, rank = 1 });
        topPlayers.Add(new RankEntry { playerName = "测试玩家2", score = 8888, rank = 2 });
        Debug.Log($"排行榜已刷新，共 {topPlayers.Count} 条记录");
    }

    public List<RankEntry> GetTopPlayers(int count)
    {
        if (topPlayers.Count <= count) return topPlayers;
        return topPlayers.GetRange(0, count);
    }
}

[System.Serializable]
public class RankEntry
{
    public string playerName;
    public int score;
    public int rank;
}

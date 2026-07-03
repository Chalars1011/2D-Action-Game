using UnityEditor;
using UnityEngine;

/// <summary>
/// 一键生成框架所需的所有 ScriptableObject 资产。
/// 菜单：Tools → Generate All SO Assets
/// </summary>
public class SOAssetGenerator
{
    [MenuItem("Tools/Generate All SO Assets", false, 150)]
    public static void GenerateAll()
    {
        EnsureFolder("Assets/_Game/ScriptableObjects/Configs");

        CreateAsset<PlayerConfig_SO>("Assets/_Game/ScriptableObjects/Configs/PlayerConfig.asset",
            "PlayerConfig");
        CreateAsset<EnemyConfig_SO>("Assets/_Game/ScriptableObjects/Configs/EnemyConfig_Default.asset",
            "EnemyConfig_Default");
        CreateAsset<EnemyConfig_SO>("Assets/_Game/ScriptableObjects/Configs/EnemyConfig_Elite.asset",
            "EnemyConfig_Elite (精英怪: 1.5x 速度/伤害)");
        CreateAsset<CharacterStats_SO>("Assets/_Game/ScriptableObjects/Configs/PlayerStats.asset",
            "PlayerStats");
        CreateAsset<CharacterStats_SO>("Assets/_Game/ScriptableObjects/Configs/BossStats.asset",
            "BossStats");

        // Shake profiles
        EnsureFolder("Assets/_Game/ScriptableObjects/Configs/ShakeProfiles");
        CreateAsset<ShakeProfile_SO>("Assets/_Game/ScriptableObjects/Configs/ShakeProfiles/Shake_Light.asset",
            "Shake_Light (轻攻击)");
        CreateAsset<ShakeProfile_SO>("Assets/_Game/ScriptableObjects/Configs/ShakeProfiles/Shake_Heavy.asset",
            "Shake_Heavy (重攻击)");
        CreateAsset<ShakeProfile_SO>("Assets/_Game/ScriptableObjects/Configs/ShakeProfiles/Shake_Hurt.asset",
            "Shake_Hurt (受击)");

        AssetDatabase.Refresh();
        Debug.Log("[SO Generator] All assets created. Check _Game/ScriptableObjects/Configs/");
    }

    private static void CreateAsset<T>(string path, string displayName) where T : ScriptableObject
    {
        // 如果已存在则跳过
        if (AssetDatabase.LoadAssetAtPath<T>(path) != null)
        {
            Debug.Log($"[SO Generator] Skip (exists): {displayName}");
            return;
        }

        var asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        Debug.Log($"[SO Generator] Created: {displayName} at {path}");
    }

    private static void EnsureFolder(string path)
    {
        string[] parts = path.Split('/');
        string current = "";
        foreach (var part in parts)
        {
            if (string.IsNullOrEmpty(part)) continue;
            string parent = current;
            current = string.IsNullOrEmpty(current) ? part : $"{current}/{part}";
            if (!AssetDatabase.IsValidFolder(current))
                AssetDatabase.CreateFolder(parent, part);
        }
    }
}

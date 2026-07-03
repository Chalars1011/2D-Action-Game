using UnityEditor;
using UnityEngine;
using GameArchitecture.Combat;
using GameArchitecture.Actor;

/// <summary>
/// 框架迁移工具——一键给 Prefab 添加新架构组件。
/// Unity 菜单: Tools → Framework Migration → ...
/// </summary>
public class FrameworkMigrationTool
{
    [MenuItem("Tools/Framework Migration/1. Migrate Player", false, 100)]
    public static void MigratePlayer()
    {
        MigratePrefab("Assets/_Game/Prefabs/Player/Player.prefab", (go) =>
        {
            AddIfMissing<HealthComponent>(go, "HealthComponent");
            AddIfMissing<PlayerStats>(go, "PlayerStats");
        });
    }

    [MenuItem("Tools/Framework Migration/2. Migrate All Enemies", false, 101)]
    public static void MigrateAllEnemies()
    {
        string[] paths =
        {
            "Assets/_Game/Prefabs/Enemies/QiangDao.prefab",
            "Assets/_Game/Prefabs/Enemies/DiuDiuGuai.prefab",
            "Assets/_Game/Prefabs/Enemies/ShiTouRen.prefab",
            "Assets/_Game/Prefabs/Enemies/ShiTouRen_Elite.prefab",
        };

        foreach (var path in paths)
        {
            MigratePrefab(path, (go) =>
            {
                AddIfMissing<HealthComponent>(go, "HealthComponent");
                AddIfMissing<EnemyDeathHandler>(go, "EnemyDeathHandler");
            });
        }
    }

    [MenuItem("Tools/Framework Migration/3. Migrate Boss", false, 102)]
    public static void MigrateBoss()
    {
        MigratePrefab("Assets/_Game/Prefabs/Boss/Boss_KanMenRen.prefab", (go) =>
        {
            AddIfMissing<HealthComponent>(go, "HealthComponent");
            AddIfMissing<EnemyDeathHandler>(go, "EnemyDeathHandler");
        });
    }

    [MenuItem("Tools/Framework Migration/4. Migrate Attack Effects", false, 103)]
    public static void MigrateAttackEffects()
    {
        string[] paths =
        {
            "Assets/Resources/Attack_1_Effect.prefab",
            "Assets/Resources/Attack_2_Effect.prefab",
            "Assets/Resources/Attack_3_Effect.prefab",
        };

        foreach (var path in paths)
        {
            MigratePrefab(path, (go) =>
            {
                AddIfMissing<HitboxComponent>(go, "HitboxComponent");
                AddIfMissing<HitPresentation>(go, "HitPresentation");
            });
        }
    }

    // ============================================================
    // Helpers
    // ============================================================

    private static void AddIfMissing<T>(GameObject go, string label) where T : Component
    {
        if (go.GetComponent<T>() == null)
        {
            go.AddComponent<T>();
            Debug.Log($"[Migration] +{label} on {go.name}");
        }
        else
        {
            Debug.Log($"[Migration] {label} already exists on {go.name} (skip)");
        }
    }

    private static void MigratePrefab(string assetPath, System.Action<GameObject> action)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (prefab == null)
        {
            Debug.LogWarning($"[Migration] Not found: {assetPath}");
            return;
        }

        // Load prefab contents for editing
        GameObject instance = PrefabUtility.LoadPrefabContents(assetPath);
        try
        {
            action(instance);
            PrefabUtility.SaveAsPrefabAsset(instance, assetPath);
            Debug.Log($"[Migration] Done: {assetPath}");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(instance);
        }
    }
}

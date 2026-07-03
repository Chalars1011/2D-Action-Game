using UnityEngine;
using UnityEditor;

public class SpriteMaterialSwapper : EditorWindow
{
    public Material targetMaterial;

    [MenuItem("Tools/一键替换所有精灵材质")]
    static void ShowWindow()
    {
        GetWindow<SpriteMaterialSwapper>("精灵材质替换");
    }

    void OnGUI()
    {
        GUILayout.Label("把所有SpriteRenderer材质替换为指定材质");
        targetMaterial = (Material)EditorGUILayout.ObjectField("目标材质", targetMaterial, typeof(Material), false);

        if (GUILayout.Button("替换所有场景精灵材质"))
        {
            if (targetMaterial == null)
            {
                Debug.LogError("请先拖一个材质进来");
                return;
            }

            int count = 0;
            SpriteRenderer[] allSprites = FindObjectsOfType<SpriteRenderer>(true);
            foreach (SpriteRenderer sr in allSprites)
            {
                Undo.RecordObject(sr, "Swap Sprite Material");
                sr.material = targetMaterial;
                count++;
            }

            Debug.Log($"已替换 {count} 个SpriteRenderer的材质为 {targetMaterial.name}");
        }
    }
}

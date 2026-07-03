using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Unity Bridge — 文件驱动，零第三方依赖。
/// 
/// 方式一（自动）: 写入 bridge/cmd.json，点菜单 Bridge → Execute Command 执行
/// 方式二（截图）: 菜单 Bridge → Take Screenshot
/// 方式三（执行）: 菜单 Bridge → Execute Command 读取 cmd.json 并执行
/// </summary>
public class UnityBridge : EditorWindow
{
    private static readonly string CmdPath = "bridge/cmd.json";
    private static readonly string ResultPath = "bridge/result.json";

    [MenuItem("Bridge/Execute Command", false, 200)]
    public static void ExecuteFromFile()
    {
        string fullPath = Path.Combine(Application.dataPath, "../bridge/cmd.json");
        if (!File.Exists(fullPath))
        {
            Debug.Log("[Bridge] No cmd.json found");
            return;
        }

        try
        {
            string json = File.ReadAllText(fullPath);
            var cmd = JsonUtility.FromJson<BridgeCommand>(json);
            string result = Execute(cmd);
            WriteResult(result);
            Debug.Log($"[Bridge] Done: {cmd.action}");
        }
        catch (Exception ex)
        {
            WriteError(ex.Message);
            Debug.LogError($"[Bridge] {ex.Message}");
        }
    }

    [MenuItem("Bridge/Take Screenshot", false, 201)]
    public static void TakeShot()
    {
        string path = Path.Combine(Application.dataPath, "../bridge/screenshot.png");
        ScreenCapture.CaptureScreenshot("bridge/screenshot.png");
        Debug.Log("[Bridge] Screenshot saved to bridge/screenshot.png");
    }

    [MenuItem("Bridge/Show Console Logs", false, 202)]
    public static void ShowConsole()
    {
        string logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Unity/Editor/Editor.log");
        if (File.Exists(logPath))
        {
            string[] lines = File.ReadAllLines(logPath);
            int start = Math.Max(0, lines.Length - 30);
            for (int i = start; i < lines.Length; i++)
                if (lines[i].Contains("Exception") || lines[i].Contains("Error") || lines[i].Contains("error CS"))
                    Debug.Log(lines[i]);
            Debug.Log($"[Bridge] Showing errors from last {lines.Length - start} lines");
        }
    }

    private static string Execute(BridgeCommand cmd)
    {
        switch (cmd.action)
        {
            case "screenshot":
                ScreenCapture.CaptureScreenshot("bridge/screenshot.png");
                return "ok";
            case "exec":
                Debug.Log($"[Bridge Exec] {cmd.code}");
                return "ok (logged to console)";
            case "log":
                ShowConsole();
                return "ok";
            default:
                return $"unknown: {cmd.action}";
        }
    }

    private static void WriteResult(string msg)
    {
        string fullPath = Path.Combine(Application.dataPath, "../bridge/result.json");
        File.WriteAllText(fullPath, $"{{\"status\":\"ok\",\"message\":\"{msg}\"}}");
    }

    private static void WriteError(string msg)
    {
        string fullPath = Path.Combine(Application.dataPath, "../bridge/result.json");
        File.WriteAllText(fullPath, $"{{\"status\":\"error\",\"message\":\"{msg}\"}}");
    }

    [Serializable]
    private class BridgeCommand
    {
        public string action;
        public string code;
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// 传送门管理器 - 处理场景加载
public class PortalManager : MonoBehaviour
{
    // 单例模式
    public static PortalManager Instance { get; private set; }

    [Header("玩家设置")]
    [SerializeField] private GameObject playerPrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 传送玩家到目标场景和位置
    public void Teleport(string sceneName, string spawnPointName, int faceDirection)
    {
        // 保存传送数据
        PlayerPrefs.SetString("SpawnPointName", spawnPointName);
        PlayerPrefs.SetInt("FaceDirection", faceDirection);

        // 开始淡入过程
        FadeManager.FadeIn(() => {
            // 淡入完成后加载场景
            StartCoroutine(LoadSceneAsync(sceneName));
        });
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // 加载目标场景
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);

        // 等待场景加载完成
        while (!loadOperation.isDone)
        {
            yield return null;
        }

        // 延迟一帧，确保所有初始化完成
        yield return null;

        // 场景加载完成后淡出
        FadeManager.FadeOut();
    }
}
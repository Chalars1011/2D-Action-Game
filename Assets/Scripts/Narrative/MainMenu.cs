using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("第一个关卡场景名")]
    public string firstSceneName = "DuShen_1";

    [Header("按钮音效")]
    public AudioClip clickSound;

    [Header("开始画面BGM")]
    public AudioEventData_SO menuBGM;

    private AudioSource menuAudioSource;

    void Start()
    {
        // 自己管BGM，不依赖BGMPlayer单例
        if (menuBGM != null && menuBGM.clips.Count > 0)
        {
            menuAudioSource = gameObject.AddComponent<AudioSource>();
            menuAudioSource.clip = menuBGM.clips[0];
            menuAudioSource.loop = true;
            menuAudioSource.playOnAwake = false;
            menuAudioSource.spatialBlend = 0f;
            menuAudioSource.volume = menuBGM.volume > 0 ? menuBGM.volume : 0.6f;
            menuAudioSource.Play();
        }
    }

    public void StartGame()
    {
        if (clickSound != null)
            AudioManager.Instance.PlayAtPoint(clickSound, Vector3.zero, 1f, AudioCategory.UI);

        StartCoroutine(StartGameRoutine());
    }

    private IEnumerator StartGameRoutine()
    {
        // 黑屏
        FadeManager.FadeIn();

        // 等黑屏时间
        yield return new WaitForSeconds(0.5f);

        // 加载场景（FadeManager的OnSceneLoaded会自动FadeOut）
        SceneManager.LoadSceneAsync(firstSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

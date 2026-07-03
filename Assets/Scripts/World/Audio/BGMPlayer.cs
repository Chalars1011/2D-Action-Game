using UnityEngine;

public class BGMPlayer : MonoBehaviour
{
    [Header("初始BGM")]
    public AudioEventData_SO defaultBGM;

    [Header("BGM音量")]
    [Range(0f, 1f)]
    public float volume = 0.6f;

    private static BGMPlayer instance;
    private AudioSource bgmSource;
    private AudioEventData_SO currentBGM;

    void Awake()
    {
        // 单例：只保留第一个，新场景的销毁
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // 停掉对象池里所有残留BGM
        AudioManager.Instance.StopCategory(AudioCategory.BGM);

        GameObject go = new GameObject("_BGM_Source");
        go.transform.SetParent(transform);
        bgmSource = go.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.spatialBlend = 0f;
        bgmSource.volume = volume;

        if (defaultBGM != null)
            PlayBGM(defaultBGM);
    }

    public static void Play(AudioEventData_SO bgmData)
    {
        if (instance != null)
            instance.PlayBGM(bgmData);
    }

    public void PlayBGM(AudioEventData_SO bgmData)
    {
        if (bgmData == null || bgmData.clips == null || bgmData.clips.Count == 0) return;
        if (bgmData == currentBGM) return;

        AudioClip clip = bgmData.clips[0];
        if (clip == null) return;

        currentBGM = bgmData;
        bgmSource.clip = clip;
        bgmSource.volume = bgmData.volume > 0 ? bgmData.volume : volume;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
        currentBGM = null;
    }

    public void SetVolume(float vol)
    {
        volume = vol;
        bgmSource.volume = vol;
    }
}

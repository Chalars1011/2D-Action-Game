using UnityEngine;

public class AmbientPlayer : MonoBehaviour
{
    [Header("环境音（拖AudioEventData_SO进来）")]
    public AudioEventData_SO ambientSound;

    [Header("音量")]
    [Range(0f, 1f)]
    public float volume = 0.4f;

    // 静态引用，跨场景只保留一个
    private static AudioSource currentAmbientSource;

    void Start()
    {
        if (ambientSound == null) { Destroy(gameObject); return; }

        // 停掉对象池里所有Ambient音源
        AudioManager.Instance.StopCategory(AudioCategory.Ambient);

        // 停掉上一个场景残留的独立AudioSource
        if (currentAmbientSource != null && currentAmbientSource)
        {
            Destroy(currentAmbientSource.gameObject);
            currentAmbientSource = null;
        }

        // 独立创建AudioSource，不经过对象池
        GameObject go = new GameObject("_Ambient_Source_" + gameObject.scene.name);
        go.transform.SetParent(transform);
        currentAmbientSource = go.AddComponent<AudioSource>();
        currentAmbientSource.spatialBlend = 0f;
        currentAmbientSource.loop = true;
        currentAmbientSource.playOnAwake = false;
        currentAmbientSource.volume = ambientSound.volume > 0 ? ambientSound.volume : volume;

        AudioClip clip = ambientSound.clips.Count > 0 ? ambientSound.clips[0] : null;
        if (clip != null)
        {
            currentAmbientSource.clip = clip;
            currentAmbientSource.Play();
            Debug.Log($"[AmbientPlayer] 播放环境音: {clip.name}，场景: {gameObject.scene.name}");
        }
    }

    void OnDestroy()
    {
        if (currentAmbientSource != null && currentAmbientSource.isPlaying)
        {
            Debug.Log($"[AmbientPlayer] 停止环境音: {currentAmbientSource.clip?.name}");
            currentAmbientSource.Stop();
        }
    }
}

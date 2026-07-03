using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class AudioManager : Singleton<AudioManager>
{
    [Header("池容量 - 每类同时可播数量")]

    [Header("── 角色 ──")]
    public int footstepPoolSize = 2;
    public int movementPoolSize = 2;
    public int weaponPoolSize = 3;
    public int heavyAttackPoolSize = 2;
    public int skillPoolSize = 3;
    public int hitPoolSize = 3;
    public int deathPoolSize = 1;

    [Header("── 投射物 ──")]
    public int projectilePoolSize = 5;
    public int impactPoolSize = 4;

    [Header("── 道具 & 交互 ──")]
    public int itemPoolSize = 3;
    public int mechanismPoolSize = 2;

    [Header("── 界面 & 环境 ──")]
    public int uiPoolSize = 3;
    public int bgmPoolSize = 1;
    public int ambientPoolSize = 2;
    public int voicePoolSize = 2;

    private Dictionary<AudioCategory, Queue<AudioSource>> pools = new Dictionary<AudioCategory, Queue<AudioSource>>();
    private Dictionary<AudioCategory, HashSet<AudioSource>> active = new Dictionary<AudioCategory, HashSet<AudioSource>>();
    private GameObject poolRoot;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        BuildAllPools();
    }

    private void BuildAllPools()
    {
        poolRoot = new GameObject("_AudioPool");
        poolRoot.transform.SetParent(transform);

        Build(AudioCategory.Footstep, footstepPoolSize);
        Build(AudioCategory.Movement, movementPoolSize);
        Build(AudioCategory.Weapon, weaponPoolSize);
        Build(AudioCategory.HeavyAttack, heavyAttackPoolSize);
        Build(AudioCategory.Skill, skillPoolSize);
        Build(AudioCategory.Projectile, projectilePoolSize);
        Build(AudioCategory.Impact, impactPoolSize);
        Build(AudioCategory.Hit, hitPoolSize);
        Build(AudioCategory.Death, deathPoolSize);
        Build(AudioCategory.Item, itemPoolSize);
        Build(AudioCategory.Mechanism, mechanismPoolSize);
        Build(AudioCategory.UI, uiPoolSize);
        Build(AudioCategory.BGM, bgmPoolSize);
        Build(AudioCategory.Ambient, ambientPoolSize);
        Build(AudioCategory.Voice, voicePoolSize);
    }

    private void Build(AudioCategory cat, int size)
    {
        var q = new Queue<AudioSource>();
        for (int i = 0; i < size; i++)
        {
            var go = new GameObject($"Audio_{cat}_{i}");
            go.transform.SetParent(poolRoot.transform);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            q.Enqueue(src);
        }
        pools[cat] = q;
        active[cat] = new HashSet<AudioSource>();
    }

    public void PlayAt(AudioEventData_SO data, Vector3 position)
    {
        if (data == null || data.clips == null || data.clips.Count == 0) return;

        AudioClip clip = data.GetRandomClip();
        if (clip == null) return;

        AudioCategory cat = data.category;
        AudioSource src = RentSource(cat, data.priority);
        if (src == null) return;

        src.transform.position = position;
        src.clip = clip;
        src.priority = data.priority;
        src.pitch = data.randomizePitch ? Random.Range(data.pitchMin, data.pitchMax) : 1f;

        // 空间音频
        if (data.useSpatialAudio)
        {
            src.spatialBlend = 0f;
            src.panStereo = Get2DPan(position);
            src.volume = data.volume * Get2DDistanceMultiplier(position, data.minDistance, data.maxDistance);
        }
        else
        {
            src.spatialBlend = 0f;
            src.panStereo = 0f;
            src.volume = data.volume;
        }

        src.Play();

        float dur = clip.length / src.pitch + 0.1f;
        StartCoroutine(RecycleAfter(src, cat, dur));
    }

    public void PlayAtPoint(AudioClip clip, Vector3 position, float volume = 1f, AudioCategory cat = AudioCategory.Skill)
    {
        if (clip == null) return;

        AudioSource src = RentSource(cat, 128);
        if (src == null) return;

        src.transform.position = position;
        src.clip = clip;
        src.priority = 128;
        src.pitch = 1f;

        //2D 空间音频
        src.spatialBlend = 0f;
        src.panStereo = Get2DPan(position);
        src.volume = volume * Get2DDistanceMultiplier(position, 1f, 30f);

        src.Play();

        float dur = clip.length + 0.1f;
        StartCoroutine(RecycleAfter(src, cat, dur));
    }

    // 基于 X 轴偏移计算左右声道分配
    private float Get2DPan(Vector3 position)
    {
        Camera cam = Camera.main;
        if (cam == null) return 0f;

        float dx = position.x - cam.transform.position.x;
        float halfWidth = cam.orthographicSize * cam.aspect;
        return Mathf.Clamp(dx / halfWidth, -1f, 1f);
    }

    // 基于 2D 距离计算音量衰减
    private float Get2DDistanceMultiplier(Vector3 position, float minDist, float maxDist)
    {
        Camera cam = Camera.main;
        if (cam == null) return 1f;

        Vector2 cam2D = cam.transform.position;
        Vector2 pos2D = position;
        float dist = Vector2.Distance(cam2D, pos2D);

        if (dist <= minDist) return 1f;
        if (dist >= maxDist) return 0f;
        return 1f - (dist - minDist) / (maxDist - minDist);
    }

    private AudioSource RentSource(AudioCategory cat, int wantPriority)
    {
        if (!pools.ContainsKey(cat)) return null;
        var q = pools[cat];
        var set = active[cat];

        while (q.Count > 0)
        {
            var src = q.Dequeue();
            if (src != null)
            {
                set.Add(src);
                return src;
            }
        }

        AudioSource steal = null;
        int worst = wantPriority;
        foreach (var src in set)
        {
            if (src.priority > worst)
            {
                worst = src.priority;
                steal = src;
            }
        }

        if (steal != null)
        {
            steal.Stop();
            return steal;
        }

        return null;
    }

    private IEnumerator RecycleAfter(AudioSource src, AudioCategory cat, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (src == null) yield break;

        src.Stop();
        src.clip = null;
        active[cat].Remove(src);
        pools[cat].Enqueue(src);
    }

    public void StopCategory(AudioCategory cat)
    {
        if (!active.ContainsKey(cat)) return;
        foreach (var src in active[cat])
            if (src != null) src.Stop();
    }

    public void StopAll()
    {
        foreach (var kv in active)
            foreach (var src in kv.Value)
                if (src != null) src.Stop();
    }
}

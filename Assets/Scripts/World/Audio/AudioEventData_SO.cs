using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/AudioEvent", fileName = "NewAudioEvent")]
public class AudioEventData_SO : ScriptableObject
{
    [Header("分类")]
    public AudioCategory category = AudioCategory.Weapon;

    [Header("音频片段（随机选一个播）")]
    public List<AudioClip> clips = new List<AudioClip>();

    [Header("音量")]
    [Range(0f, 1f)]
    [Tooltip("Unity限制0~1，音频源太小需在Audacity里放大")]
    public float volume = 1f;

    [Header("空间音频")]
    [Tooltip("勾上才有左右声道和距离衰减，UI/BGM不勾")]
    public bool useSpatialAudio = true;

    [Tooltip("在这个距离内音量最大")]
    public float minDistance = 1f;

    [Tooltip("超过这个距离就听不到")]
    public float maxDistance = 30f;

    [Header("音调随机化")]
    public bool randomizePitch = true;
    public float pitchMin = 0.95f;
    public float pitchMax = 1.05f;

    [Header("优先级(0最高 256最低)")]
    [Range(0, 256)]
    public int priority = 128;

    public AudioClip GetRandomClip()
    {
        if (clips == null || clips.Count == 0) return null;
        return clips[Random.Range(0, clips.Count)];
    }
}

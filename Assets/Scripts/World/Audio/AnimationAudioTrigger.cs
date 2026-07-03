using UnityEngine;

public class AnimationAudioTrigger : MonoBehaviour
{
    [Tooltip("按动画事件传的 int 索引匹配，0 对应第 1 个")]
    public AudioEventData_SO[] sounds;

    public void PlaySound(int index)
    {
        if (index < 0 || index >= sounds.Length) return;
        if (sounds[index] == null) return;
        AudioManager.Instance.PlayAt(sounds[index], transform.position);
    }
}

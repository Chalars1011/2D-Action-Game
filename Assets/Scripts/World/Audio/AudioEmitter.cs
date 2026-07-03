using UnityEngine;

public class AudioEmitter : MonoBehaviour
{
    public void PlayAtSource(AudioEventData_SO data)
    {
        if (data != null)
            AudioManager.Instance.PlayAt(data, transform.position);
    }
}

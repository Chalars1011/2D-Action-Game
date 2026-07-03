using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName ="FootAudioData",fileName ="AudioData")]
public class FootAudioData_SO : ScriptableObject
{
    public List<FootsAudio> footsAudios = new List<FootsAudio>();



    [System.Serializable]
    public class FootsAudio
    {
        public string Tag;
        public List<AudioClip> footClips = new List<AudioClip>();
      //  public float Delay;
    }
}

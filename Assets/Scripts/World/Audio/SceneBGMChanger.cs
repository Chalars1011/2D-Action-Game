using UnityEngine;

public class SceneBGMChanger : MonoBehaviour
{
    [Header("本场景的BGM")]
    public AudioEventData_SO sceneBGM;

    void Start()
    {
        if (sceneBGM != null)
            BGMPlayer.Play(sceneBGM);
    }
}

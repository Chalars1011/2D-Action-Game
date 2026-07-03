using GameArchitecture.Core;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameEndController : MonoBehaviour
{
    [Header("淡入的图片")]
    public Image endImage;

    [Header("淡入的文字")]
    public TextMeshProUGUI endText;

    [Header("淡入耗时（秒）")]
    public float fadeInDuration = 2f;

    [Header("结束BGM")]
    public AudioEventData_SO endingBGM;

    private bool ended;

    public void TriggerEnd()
    {
        if (ended) return;
        ended = true;
        StartCoroutine(EndSequence());
    }

    private IEnumerator EndSequence()
    {
        // 1. 禁用玩家输入就够了，不改标志位防残留
        PlayerController pc = Blackboard.PlayerTransform?.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.canRun = false;
            pc.enabled = false;
        }

        // 2. 关掉战斗声音
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopCategory(AudioCategory.Ambient);
            AudioManager.Instance.StopCategory(AudioCategory.Weapon);
            AudioManager.Instance.StopCategory(AudioCategory.Skill);
            AudioManager.Instance.StopCategory(AudioCategory.Hit);
            AudioManager.Instance.StopCategory(AudioCategory.Footstep);
        }

        // 3. 播放结束BGM
        if (endingBGM != null)
            BGMPlayer.Play(endingBGM);

        // 4. 关残血效果
        HealthVignetteController vignette = FindObjectOfType<HealthVignetteController>();
        if (vignette != null)
            vignette.enabled = false;

        // 5. 初始透明
        if (endImage != null)
        {
            endImage.gameObject.SetActive(true);
            endImage.color = new Color(endImage.color.r, endImage.color.g, endImage.color.b, 0f);
        }
        if (endText != null)
        {
            endText.gameObject.SetActive(true);
            endText.color = new Color(endText.color.r, endText.color.g, endText.color.b, 0f);
        }

        // 6. 淡入
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float a = elapsed / fadeInDuration;

            if (endImage != null)
                endImage.color = new Color(endImage.color.r, endImage.color.g, endImage.color.b, a);
            if (endText != null)
                endText.color = new Color(endText.color.r, endText.color.g, endText.color.b, a);

            yield return null;
        }
    }
}

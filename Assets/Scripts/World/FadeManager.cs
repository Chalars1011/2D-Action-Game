using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// 淡入淡出管理器 - 单例模式，用于处理场景切换和特殊效果的屏幕过渡
/// </summary>
public class FadeManager : MonoBehaviour
{
    /// <summary>
    /// 单例实例引用
    /// </summary>
    private static FadeManager _instance;

    /// <summary>
    /// 淡入淡出画布
    /// </summary>
    private static Canvas _fadeCanvas;

    /// <summary>
    /// 淡入淡出图片
    /// </summary>
    private static Image _fadeImage;

    /// <summary>
    /// 是否正在执行淡入淡出效果
    /// </summary>
    private static bool _isFading = false;

    [Header("淡入淡出设置")]
    [Tooltip("淡入淡出持续时间")]
    [SerializeField] private float fadeDuration = 0.5f;

    [Tooltip("默认淡入淡出颜色（通常为黑色）")]
    [SerializeField] private Color fadeColor = Color.black;

    [Tooltip("死亡时的红色效果（60%透明度）")]
    [SerializeField] private Color deathRedColor = new Color(1, 0, 0, 0.6f);

    /// <summary>
    /// 初始化单例和UI组件
    /// </summary>
    private void Awake()
    {
        // 确保只有一个实例存在
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFadeUI();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 清理事件注册
    /// </summary>
    private void OnDestroy()
    {
        if (_instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    /// <summary>
    /// 初始化淡入淡出UI组件
    /// </summary>
    private void InitializeFadeUI()
    {
        // 创建画布
        GameObject canvasObject = new GameObject("FadeCanvas");
        _fadeCanvas = canvasObject.AddComponent<Canvas>();
        _fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _fadeCanvas.sortingOrder = 1000;
        canvasObject.AddComponent<CanvasScaler>();
        DontDestroyOnLoad(canvasObject);

        // 创建图片
        GameObject imageObject = new GameObject("FadeImage");
        imageObject.transform.SetParent(canvasObject.transform, false);

        RectTransform rectTransform = imageObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;

        _fadeImage = imageObject.AddComponent<Image>();
        _fadeImage.color = fadeColor;
        SetAlpha(0);
    }

    /// <summary>
    /// 设置图片透明度
    /// </summary>
    /// <param name="alpha">透明度值（0-1）</param>
    private static void SetAlpha(float alpha)
    {
        if (_fadeImage != null)
        {
            Color color = _fadeImage.color;
            color.a = alpha;
            _fadeImage.color = color;
        }
    }

    /// <summary>
    /// 设置图片颜色
    /// </summary>
    /// <param name="color">目标颜色</param>
    private static void SetColor(Color color)
    {
        if (_fadeImage != null)
        {
            _fadeImage.color = color;
        }
    }

    /// <summary>
    /// 场景加载完成回调
    /// </summary>
    /// <param name="scene">加载的场景</param>
    /// <param name="mode">加载模式</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 如果正在淡入效果中（画面未完全透明），继续执行淡出
        if (_isFading && _fadeImage.color.a > 0)
        {
            _instance.StartCoroutine(FadeOutRoutine());
        }
    }

    /// <summary>
    /// 淡入效果（从透明到不透明）
    /// </summary>
    /// <param name="onComplete">完成后的回调</param>
    public static void FadeIn(System.Action onComplete = null)
    {
        if (_isFading || _instance == null) return;
        _instance.StartCoroutine(FadeInRoutine(onComplete));
    }

    /// <summary>
    /// 淡出效果（从不透明到透明）
    /// </summary>
    /// <param name="onComplete">完成后的回调</param>
    public static void FadeOut(System.Action onComplete = null)
    {
        if (_isFading || _instance == null) return;
        _instance.StartCoroutine(FadeOutRoutine(onComplete));
    }

    /// <summary>
    /// 死亡过渡效果（红->黑->消散）
    /// </summary>
    /// <param name="onComplete">完成后的回调</param>
    public static void DeathFade(System.Action onComplete = null)
    {
        if (_isFading || _instance == null) return;
        _instance.StartCoroutine(DeathFadeRoutine(onComplete));
    }

    /// <summary>
    /// 淡入协程
    /// </summary>
    /// <param name="onComplete">完成后的回调</param>
    private static IEnumerator FadeInRoutine(System.Action onComplete = null)
    {
        _isFading = true;
        float elapsed = 0;

        while (elapsed < _instance.fadeDuration)
        {
            float alpha = Mathf.Lerp(0, 1, elapsed / _instance.fadeDuration);
            SetAlpha(alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        SetAlpha(1);
        _isFading = false;
        onComplete?.Invoke();
    }

    /// <summary>
    /// 淡出协程
    /// </summary>
    /// <param name="onComplete">完成后的回调</param>
    private static IEnumerator FadeOutRoutine(System.Action onComplete = null)
    {
        _isFading = true;
        float elapsed = 0;

        while (elapsed < _instance.fadeDuration)
        {
            float alpha = Mathf.Lerp(1, 0, elapsed / _instance.fadeDuration);
            SetAlpha(alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        SetAlpha(0);
        _isFading = false;
        onComplete?.Invoke();
    }

    /// <summary>
    /// 死亡过渡协程：红->黑->消散
    /// 阶段1：屏幕变红（60%透明度）
    /// 阶段2：红色变黑色（变黑后立即执行回调，传送玩家）
    /// 阶段3：黑色消散
    /// </summary>
    /// <param name="onComplete">屏幕变黑时的回调（传送玩家）</param>
    private static IEnumerator DeathFadeRoutine(System.Action onComplete = null)
    {
        _isFading = true;
        float elapsed = 0;

        // 第一阶段：屏幕变红（60%透明度）
        while (elapsed < _instance.fadeDuration)
        {
            float alpha = Mathf.Lerp(0, _instance.deathRedColor.a, elapsed / _instance.fadeDuration);
            SetColor(new Color(_instance.deathRedColor.r, _instance.deathRedColor.g, _instance.deathRedColor.b, alpha));
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 等待一小段时间，让玩家看到红色效果
        yield return new WaitForSeconds(0.3f);

        // 第二阶段：红色变黑色
        elapsed = 0;
        Color currentColor = _fadeImage.color;
        while (elapsed < _instance.fadeDuration)
        {
            float t = elapsed / _instance.fadeDuration;
            Color newColor = Color.Lerp(currentColor, Color.black, t);
            SetColor(newColor);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 屏幕变黑后立即执行回调（传送玩家到复活点）
        onComplete?.Invoke();

        // 等待一小段时间
        yield return new WaitForSeconds(0.2f);

        // 第三阶段：黑色消散（时间稍长一点，更平滑）
        elapsed = 0;
        while (elapsed < _instance.fadeDuration * 1.5f)
        {
            float alpha = Mathf.Lerp(1, 0, elapsed / (_instance.fadeDuration * 1.5f));
            SetAlpha(alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        SetAlpha(0);
        _isFading = false;
    }
}

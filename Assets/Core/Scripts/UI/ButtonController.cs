using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public class ButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [TitleGroup("References")]
    [SerializeField] private GameObject targetLeftChevron;
    [SerializeField] private GameObject targetRightChevron;
    [SerializeField] private GameObject targetLight;
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text targetText;

    [TitleGroup("Auto Resize")]
    [SerializeField] private bool autoResizeWidthByText = true;
    [SerializeField, ShowIf(nameof(autoResizeWidthByText))] private float widthPerTextCharacter = 24f;

    [TitleGroup("Chevron")]
    [SerializeField] private float chevronDistance = -30f;

    [TitleGroup("Interaction")]
    [SerializeField] private float clickCooldownTime = 0.1f;

    [TitleGroup("Animation")]
    [SerializeField] private float animationDuration = 0.1f;

    private readonly Vector3 defaultScale = new(0.03f, 0.03f, 1f);
    private readonly Vector3 hoverScale = new(0.3f, 0.3f, 1f);
    private readonly Vector3 clickScale = new(0.25f, 0.25f, 1f);
    private readonly Vector3 lightStartScale = new(0.05f, 0.01f, 1f);
    private readonly Vector3 lightVerticalScale = new(0.05f, 1f, 1f);

    private RectTransform rootRectTransform;
    private RectTransform leftChevronRectTransform;
    private RectTransform rightChevronRectTransform;
    private RectTransform lightRectTransform;
    private Graphic[] leftChevronGraphics;
    private Graphic[] rightChevronGraphics;
    private Graphic[] lightGraphics;
    private Sequence clickEffectSequence;
    private float lastClickTime = -Mathf.Infinity;
    private bool isInitialized;
    private bool isRefreshingLayout;

    private void Awake()
    {
        CacheReferences();
        RefreshAdaptiveLayout();

        if (!Application.IsPlaying(gameObject))
            return;

        InitializeEffects();
    }

    private void OnEnable()
    {
        CacheReferences();
        RefreshAdaptiveLayout();

        if (!Application.IsPlaying(gameObject))
            return;

        SceneManager.activeSceneChanged += OnActiveSceneChanged;

        if (isInitialized)
            RemoveEffect();
    }

    private void Start()
    {
        if (!Application.IsPlaying(gameObject))
            return;

        if (!isInitialized)
            InitializeEffects();

        button.onClick.RemoveListener(OnClickButton);
        button.onClick.AddListener(OnClickButton);
    }

    private void Update()
    {
        RefreshAdaptiveLayout();

        if (!Application.IsPlaying(gameObject))
            return;

        bool isCooldownOver = lastClickTime + clickCooldownTime <= Time.time;

        if (isCooldownOver != button.interactable)
            button.interactable = isCooldownOver;
    }

    private void OnValidate()
    {
        CacheReferences();
        RefreshAdaptiveLayout();
    }

    private void OnRectTransformDimensionsChange()
    {
        RefreshAdaptiveLayout();
    }

    public void OnPointerEnter(PointerEventData eventData) => OnMouseEnter();

    public void OnPointerExit(PointerEventData eventData) => OnMouseExit();

    public void OnPointerDown(PointerEventData eventData) => OnPointerDown();

    public void OnPointerUp(PointerEventData eventData) => OnPointerUp();

    public void OnMouseEnter()
    {
        AudioManager.Instance.PlayOneShotSFX("OnMouse", gameObject);
        PlayChevronEffect(hoverScale, 1f, Ease.OutQuad);
    }

    public void OnMouseExit()
    {
        PlayChevronEffect(defaultScale, 0f, Ease.InQuad);
    }

    public void OnPointerDown()
    {
        KillChevronScaleTweens();
        SetChevronScale(clickScale);
    }

    public void OnPointerUp()
    {
        AudioManager.Instance.PlayOneShotSFX("OnClick", gameObject);

        KillChevronScaleTweens();
        SetChevronScale(hoverScale);
        SetChevronAlpha(1f);

        RestartClickEffect();
    }

    public void RemoveEffect()
    {
        if (!isInitialized)
            return;

        CancelAllEffects(true);
    }

    private void CacheReferences()
    {
        rootRectTransform = transform as RectTransform;
        leftChevronRectTransform = targetLeftChevron.transform as RectTransform;
        rightChevronRectTransform = targetRightChevron.transform as RectTransform;
        lightRectTransform = targetLight.transform as RectTransform;
    }

    private void RefreshAdaptiveLayout()
    {
        if (isRefreshingLayout)
            return;

        isRefreshingLayout = true;

        if (autoResizeWidthByText)
            ResizeRootWidthByText();

        SyncLightRectTransform();
        SyncChevronPosition(leftChevronRectTransform, -1f);
        SyncChevronPosition(rightChevronRectTransform, 1f);

        isRefreshingLayout = false;
    }

    private void ResizeRootWidthByText()
    {
        int characterCount = GetTextCharacterCount();
        float width = Mathf.Max(0f, characterCount * widthPerTextCharacter);
        rootRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }

    private int GetTextCharacterCount()
    {
        targetText.ForceMeshUpdate(true, true);
        return targetText.textInfo.characterCount;
    }

    private void SyncLightRectTransform()
    {
        Vector2 rootSize = rootRectTransform.rect.size;
        lightRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rootSize.x);
        lightRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rootSize.y);
    }

    private void SyncChevronPosition(RectTransform chevronRectTransform, float direction)
    {
        Vector2 position = chevronRectTransform.anchoredPosition;
        position.x = direction * (rootRectTransform.rect.width * 0.5f + chevronDistance);
        chevronRectTransform.anchoredPosition = position;
    }

    private void InitializeEffects()
    {
        InitializeChevron(targetLeftChevron, out leftChevronGraphics);
        InitializeChevron(targetRightChevron, out rightChevronGraphics);

        lightGraphics = targetLight.GetComponentsInChildren<Graphic>(true);
        targetLight.transform.localScale = defaultScale;
        SetAlpha(lightGraphics, 0f);

        isInitialized = true;
    }

    private void InitializeChevron(GameObject chevron, out Graphic[] graphics)
    {
        chevron.transform.localScale = defaultScale;
        graphics = chevron.GetComponentsInChildren<Graphic>(true);
        SetAlpha(graphics, 0f);
    }

    private void PlayChevronEffect(Vector3 scale, float alpha, Ease ease)
    {
        CancelChevronTweens();

        PlayChevronEffect(targetLeftChevron, leftChevronGraphics, scale, alpha, ease);
        PlayChevronEffect(targetRightChevron, rightChevronGraphics, scale, alpha, ease);
    }

    private void PlayChevronEffect(GameObject chevron, Graphic[] graphics, Vector3 scale, float alpha, Ease ease)
    {
        chevron.transform
            .DOScale(scale, animationDuration)
            .SetEase(ease)
            .SetLink(chevron, LinkBehaviour.KillOnDestroy);

        FadeGraphics(graphics, alpha, animationDuration);
    }

    private void RestartClickEffect()
    {
        CancelClickEffect(true);

        SyncLightRectTransform();
        targetLight.transform.localScale = lightStartScale;
        SetAlpha(lightGraphics, 1f);

        clickEffectSequence = DOTween.Sequence()
            .SetTarget(targetLight)
            .SetLink(targetLight, LinkBehaviour.KillOnDestroy);

        clickEffectSequence
            .Append(targetLight.transform.DOScale(lightVerticalScale, animationDuration).SetEase(Ease.OutQuad))
            .Append(targetLight.transform.DOScale(Vector3.one, animationDuration).SetEase(Ease.OutQuad))
            .AppendInterval(animationDuration);

        AppendFade(clickEffectSequence, lightGraphics, 0f, animationDuration);

        clickEffectSequence.OnComplete(CompleteClickEffect);
        clickEffectSequence.OnKill(() => clickEffectSequence = null);
    }

    private void CompleteClickEffect()
    {
        ResetLight();
    }

    private void CancelAllEffects(bool reset)
    {
        CancelClickEffect(reset);
        CancelChevronTweens();

        if (!reset)
            return;

        ResetChevrons();
    }

    private void CancelClickEffect(bool reset)
    {
        if (clickEffectSequence != null && clickEffectSequence.IsActive())
            clickEffectSequence.Kill(false);

        clickEffectSequence = null;

        if (targetLight != null)
            targetLight.transform.DOKill(false);

        KillGraphics(lightGraphics);

        if (!reset)
            return;

        ResetLight();
    }

    private void CancelChevronTweens()
    {
        KillChevronScaleTweens();
        KillGraphics(leftChevronGraphics);
        KillGraphics(rightChevronGraphics);
    }

    private void KillChevronScaleTweens()
    {
        targetLeftChevron.transform.DOKill(false);
        targetRightChevron.transform.DOKill(false);
    }

    private void ResetChevrons()
    {
        ResetChevron(targetLeftChevron, leftChevronGraphics);
        ResetChevron(targetRightChevron, rightChevronGraphics);
    }

    private void ResetChevron(GameObject chevron, Graphic[] graphics)
    {
        chevron.transform.localScale = defaultScale;
        SetAlpha(graphics, 0f);
    }

    private void ResetLight()
    {
        SyncLightRectTransform();
        targetLight.transform.localScale = defaultScale;
        SetAlpha(lightGraphics, 0f);
    }

    private void SetChevronScale(Vector3 scale)
    {
        targetLeftChevron.transform.localScale = scale;
        targetRightChevron.transform.localScale = scale;
    }

    private void SetChevronAlpha(float alpha)
    {
        SetAlpha(leftChevronGraphics, alpha);
        SetAlpha(rightChevronGraphics, alpha);
    }

    private void FadeGraphics(Graphic[] graphics, float alpha, float duration)
    {
        for (int i = 0; i < graphics.Length; i++)
        {
            graphics[i]
                .DOFade(alpha, duration)
                .SetLink(graphics[i].gameObject, LinkBehaviour.KillOnDestroy);
        }
    }

    private void AppendFade(Sequence sequence, Graphic[] graphics, float alpha, float duration)
    {
        bool isFirstTween = true;

        for (int i = 0; i < graphics.Length; i++)
        {
            Tween fadeTween = graphics[i]
                .DOFade(alpha, duration)
                .SetLink(graphics[i].gameObject, LinkBehaviour.KillOnDestroy);

            if (isFirstTween)
            {
                sequence.Append(fadeTween);
                isFirstTween = false;
                continue;
            }

            sequence.Join(fadeTween);
        }
    }

    private void KillGraphics(Graphic[] graphics)
    {
        for (int i = 0; i < graphics.Length; i++)
        {
            graphics[i].DOKill(false);
        }
    }

    private void SetAlpha(Graphic[] graphics, float alpha)
    {
        for (int i = 0; i < graphics.Length; i++)
        {
            Color color = graphics[i].color;
            color.a = alpha;
            graphics[i].color = color;
        }
    }

    private void OnClickButton()
    {
        lastClickTime = Time.time;
    }

    private void OnActiveSceneChanged(Scene currentScene, Scene nextScene)
    {
        if (isInitialized)
            CancelAllEffects(true);
    }

    private void OnDisable()
    {
        if (!Application.IsPlaying(gameObject))
            return;

        SceneManager.activeSceneChanged -= OnActiveSceneChanged;

        if (isInitialized)
            CancelAllEffects(true);
    }

    private void OnDestroy()
    {
        if (!Application.IsPlaying(gameObject))
            return;

        if (button != null)
            button.onClick.RemoveListener(OnClickButton);

        if (isInitialized)
            CancelAllEffects(false);
    }
}
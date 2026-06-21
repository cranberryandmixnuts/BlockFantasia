using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public GameObject targetLeftChevron;
    public GameObject targetRightChevron;
    public GameObject targetLight;

    [SerializeField] private Button button;
    [SerializeField] private float clickCooldownTime = 0.1f;
    [SerializeField] private float animationDuration = 0.1f;

    private readonly Vector3 defaultScale = new(0.03f, 0.03f, 1f);
    private readonly Vector3 hoverScale = new(0.3f, 0.3f, 1f);
    private readonly Vector3 clickScale = new(0.25f, 0.25f, 1f);
    private readonly Vector3 lightStartScale = new(0.05f, 0.01f, 1f);
    private readonly Vector3 lightVerticalScale = new(0.05f, 1f, 1f);

    private Graphic[] leftChevronGraphics;
    private Graphic[] rightChevronGraphics;
    private Graphic[] lightGraphics;
    private Sequence clickEffectSequence;
    private float lastClickTime = -Mathf.Infinity;
    private bool isInitialized;

    private void Awake()
    {
        InitializeEffects();
    }

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;

        if (isInitialized)
            RemoveEffect();
    }

    private void Start()
    {
        button.onClick.AddListener(OnClickButton);
    }

    private void Update()
    {
        bool isCooldownOver = lastClickTime + clickCooldownTime <= Time.time;

        if (isCooldownOver != button.interactable)
            button.interactable = isCooldownOver;
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

        if (targetLight == null)
            return;

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
        if (targetLight == null)
            return;

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
            if (graphics[i] == null)
                continue;

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
            if (graphics[i] == null)
                continue;

            Tween fadeTween = graphics[i].DOFade(alpha, duration);

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
        if (graphics == null)
            return;

        for (int i = 0; i < graphics.Length; i++)
        {
            if (graphics[i] == null)
                continue;

            graphics[i].DOKill(false);
        }
    }

    private void SetAlpha(Graphic[] graphics, float alpha)
    {
        if (graphics == null)
            return;

        for (int i = 0; i < graphics.Length; i++)
        {
            if (graphics[i] == null)
                continue;

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
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;

        if (isInitialized)
            CancelAllEffects(true);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OnClickButton);

        if (isInitialized)
            CancelAllEffects(false);
    }
}
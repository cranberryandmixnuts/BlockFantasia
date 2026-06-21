using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public GameObject[] targetChevrons;
    public GameObject targetLight;

    [SerializeField] private Button button;
    [SerializeField] private float clickCooldownTime = 0.1f;
    [SerializeField] private float animationDuration = 0.1f;

    private Vector2 DefaultScale = new(0.03f, 0.03f);
    private Vector2 HoverScale = new(0.3f, 0.3f);
    private Vector2 clickScale = new(0.23f, 0.23f);

    private CanvasGroup[] chevronCanvasGroups;
    private CanvasGroup lightCanvasGroup;

    private float lastClickTime = -Mathf.Infinity;

    private void Start()
    {
        chevronCanvasGroups = new CanvasGroup[targetChevrons.Length];

        for (int i = 0; i < targetChevrons.Length; i++)
        {
            targetChevrons[i].transform.localScale = HoverScale;

            chevronCanvasGroups[i] = targetChevrons[i].GetComponent<CanvasGroup>();
        }

        StopAllCoroutines();

        for (int i = 0; i < targetChevrons.Length; i++)
        {
            targetChevrons[i].transform.DOKill();
            chevronCanvasGroups[i].DOKill();

            targetChevrons[i].transform.localScale = DefaultScale;
            chevronCanvasGroups[i].alpha = 0f;

        }

        lightCanvasGroup = targetLight.GetComponent<CanvasGroup>();
        lightCanvasGroup.alpha = 0f;


        button.onClick.AddListener(OnClickButton);

        RemoveEffect();
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
        StopAllCoroutines();

        AudioManager.Instance.PlayOneShotSFX("OnMouse", gameObject);

        for (int i = 0; i < targetChevrons.Length; i++)
        {
            if (targetChevrons[i] != null)
            {
                targetChevrons[i].transform.DOKill();
                chevronCanvasGroups[i].DOKill();

                targetChevrons[i].transform.DOScale(HoverScale, animationDuration).SetEase(Ease.OutQuad);
                chevronCanvasGroups[i].DOFade(1f, animationDuration);
            }
        }
    }

    public void OnMouseExit()
    {
        StopAllCoroutines();

        for (int i = 0; i < targetChevrons.Length; i++)
        {
            if (targetChevrons[i] != null)
            {
                targetChevrons[i].transform.DOKill();
                chevronCanvasGroups[i].DOKill();

                targetChevrons[i].transform.DOScale(DefaultScale, animationDuration).SetEase(Ease.InQuad);
                chevronCanvasGroups[i].DOFade(0f, animationDuration);
            }
        }
    }

    public void OnPointerDown()
    {
        for (int i = 0; i < targetChevrons.Length; i++)
        {
            targetChevrons[i].transform.localScale = clickScale;
        }
    }

    public void OnPointerUp()
    {
        AudioManager.Instance.PlayOneShotSFX("OnClick", gameObject);

        for (int i = 0; i < targetChevrons.Length; i++)
        {
            targetChevrons[i].transform.localScale = HoverScale;
        }

        targetLight.transform.localScale = new Vector2(0.05f, 0.01f);
        lightCanvasGroup.alpha = 1f;

        Sequence sequence = DOTween.Sequence();

        sequence.Append(targetLight.transform.DOScale(new Vector2(0.05f, 1f), animationDuration).SetEase(Ease.OutQuad));
        sequence.Append(targetLight.transform.DOScale(new Vector2(1f, 1f), animationDuration).SetEase(Ease.OutQuad));

        sequence.AppendInterval(animationDuration)
            .Append(lightCanvasGroup.DOFade(0f, animationDuration))
            .OnComplete(() => targetLight.transform.localScale = DefaultScale);

    }

    public void RemoveEffect()
    {
        for (int i = 0; i < targetChevrons.Length; i++)
        {
            targetChevrons[i].transform.DOKill();
            chevronCanvasGroups[i].DOKill();

            targetChevrons[i].transform.localScale = DefaultScale;
            chevronCanvasGroups[i].alpha = 0f;

        }
    }

    private void OnClickButton()
    {
        lastClickTime = Time.time;
    }

    private void OnDisable() => RemoveEffect();
}
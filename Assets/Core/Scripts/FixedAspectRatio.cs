using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Camera))]
public sealed class FixedAspectRatio : MonoBehaviour
{
    [SerializeField]
    private Camera cameraComponent;

    [SerializeField, MinValue(1)]
    private int targetAspectWidth = 16;

    [SerializeField, MinValue(1)]
    private int targetAspectHeight = 9;

    [SerializeField, MinValue(0.1f)]
    private float checkInterval = 1f;

    private int lastScreenWidth = -1;
    private int lastScreenHeight = -1;

    private void Start()
    {
        ApplyAspectIfNeeded(true);
        InvokeRepeating(nameof(CheckResolutionChanged), checkInterval, checkInterval);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
            ApplyAspectIfNeeded(true);
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(CheckResolutionChanged));
        cameraComponent.rect = new Rect(0f, 0f, 1f, 1f);
    }

    private void CheckResolutionChanged() => ApplyAspectIfNeeded();

    private void ApplyAspectIfNeeded(bool force = false)
    {
        if (!force && lastScreenWidth == Screen.width && lastScreenHeight == Screen.height)
            return;

        ApplyAspect();
    }

    private void ApplyAspect()
    {
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        float targetAspect = (float)targetAspectWidth / targetAspectHeight;
        float currentAspect = (float)Screen.width / Screen.height;

        if (currentAspect > targetAspect)
        {
            float width = targetAspect / currentAspect;
            float x = (1f - width) * 0.5f;
            cameraComponent.rect = new Rect(x, 0f, width, 1f);
            return;
        }

        if (currentAspect < targetAspect)
        {
            float height = currentAspect / targetAspect;
            float y = (1f - height) * 0.5f;
            cameraComponent.rect = new Rect(0f, y, 1f, height);
            return;
        }

        cameraComponent.rect = new Rect(0f, 0f, 1f, 1f);
    }
}
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class CursorManager : Singleton<CursorManager, GlobalScope>
{
    [SerializeField] private Texture2D normalCursor;
    [SerializeField] private Texture2D pressedCursor;
    [SerializeField] private Vector2 hotSpot;
    [SerializeField] private CursorMode cursorMode = CursorMode.Auto;

    private bool isPressed;

    private void OnEnable() => Apply(normalCursor);

    private void Update()
    {
        bool nextPressed = Mouse.current.leftButton.isPressed;

        if (isPressed == nextPressed)
            return;

        isPressed = nextPressed;
        Apply(isPressed ? pressedCursor : normalCursor);
    }

    private void OnDisable() => Cursor.SetCursor(null, Vector2.zero, cursorMode);

    private void Apply(Texture2D texture) => Cursor.SetCursor(texture, hotSpot, cursorMode);
}
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class ClickEffectManager : Singleton<ClickEffectManager, GlobalScope>
{
    [SerializeField, Required] private ParticleSystem minoParticleSystem;
    [SerializeField]
    private Color[] possibleColors =
    {
        Color.red,
        Color.yellow,
        Color.green,
        Color.cyan,
        Color.magenta
    };

    [SerializeField, MinValue(1)] private int minCount = 5;
    [SerializeField, MinValue(1)] private int maxCount = 6;
    [SerializeField] private float UpPower = 3.3f;
    [SerializeField] private float spreadAngle = 30f;
    [SerializeField] private float Size = 0.5f;
    [SerializeField] private float minLifetime = 0.6f;
    [SerializeField] private float maxLifetime = 0.9f;

    private bool isEffectEnabled = true;

    public bool IsEffectEnabled
    {
        get => isEffectEnabled;
        set => isEffectEnabled = value;
    }

    private void Update()
    {
        if (!isEffectEnabled)
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorldPosition.z = 0f;

        Play(mouseWorldPosition);
    }

    public void Play(Vector3 position)
    {
        int count = Random.Range(minCount, maxCount + 1);

        for (int i = 0; i < count; i++)
            Emit(position);
    }

    private void Emit(Vector3 position)
    {
        float angle = Random.Range(-spreadAngle, spreadAngle);
        Vector3 direction = Quaternion.Euler(0f, 0f, angle) * Vector3.up;

        ParticleSystem.EmitParams emitParams = new()
        {
            position = position,
            velocity = direction * UpPower,
            startColor = possibleColors[Random.Range(0, possibleColors.Length)],
            startSize = Size,
            startLifetime = Random.Range(minLifetime, maxLifetime),
            rotation3D = new Vector3(0f, 0f, Random.Range(0f, 360f))
        };

        minoParticleSystem.Emit(emitParams, 1);
    }
}
using UnityEngine;
using Sirenix.OdinInspector;

public sealed class SceneLoadButton : MonoBehaviour
{
    [SerializeField]
    private bool reloadCurrentScene = false;

    [HideIf(nameof(reloadCurrentScene))]
    [ValidateInput(nameof(IsValidScene), "SceneType이 None이면 로드할 수 없습니다!"), SerializeField]
    private SceneType scene = SceneType.None;

    private bool Loaded = false;

    public void Load()
    {
        if (Loaded) return;

        SceneType targetScene = reloadCurrentScene
            ? SceneLoader.Instance.CurrentSceneType
            : scene;

        if (targetScene == SceneType.None) return;

        Loaded = true;
        SceneLoader.Instance.LoadScene(targetScene);
    }

    private bool IsValidScene(SceneType value) => reloadCurrentScene || value != SceneType.None;
}
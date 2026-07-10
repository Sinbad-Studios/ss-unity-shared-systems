using UnityEngine;
using TMPro;

namespace SinbadStudios.SharedSystems.Runtime
{
    public class VersionDisplay : MonoBehaviour
    {
        [SerializeField] private string prefix = "v";

        private void Awake()
        {
            if (TryGetComponent<TMP_Text>(out var tmp) == false)
            {
                Debug.LogError("VersionDisplay requires a TMP_Text component on the same GameObject.");
                return;
            }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            tmp.text = $"DEV {prefix} {Application.version}";
#else
        tmp.text = $"{prefix} {Application.version}";
#endif
        }
    }
}

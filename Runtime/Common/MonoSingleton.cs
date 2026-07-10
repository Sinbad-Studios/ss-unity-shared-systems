using UnityEngine;

namespace SinbadStudios.SharedSystems.Runtime
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {

        public static T Instance { get; private set; }

        public static bool IsInstantiated { get => Instance != null; }

        // Don't override Awake. Override Init instead.
        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("Multiple instances of " + typeof(T).ToString());
                Destroy(gameObject);
                return;
            }
            Instance = this as T;
            DontDestroyOnLoad(gameObject);
            Instance.Init();
        }

        /// <summary>
        /// Protected virtual method for initialization logic.
        /// Override this in derived classes to perform setup after the singleton instance is set.
        /// </summary>
        protected virtual void Init() { }
    }
}

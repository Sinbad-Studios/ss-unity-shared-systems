#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

namespace SinbadStudios.SharedSystems.Editor
{
    [InitializeOnLoad]
    public static class AlwaysStartFromFirstSceneInEditor
    {
        private const string MenuPath = "Tools/Always Start From First Scene";
        private const string EnabledPreferenceKey = "AlwaysStartFromFirstSceneInEditor.Enabled";

        static AlwaysStartFromFirstSceneInEditor()
        {
            EditorApplication.delayCall += SetPlayModeStartScene;
            EditorBuildSettings.sceneListChanged += SetPlayModeStartScene;
        }

        [MenuItem(MenuPath)]
        private static void ToggleEnabled()
        {
            bool enabled = !IsEnabled;
            EditorPrefs.SetBool(EnabledPreferenceKey, enabled);
            SetPlayModeStartScene();
        }

        [MenuItem(MenuPath, true)]
        private static bool ValidateToggleEnabled()
        {
            Menu.SetChecked(MenuPath, IsEnabled);
            return true;
        }

        private static bool IsEnabled => EditorPrefs.GetBool(EnabledPreferenceKey, true);

        private static void SetPlayModeStartScene()
        {
            if (!IsEnabled)
            {
                EditorSceneManager.playModeStartScene = null;
                return;
            }

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (!scene.enabled)
                {
                    continue;
                }

                EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
                return;
            }

            EditorSceneManager.playModeStartScene = null;
        }
    }
}
#endif

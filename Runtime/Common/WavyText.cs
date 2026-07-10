using TMPro;
using UnityEngine;

namespace SinbadStudios.SharedSystems.Runtime
{
    /// <summary>
    /// Applies a smooth wavy animation to a TextMeshProUGUI component by modifying its vertex positions over time.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class WavyText : MonoBehaviour
    {
        [Header("Wave Settings")]
        [Tooltip("Speed of the wave motion.")]
        [SerializeField] private float waveSpeed = 5f;

        [Tooltip("Height (amplitude) of the wave movement.")]
        [SerializeField] private float waveHeight = 5f;

        [Tooltip("Distance between each wave peak (frequency).")]
        [SerializeField] private float waveFrequency = 2f;

        private TextMeshProUGUI text;
        private TMP_TextInfo textInfo;

        private void Awake()
        {
            text = GetComponent<TextMeshProUGUI>();
            text.ForceMeshUpdate(); // Ensure textInfo is populated
            textInfo = text.textInfo;
        }

        private void Update()
        {
            // Update mesh info every frame for real-time animation
            text.ForceMeshUpdate();
            textInfo = text.textInfo;

            AnimateTextVertices();
            ApplyVertexChanges();
        }

        /// <summary>
        /// Applies a vertical sine-wave offset to each visible character's vertices.
        /// </summary>
        private void AnimateTextVertices()
        {
            int characterCount = textInfo.characterCount;

            for (int i = 0; i < characterCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible) continue;

                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                int vertexIndex = charInfo.vertexIndex;
                Vector3[] vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

                // Calculate the wave offset for this character
                Vector3 offset = CalculateWaveOffset(i);

                // Apply offset to all 4 vertices of the character quad
                vertices[vertexIndex + 0] += offset;
                vertices[vertexIndex + 1] += offset;
                vertices[vertexIndex + 2] += offset;
                vertices[vertexIndex + 3] += offset;
            }
        }

        /// <summary>
        /// Calculates the vertical offset for a given character index using sine wave logic.
        /// </summary>
        private Vector3 CalculateWaveOffset(int index)
        {
            float wave = Mathf.Sin(Time.unscaledTime * waveSpeed + index * waveFrequency) * waveHeight;
            return new Vector3(0, wave, 0);
        }

        /// <summary>
        /// Updates all text meshes after vertex modifications.
        /// </summary>
        private void ApplyVertexChanges()
        {
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                text.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }
        }
    }
}

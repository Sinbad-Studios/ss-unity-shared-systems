using UnityEngine;
using UnityEngine.UI;

namespace SinbadStudios.SharedSystems.Runtime
{
    [RequireComponent(typeof(Image))]
    public class ScrollingUIBackground : MonoBehaviour
    {
        [SerializeField] private float speed;
        [SerializeField] private Vector2 direction;

        private Image image;

        private void Awake()
        {
            image = GetComponent<Image>();
        }

        private void Update()
        {
            image.material.mainTextureOffset += -direction.normalized * Time.deltaTime * speed;
        }
    }
}

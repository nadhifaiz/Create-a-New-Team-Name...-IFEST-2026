using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 50f;
    [SerializeField] private float fadeSpeed = 2f;

    private TMP_Text textComponent;
    private Color textColor;

    private void Start()
    {
        textComponent = GetComponent<TMP_Text>();
        if (textComponent != null)
        {
            textColor = textComponent.color;
        }
    }

    private void Update()
    {
        // Gerak ke atas
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // Hilang perlahan (Fade Out)
        if (textComponent != null)
        {
            textColor.a -= fadeSpeed * Time.deltaTime;
            textComponent.color = textColor;

            // Hancurkan object jika sudah transparan sepenuhnya
            if (textColor.a <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
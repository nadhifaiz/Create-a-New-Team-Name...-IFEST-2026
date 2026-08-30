using UnityEngine;

namespace TypeForward
{
    /// <summary>
    /// Design-system asset: every visual token the typing UI uses lives here.
    /// Create via Assets > Create > TypeForward > Theme, then swap themes
    /// (e.g. Light/Dark/HighContrast) without touching any code.
    /// </summary>
    [CreateAssetMenu(fileName = "TypingTheme", menuName = "TypeForward/Theme")]
    public class TypingTheme : ScriptableObject
    {
        [Header("Text Colors")]
        public Color pendingColor = new Color(0.29f, 0.32f, 0.39f);   // untyped chars
        public Color correctColor = new Color(0.83f, 0.84f, 0.87f);  // typed correctly
        public Color incorrectColor = new Color(0.89f, 0.36f, 0.36f); // typed wrong
        public Color cursorColor = new Color(0.89f, 0.72f, 0.08f);   // caret / active char

        [Header("Accent")]
        public Color backgroundColor = new Color(0.06f, 0.07f, 0.09f);
        public Color panelColor = new Color(0.09f, 0.11f, 0.15f);

        [Header("Feedback")]
        public float mistakeFlashDuration = 0.15f;
        public float shakeMagnitude = 6f;

        public string ColorHex(Color c) => "#" + ColorUtility.ToHtmlStringRGB(c);
    }
}

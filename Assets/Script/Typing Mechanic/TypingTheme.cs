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
        public Color incorrectColor = new Color(0.89f, 0.36f, 0.36f); // typed wrong / overtyped
        public Color cursorColor = new Color(0.89f, 0.72f, 0.08f);   // caret / active char

        [Header("Sentence Transition Animation")]
        [Tooltip("How far (in UI pixels) the sentence slides in from below / out to above.")]
        public float slideDistance = 60f;
        [Tooltip("Duration of the fade-out + slide-up when a sentence finishes.")]
        public float fadeOutDuration = 0.25f;
        [Tooltip("Duration of the fade-in + slide-into-place for the next sentence.")]
        public float fadeInDuration = 0.3f;

        public string ColorHex(Color c) => "#" + ColorUtility.ToHtmlStringRGB(c);
    }
}
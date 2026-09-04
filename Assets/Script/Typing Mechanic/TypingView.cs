using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace TypeForward
{
    /// <summary>
    /// Pure presentation: turns CharCells into TMP rich text and plays the
    /// fade-out-up / fade-in-from-below transition between sentences.
    /// Has zero game-rule knowledge — Controller decides WHAT and WHEN,
    /// this class only knows HOW to draw and animate it.
    /// </summary>
    public class TypingView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text sentenceDisplay;
        [Tooltip("Optional. Parent RectTransform that slides/fades. If empty, sentenceDisplay's own RectTransform is used.")]
        [SerializeField] private RectTransform sentenceRoot;
        [Tooltip("Optional but required for fading. Put a CanvasGroup on the same object as sentenceRoot.")]
        [SerializeField] private CanvasGroup sentenceCanvasGroup;

        [Header("Theme")]
        [SerializeField] private TypingTheme theme;

        private readonly StringBuilder _sb = new StringBuilder(1024);
        private Vector2 _basePosition;
        private bool _basePositionCaptured;

        private void Awake()
        {
            if (sentenceRoot == null && sentenceDisplay != null) sentenceRoot = sentenceDisplay.rectTransform;
            CaptureBasePosition();
        }

        private void CaptureBasePosition()
        {
            if (_basePositionCaptured || sentenceRoot == null) return;
            _basePosition = sentenceRoot.anchoredPosition;
            _basePositionCaptured = true;
        }

        public void RenderSentence(IReadOnlyList<CharCell> cells)
        {
            if (sentenceDisplay == null) return;

            _sb.Clear();
            string pending = theme.ColorHex(theme.pendingColor);
            string correct = theme.ColorHex(theme.correctColor);
            string incorrect = theme.ColorHex(theme.incorrectColor);
            string cursor = theme.ColorHex(theme.cursorColor);

            foreach (var cell in cells)
            {
                string glyph = cell.Character == ' ' ? " " : cell.Character.ToString();
                switch (cell.State)
                {
                    case CharState.Correct:
                        _sb.Append("<color=").Append(correct).Append('>').Append(glyph).Append("</color>");
                        break;
                    case CharState.Incorrect:
                        _sb.Append("<color=").Append(incorrect).Append("><u>").Append(glyph == " " ? "_" : glyph)
                           .Append("</u></color>");
                        break;
                    case CharState.Extra:
                        _sb.Append("<color=").Append(incorrect).Append("><s>").Append(glyph).Append("</s></color>");
                        break;
                    case CharState.Cursor:
                        _sb.Append("<color=").Append(cursor).Append("><u>").Append(glyph).Append("</u></color>");
                        break;
                    default:
                        _sb.Append("<color=").Append(pending).Append('>').Append(glyph).Append("</color>");
                        break;
                }
            }

            sentenceDisplay.text = _sb.ToString();
        }

        /// <summary>
        /// Plays: current sentence fades out while sliding up -> onMidpoint (Controller swaps
        /// the text to the next sentence here) -> new sentence starts below and fades/slides
        /// up into its resting position -> onComplete.
        /// </summary>
        public void PlaySentenceTransition(Action onMidpoint, Action onComplete)
        {
            StartCoroutine(TransitionRoutine(onMidpoint, onComplete));
        }

        private IEnumerator TransitionRoutine(Action onMidpoint, Action onComplete)
        {
            CaptureBasePosition();
            float slide = theme != null ? theme.slideDistance : 60f;
            float outDur = theme != null ? theme.fadeOutDuration : 0.25f;
            float inDur = theme != null ? theme.fadeInDuration : 0.3f;

            // --- FADE OUT + SLIDE UP ---
            yield return Animate(
                fromPos: _basePosition,
                toPos: _basePosition + Vector2.up * slide,
                fromAlpha: 1f,
                toAlpha: 0f,
                duration: outDur);

            onMidpoint?.Invoke(); // Controller swaps to the next sentence and calls RenderSentence here

            // --- FADE IN FROM BELOW ---
            yield return Animate(
                fromPos: _basePosition - Vector2.up * slide,
                toPos: _basePosition,
                fromAlpha: 0f,
                toAlpha: 1f,
                duration: inDur);

            onComplete?.Invoke();
        }

        private IEnumerator Animate(Vector2 fromPos, Vector2 toPos, float fromAlpha, float toAlpha, float duration)
        {
            if (sentenceRoot != null) sentenceRoot.anchoredPosition = fromPos;
            if (sentenceCanvasGroup != null) sentenceCanvasGroup.alpha = fromAlpha;

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = duration <= 0f ? 1f : Mathf.Clamp01(t / duration);
                float eased = 1f - Mathf.Pow(1f - k, 2f); // ease-out, feels snappier than linear

                if (sentenceRoot != null) sentenceRoot.anchoredPosition = Vector2.LerpUnclamped(fromPos, toPos, eased);
                if (sentenceCanvasGroup != null) sentenceCanvasGroup.alpha = Mathf.LerpUnclamped(fromAlpha, toAlpha, eased);

                yield return null;
            }

            if (sentenceRoot != null) sentenceRoot.anchoredPosition = toPos;
            if (sentenceCanvasGroup != null) sentenceCanvasGroup.alpha = toAlpha;
        }

        /// <summary>Hook for backspace-rejected feedback (shake, sound). Override or extend as needed.</summary>
        public virtual void PlayBackspaceRejectedFeedback() { }
    }
}
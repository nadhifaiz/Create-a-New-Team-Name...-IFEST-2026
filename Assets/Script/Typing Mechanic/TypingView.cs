using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace TypeForward
{
    /// <summary>
    /// Pure presentation: turns CharCells / results into TMP rich text.
    /// Has zero game-rule knowledge — it only knows how to draw what it's told.
    /// Swap this class out entirely (e.g. for a per-word highlight renderer,
    /// or a non-TMP UI toolkit view) without touching TypingSession.
    /// </summary>
    public class TypingView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text wordDisplay;
        [SerializeField] private TMP_Text timerDisplay;
        [SerializeField] private TMP_Text resultsDisplay;
        [SerializeField] private GameObject resultsPanel;

        [Header("Theme")]
        [SerializeField] private TypingTheme theme;

        [Header("Scrolling")]
        [Tooltip("How many lines of text stay visible inside the masked viewport.")]
        [SerializeField] private int visibleLines = 3;
        [Tooltip("Optional: overrides auto-detected line height (world px). Leave 0 to auto-detect from TMP.")]
        [SerializeField] private float lineHeightOverride = 0f;

        private readonly StringBuilder _sb = new StringBuilder(4096);
        private int _lastCursorIndex = -1;

        public void ShowGameplay()
        {
            if (resultsPanel != null) resultsPanel.SetActive(false);
            _lastCursorIndex = -1;
            if (wordDisplay != null)
            {
                var pos = wordDisplay.rectTransform.anchoredPosition;
                pos.y = 0f;
                wordDisplay.rectTransform.anchoredPosition = pos;
            }
        }

        public void RenderText(IReadOnlyList<CharCell> cells)
        {
            if (wordDisplay == null) return;

            _sb.Clear();
            string pending = theme.ColorHex(theme.pendingColor);
            string correct = theme.ColorHex(theme.correctColor);
            string incorrect = theme.ColorHex(theme.incorrectColor);
            string cursor = theme.ColorHex(theme.cursorColor);
            string extra = theme.ColorHex(theme.incorrectColor); // overtyped chars use the same "wrong" color

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
                        // typed past the end of the word — show with strikethrough so it reads as "shouldn't be here"
                        _sb.Append("<color=").Append(extra).Append("><s>").Append(glyph).Append("</s></color>");
                        break;
                    case CharState.Cursor:
                        _sb.Append("<color=").Append(cursor).Append("><u>").Append(glyph).Append("</u></color>");
                        break;
                    default:
                        _sb.Append("<color=").Append(pending).Append('>').Append(glyph).Append("</color>");
                        break;
                }
            }

            wordDisplay.text = _sb.ToString();
            ScrollToCursor(cells);
        }

        /// <summary>
        /// Keeps the current typing line inside the visible window by shifting
        /// wordDisplay upward inside its masked viewport parent, Monkeytype-style.
        /// wordDisplay's RectTransform must be top-anchored/top-pivoted and sit
        /// inside a parent with a Mask or RectMask2D of fixed height.
        /// </summary>
        private void ScrollToCursor(IReadOnlyList<CharCell> cells)
        {
            // Find the index of the cursor character. If the current word is exactly full
            // (just-typed the last letter, space not pressed yet), there's no Cursor cell —
            // fall back to the first Pending cell instead of the very end of the whole text,
            // otherwise the view briefly scrolls to the last generated line.
            int cursorIndex = -1;
            int firstPending = -1;
            for (int i = 0; i < cells.Count; i++)
            {
                if (cells[i].State == CharState.Cursor) { cursorIndex = i; break; }
                if (firstPending == -1 && cells[i].State == CharState.Pending) firstPending = i;
            }
            if (cursorIndex == -1) cursorIndex = firstPending != -1 ? firstPending : cells.Count - 1;
            if (cursorIndex == _lastCursorIndex) return; // avoid redundant mesh work
            _lastCursorIndex = cursorIndex;

            wordDisplay.ForceMeshUpdate();
            var textInfo = wordDisplay.textInfo;
            if (textInfo.lineCount == 0) return;

            int cursorLine = 0;
            if (cursorIndex >= 0 && cursorIndex < textInfo.characterCount)
                cursorLine = textInfo.characterInfo[cursorIndex].lineNumber;
            else
                cursorLine = textInfo.lineCount - 1;

            float lineHeight = lineHeightOverride > 0f
                ? lineHeightOverride
                : textInfo.lineInfo[0].lineHeight;

            // Keep the cursor's line as the 2nd visible line (one line of context above, one below).
            int scrollLine = Mathf.Max(0, cursorLine - (visibleLines - 2));

            var rt = wordDisplay.rectTransform;
            Vector2 pos = rt.anchoredPosition;
            pos.y = scrollLine * lineHeight;
            rt.anchoredPosition = pos;
        }

        public void RenderTime(float timeLeft)
        {
            if (timerDisplay != null)
                timerDisplay.text = Mathf.CeilToInt(timeLeft).ToString();
        }

        public void RenderResults(SessionResult result)
        {
            if (resultsDisplay != null)
            {
                resultsDisplay.text =
                    $"WPM: {result.Wpm}\nAccuracy: {result.Accuracy}%\nCorrect: {result.CorrectChars}\nMistakes: {result.IncorrectChars}";
            }
            if (resultsPanel != null) resultsPanel.SetActive(true);
        }

        /// <summary>Hook for mistake/backspace feedback (shake, flash, sound). Override or extend as needed.</summary>
        public virtual void PlayMistakeFeedback() { }
        public virtual void PlayBackspaceRejectedFeedback() { }
    }
}
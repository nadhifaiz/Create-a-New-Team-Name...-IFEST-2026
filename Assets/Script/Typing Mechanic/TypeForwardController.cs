using UnityEngine;

namespace TypeForward
{
    /// <summary>
    /// Orchestrator only. Owns no game rules (that's TypingSession) and no
    /// rendering logic (that's TypingView) — it just wires input → session → view
    /// every frame and reacts to session events. This is the only class that
    /// needs to change if you swap engines, input systems, or UI frameworks.
    /// </summary>
    [RequireComponent(typeof(TypingView))]
    public class TypeForwardController : MonoBehaviour
    {
        [SerializeField] private TypingConfig config;
        [SerializeField] private TypingView view;

        private TypingSession _session;
        private IInputProvider _input;

        private void Awake()
        {
            if (view == null) view = GetComponent<TypingView>();
            _input = new LegacyInputProvider();
        }

        private void OnEnable()
        {
            StartNewSession();
        }

        private void OnDisable()
        {
            UnsubscribeSession();
        }

        public void StartNewSession()
        {
            UnsubscribeSession();

            _session = new TypingSession(config);
            _session.MistakeMade += view.PlayMistakeFeedback;
            _session.BackspaceRejected += view.PlayBackspaceRejectedFeedback;
            _session.TimeChanged += view.RenderTime;
            _session.Finished += view.RenderResults;
            _session.CharacterTyped += () => view.RenderText(_session.BuildCharCells());
            _session.TextExtended += () => view.RenderText(_session.BuildCharCells());

            view.ShowGameplay();
            view.RenderTime(config.durationSeconds);
            view.RenderText(_session.BuildCharCells());
        }

        private void UnsubscribeSession()
        {
            if (_session == null) return;
            _session.MistakeMade -= view.PlayMistakeFeedback;
            _session.BackspaceRejected -= view.PlayBackspaceRejectedFeedback;
            _session.TimeChanged -= view.RenderTime;
            _session.Finished -= view.RenderResults;
            // lambda subscribers (CharacterTyped/TextExtended) are dropped along with the session instance
        }

        private void Update()
        {
            if (_session == null || _session.IsFinished) return;

            if (_input.WasBackspacePressedThisFrame())
            {
                _session.SubmitBackspace();
                return;
            }

            string frame = _input.ConsumeFrameInput();
            if (!string.IsNullOrEmpty(frame))
            {
                foreach (char c in frame)
                {
                    if (c == '\b') continue; // backspace already handled above
                    _session.SubmitChar(c);
                }
            }

            _session.Tick(Time.deltaTime);
        }

        /// <summary>Wired to a UI "Try Again" button.</summary>
        public void OnRetryButtonPressed() => StartNewSession();
    }
}

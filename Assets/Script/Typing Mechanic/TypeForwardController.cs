using System;
using System.Collections.Generic;
using UnityEngine;

namespace TypeForward
{
    [RequireComponent(typeof(TypingView))]
    public class TypeForwardController : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private TypingConfig config;
        [SerializeField] private TypingView view;

        [Header("Combat Hook (WAJIB diisi)")]
        [Tooltip("Drag GameObject yang punya komponen TypingCombatBridge. " +
                 "Attack() dan SetCombo() dipanggil langsung dari kode — tidak perlu wiring UnityEvent.")]
        [SerializeField] private TypingCombatBridge combatBridge;

        // Optional feedback hooks — plain C# events, BUKAN UnityEvent, jadi tidak
        // muncul di Inspector (aman dari salah setting Dynamic/Static Parameters).
        // Subscribe dari script lain kalau perlu, misal buat SFX/VFX kecil.
        public event Action OnWordCorrect;
        public event Action OnWordMistake;
        public event Action<int> OnComboChanged;
        public event Action OnSentenceComplete;

        private IInputProvider _input; // baca input
        private List<string[]> _sentences; // Kalimat yang disimpan Perkata

        private int _sentenceIndex; // Track kalimat ke berapa
        private int _wordIndex; // Track kata ke berapa
        private readonly System.Text.StringBuilder _currentTyped = new System.Text.StringBuilder();
        private string[] _committedWords; // Kata kata yang sudah diketik

        private int _consecutiveCorrectWords;
        private int _combo;
        private bool _transitioning; // true while the View is mid fade-out/fade-in, input is ignored

        private void Awake()
        {
            if (view == null) view = GetComponent<TypingView>();
            _input = new LegacyInputProvider();
        }

        private void OnEnable()
        {
            BuildSentenceList();
            StartSentence(0);
        }

        private void Update()
        {
            if (_transitioning) return;

            // Lagi ada enemy nge-special-attack (parry QTE aktif) -> minigame ketik
            // ikut berhenti total. Kita cuma nengok bridge, gak perlu tau apa-apa
            // soal FighterBase/CombatFreezeSignal di sini.
            if (combatBridge != null && combatBridge.IsPaused) return;

            if (_input.WasBackspacePressedThisFrame())
            {
                view.PlayBackspaceRejectedFeedback();
                return;
            }

            string frame = _input.ConsumeFrameInput();
            if (string.IsNullOrEmpty(frame)) return;

            foreach (char c in frame)
            {
                if (c == '\b') continue; // backspace handled above
                HandleChar(c);
                if (_transitioning) break; // a sentence completed mid-loop; stop consuming this frame's input
            }
        }

        // ============================================================
        // CONTENT SETUP
        // ============================================================

        private void BuildSentenceList()
        {
            // 1. Jadikan Setiap kalimat ke dalam bentuk Per Kata
            var source = (config != null && config.sentences.Count > 0)
                ? config.sentences
                : new List<string>(TypingConfig.DefaultSentences);

            // 2. Jadikan Per Kalimat
            _sentences = new List<string[]>(source.Count);
            foreach (string sentence in source)
                _sentences.Add(sentence.Split(' '));
        }

        private void StartSentence(int index)
        {
            _sentenceIndex = index;
            _wordIndex = 0;
            _currentTyped.Clear();
            _committedWords = new string[_sentences[_sentenceIndex].Length];
            for (int i = 0; i < _committedWords.Length; i++) _committedWords[i] = null;

            RenderCurrentState();
        }

        // ============================================================
        // INPUT HANDLING
        // ============================================================

        private void HandleChar(char c)
        {
            if (c == ' ')
            {
                // Ambil kata target saat ini
                string targetWord = _sentences[_sentenceIndex][_wordIndex];

                // Cek apakah jumlah karakter yang diketik sudah minimal sama dengan panjang kata target
                if (_currentTyped.Length >= targetWord.Length)
                {
                    CommitWord();
                }
                else
                {
                    // Opsional: Mainkan efek visual/audio jika pemain mencoba skip terlalu cepat
                    view.PlayBackspaceRejectedFeedback();
                }
            }
            else
            {
                _currentTyped.Append(c);
                RenderCurrentState();
            }
        }

        private void CommitWord()
        {
            string[] words = _sentences[_sentenceIndex];
            string target = words[_wordIndex];
            string typed = _currentTyped.ToString();
            _committedWords[_wordIndex] = typed;

            bool isCorrect = typed == target;
            ScoreWord(isCorrect);

            _wordIndex++;
            _currentTyped.Clear();

            bool sentenceDone = _wordIndex >= words.Length;
            if (sentenceDone)
            {
                RenderCurrentState(); // show the finished sentence fully colored for a beat before it animates out
                AdvanceSentence();
            }
            else
            {
                RenderCurrentState();
            }
        }

        private void ScoreWord(bool isCorrect)
        {
            if (isCorrect)
            {
                _consecutiveCorrectWords++;
                // comboStartsAfterWords consecutive correct words needed before combo becomes 1
                int threshold = Mathf.Max(1, config != null ? config.comboStartsAfterWords : 2);
                _combo = Mathf.Max(0, _consecutiveCorrectWords - (threshold - 1));

                Debug.Log("Kata Benar!!!, KOMBO SAAT INI: " + _combo);

                OnWordCorrect?.Invoke(); // Untuk Efek SFX
                OnComboChanged?.Invoke(_combo);
                if (combatBridge != null) combatBridge.SetCombo(_combo);
            }
            else
            {
                _consecutiveCorrectWords = 0;
                _combo = 0;

                OnWordMistake?.Invoke();
                OnComboChanged?.Invoke(_combo);
                if (combatBridge != null) combatBridge.SetCombo(_combo);
            }
        }

        // ============================================================
        // SENTENCE TRANSITION
        // ============================================================

        private void AdvanceSentence()
        {
            OnSentenceComplete?.Invoke();
            if (combatBridge != null) combatBridge.Attack();

            _transitioning = true;
            view.PlaySentenceTransition(onMidpoint: () =>
            {
                int next = _sentenceIndex + 1;
                if (next >= _sentences.Count) next = 0; // loop back to the start of the "gudang kalimat"
                StartSentence(next);
            },
            onComplete: () =>
            {
                _transitioning = false;
            });
        }

        // ============================================================
        // RENDERING
        // ============================================================

        private void RenderCurrentState()
        {
            view.RenderSentence(BuildCharCells());
        }

        private IReadOnlyList<CharCell> BuildCharCells()
        {
            string[] words = _sentences[_sentenceIndex];
            var cells = new List<CharCell>(64);

            for (int wi = 0; wi < words.Length; wi++)
            {
                string word = words[wi];
                string typed = wi < _wordIndex ? _committedWords[wi]
                             : wi == _wordIndex ? _currentTyped.ToString()
                             : null;

                int maxLen = Math.Max(word.Length, typed?.Length ?? 0);
                for (int p = 0; p < maxLen; p++)
                {
                    if (typed == null)
                    {
                        cells.Add(new CharCell(word[p], CharState.Pending));
                    }
                    else if (p < word.Length && p < typed.Length)
                    {
                        cells.Add(new CharCell(word[p], typed[p] == word[p] ? CharState.Correct : CharState.Incorrect));
                    }
                    else if (p < word.Length)
                    {
                        if (wi == _wordIndex)
                        {
                            // active word: p == typed.Length is the exact next char (cursor),
                            // anything further right just hasn't been reached yet (pending)
                            bool isCursor = p == typed.Length;
                            cells.Add(new CharCell(word[p], isCursor ? CharState.Cursor : CharState.Pending));
                        }
                        else
                        {
                            // a PAST word (space was pressed early) with a letter that was never
                            // typed at all — it's a miss, not "still pending"
                            cells.Add(new CharCell(word[p], CharState.Incorrect));
                        }
                    }
                    else
                    {
                        cells.Add(new CharCell(typed[p], CharState.Extra));
                    }
                }

                if (wi < words.Length - 1) cells.Add(new CharCell(' ', CharState.Pending));
            }

            return cells;
        }
    }
}
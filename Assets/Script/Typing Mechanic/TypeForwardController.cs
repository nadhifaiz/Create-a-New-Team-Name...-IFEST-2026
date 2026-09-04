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
        [Tooltip("Drag GameObject yang punya komponen TypingCombatBridge.")]
        [SerializeField] private TypingCombatBridge combatBridge;

        public event Action OnWordCorrect;
        public event Action OnWordMistake;
        public event Action<int> OnComboChanged;
        public event Action OnSentenceComplete;

        private IInputProvider _input;
        private List<string[]> _sentences;

        private int _sentenceIndex;
        private int _wordIndex;
        private readonly System.Text.StringBuilder _currentTyped = new System.Text.StringBuilder();
        private string[] _committedWords;

        private int _consecutiveCorrectWords;
        private int _combo;
        private bool _transitioning;

        // --- STATS TRACKER ---
        private int _totalKeystrokes;
        private int _correctKeystrokes;
        private float _typingActiveTime;
        private bool _isSessionActive;

        private void Awake()
        {
            if (view == null) view = GetComponent<TypingView>();
            _input = new LegacyInputProvider();
        }

        private void OnEnable()
        {
            ResetStats();
            BuildSentenceList();
            StartSentence(0);
        }

        private void Update()
        {
            if (_transitioning)
            {
                return;
            }

            if (GameManager.Instance != null && (GameManager.Instance.isGameOver || GameManager.Instance.isPaused))
            {
                return;
            }

            if (combatBridge != null && combatBridge.IsPaused)
            {
                return;
            }

            // Timer hanya berjalan saat pemain sedang aktif mengetik di fase normal
            if (_isSessionActive)
            {
                _typingActiveTime += Time.deltaTime;
            }

            if (_input.WasBackspacePressedThisFrame())
            {
                view.PlayBackspaceRejectedFeedback();
                return;
            }

            string frame = _input.ConsumeFrameInput();
            if (string.IsNullOrEmpty(frame)) return;

            foreach (char c in frame)
            {
                if (c == '\b') continue;
                HandleChar(c);
                if (_transitioning) break;
            }
        }

        // ============================================================
        // CONTENT SETUP
        // ============================================================

        private void ResetStats()
        {
            _totalKeystrokes = 0;
            _correctKeystrokes = 0;
            _typingActiveTime = 0f;
            _isSessionActive = false;
        }

        private void BuildSentenceList()
        {
            var source = (config != null && config.sentences.Count > 0)
                ? config.sentences
                : new List<string>(TypingConfig.DefaultSentences);

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
        // INPUT HANDLING & STATS CALCULATION
        // ============================================================

        private void HandleChar(char c)
        {
            // Mulai timer WPM tepat saat pemain menekan tombol pertama
            if (!_isSessionActive) _isSessionActive = true;

            _totalKeystrokes++;
            string targetWord = _sentences[_sentenceIndex][_wordIndex];

            if (c == ' ')
            {
                if (_currentTyped.Length >= targetWord.Length)
                {
                    // Anggap spasi sebagai karakter benar jika seluruh kata sebelumnya benar
                    if (_currentTyped.ToString() == targetWord) _correctKeystrokes++;
                    CommitWord();
                }
                else
                {
                    view.PlayBackspaceRejectedFeedback();
                }
            }
            else
            {
                int currentIndex = _currentTyped.Length;

                // Cek apakah huruf yang diketik sama dengan huruf target di posisi saat ini
                if (currentIndex < targetWord.Length && c == targetWord[currentIndex])
                {
                    _correctKeystrokes++;
                }

                _currentTyped.Append(c);
                RenderCurrentState();
            }

            CalculateAndFireStats();
        }

        private void CalculateAndFireStats()
        {
            // Ubah detik menjadi menit
            float minutesElapsed = _typingActiveTime / 60f;

            // Rumus WPM Internasional: (Total Karakter Benar / 5) / Menit
            int wpm = minutesElapsed > 0 ? Mathf.RoundToInt((_correctKeystrokes / 5f) / minutesElapsed) : 0;

            // Rumus Akurasi: (Karakter Benar / Total Ketikan) * 100
            float accuracy = _totalKeystrokes > 0 ? ((float)_correctKeystrokes / _totalKeystrokes) * 100f : 100f;

            // Langsung kirim dan simpan ke GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.UpdateTypingStats(wpm, accuracy);
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
                RenderCurrentState();
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
                int threshold = Mathf.Max(1, config != null ? config.comboStartsAfterWords : 2);
                _combo = Mathf.Max(0, _consecutiveCorrectWords - (threshold - 1));

                OnWordCorrect?.Invoke();
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
        // SENTENCE TRANSITION & RENDERING (TETAP SAMA)
        // ============================================================

        private void AdvanceSentence()
        {
            // Pause timer sementara saat ada animasi pergantian kalimat
            _isSessionActive = false;

            OnSentenceComplete?.Invoke();
            if (combatBridge != null) combatBridge.Attack();

            _transitioning = true;
            view.PlaySentenceTransition(onMidpoint: () =>
            {
                int next = _sentenceIndex + 1;
                if (next >= _sentences.Count) next = 0;
                StartSentence(next);
            },
            onComplete: () =>
            {
                _transitioning = false;
            });
        }

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
                            bool isCursor = p == typed.Length;
                            cells.Add(new CharCell(word[p], isCursor ? CharState.Cursor : CharState.Pending));
                        }
                        else
                        {
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
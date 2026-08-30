using System;
using System.Collections.Generic;
using System.Text;

namespace TypeForward
{
    public enum CharState { Pending, Correct, Incorrect, Cursor, Extra }

    /// <summary>Immutable snapshot of one character's typing state, for the view to render.</summary>
    public readonly struct CharCell
    {
        public readonly char Character;
        public readonly CharState State;
        public CharCell(char character, CharState state)
        {
            Character = character;
            State = state;
        }
    }

    public readonly struct SessionResult
    {
        public readonly int Wpm;
        public readonly int Accuracy;
        public readonly int CorrectChars;
        public readonly int IncorrectChars;
        public SessionResult(int wpm, int accuracy, int correctChars, int incorrectChars)
        {
            Wpm = wpm;
            Accuracy = accuracy;
            CorrectChars = correctChars;
            IncorrectChars = incorrectChars;
        }
    }

    /// <summary>
    /// Core game rules for a no-backspace typing test, WORD-SYNCED like Monkeytype:
    /// pressing Space always commits the current word and jumps to the next one,
    /// no matter how many characters were typed. This means one mistake only ever
    /// affects the word it happened in — it can never desync the rest of the test.
    /// Knows nothing about Unity, TMP, or input devices.
    /// </summary>
    public class TypingSession
    {
        public event Action Started;
        public event Action CharacterTyped;                 // fired for every accepted keystroke (char or space)
        public event Action MistakeMade;                    // fired only on wrong/extra characters
        public event Action BackspaceRejected;               // fired when backspace is attempted
        public event Action<float> TimeChanged;              // fired every Tick with time remaining
        public event Action<SessionResult> Finished;
        public event Action TextExtended;                    // fired when more words are appended

        private readonly TypingConfig _config;
        private readonly Random _rng;

        private readonly List<string> _words = new List<string>();
        private readonly List<string> _typedWords = new List<string>(); // committed typed text per finished word
        private readonly StringBuilder _currentTyped = new StringBuilder();

        private int _currentWordIndex;
        private int _correctCount;
        private int _incorrectCount;
        private float _elapsed;
        private float _timeLeft;

        public bool Started_ { get; private set; }
        public bool IsFinished { get; private set; }
        public int CurrentWordIndex => _currentWordIndex;

        public TypingSession(TypingConfig config, int? seed = null)
        {
            _config = config;
            _rng = seed.HasValue ? new Random(seed.Value) : new Random();
            Reset();
        }

        public void Reset()
        {
            _words.Clear();
            _typedWords.Clear();
            _currentTyped.Clear();
            _currentWordIndex = 0;
            _correctCount = 0;
            _incorrectCount = 0;
            _elapsed = 0f;
            _timeLeft = _config.durationSeconds;
            Started_ = false;
            IsFinished = false;

            AppendWords(_config.initialWordCount);
        }

        /// <summary>Feed one input character. Space commits the current word and advances; anything else is typed into it.</summary>
        public void SubmitChar(char c)
        {
            if (IsFinished) return;
            if (char.IsControl(c) && c != ' ') return;

            if (!Started_)
            {
                Started_ = true;
                Started?.Invoke();
            }

            if (c == ' ')
            {
                CommitCurrentWord();
                CharacterTyped?.Invoke();
                return;
            }

            string word = CurrentWord();
            int pos = _currentTyped.Length;
            bool correct = pos < word.Length && c == word[pos];

            _currentTyped.Append(c);
            if (correct) _correctCount++;
            else
            {
                _incorrectCount++;
                MistakeMade?.Invoke();
            }

            CharacterTyped?.Invoke();
        }

        /// <summary>Call when the player presses Backspace. Always rejected — this is the game's rule.</summary>
        public void SubmitBackspace()
        {
            if (IsFinished) return;
            BackspaceRejected?.Invoke();
        }

        /// <summary>Advance the countdown. Call once per frame with Time.deltaTime.</summary>
        public void Tick(float deltaTime)
        {
            if (!Started_ || IsFinished) return;

            _elapsed += deltaTime;
            _timeLeft -= deltaTime;
            if (_timeLeft <= 0f)
            {
                _timeLeft = 0f;
                TimeChanged?.Invoke(_timeLeft);
                Finish();
                return;
            }
            TimeChanged?.Invoke(_timeLeft);
        }

        public float TimeLeft => _timeLeft;

        private void CommitCurrentWord()
        {
            EnsureTypedWordsSlot();
            _typedWords[_currentWordIndex] = _currentTyped.ToString();
            _currentTyped.Clear();
            _currentWordIndex++;

            if (_words.Count - _currentWordIndex < _config.refillThreshold)
            {
                AppendWords(_config.refillAmount);
                TextExtended?.Invoke();
            }
        }

        private void EnsureTypedWordsSlot()
        {
            while (_typedWords.Count <= _currentWordIndex) _typedWords.Add(string.Empty);
        }

        private string CurrentWord() => _currentWordIndex < _words.Count ? _words[_currentWordIndex] : string.Empty;

        private void Finish()
        {
            IsFinished = true;
            float minutes = Math.Max(_elapsed / 60f, 1f / 60f);
            int wpm = (int)Math.Round((_correctCount / 5f) / minutes);
            int total = _correctCount + _incorrectCount;
            int accuracy = total > 0 ? (int)Math.Round((_correctCount / (float)total) * 100f) : 100;
            Finished?.Invoke(new SessionResult(wpm, accuracy, _correctCount, _incorrectCount));
        }

        /// <summary>
        /// Builds the per-character render state for the whole visible text, word by word.
        /// Extra typed characters beyond a word's length are still emitted (State.Extra)
        /// so the view can show them, mirroring Monkeytype's overtype display.
        /// </summary>
        public IReadOnlyList<CharCell> BuildCharCells()
        {
            var cells = new List<CharCell>(_words.Count * 6);

            for (int wi = 0; wi < _words.Count; wi++)
            {
                string word = _words[wi];
                string typed = wi < _currentWordIndex ? (wi < _typedWords.Count ? _typedWords[wi] : string.Empty)
                             : wi == _currentWordIndex ? _currentTyped.ToString()
                             : null; // not reached yet

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
                        if (wi == _currentWordIndex)
                        {
                            // active word: this is the exact next character to type
                            bool isCursor = p == typed.Length;
                            cells.Add(new CharCell(word[p], isCursor ? CharState.Cursor : CharState.Pending));
                        }
                        else
                        {
                            // word already committed (Space was pressed early) but this letter
                            // was never typed — it's a miss, NOT "still pending", so it must never
                            // be picked up as the "first untyped character" by the scroll logic.
                            cells.Add(new CharCell(word[p], CharState.Incorrect));
                        }
                    }
                    else
                    {
                        // typed beyond the word's length — extra/overtyped character
                        cells.Add(new CharCell(typed[p], CharState.Extra));
                    }
                }

                // space between words
                if (typed == null)
                    cells.Add(new CharCell(' ', CharState.Pending));
                else if (wi < _currentWordIndex)
                    cells.Add(new CharCell(' ', CharState.Correct));
                else
                    cells.Add(new CharCell(' ', word.Length == 0 && typed.Length == 0 ? CharState.Cursor : CharState.Pending));
            }

            return cells;
        }

        private void AppendWords(int n)
        {
            var bank = _config.words.Count > 0 ? _config.words : new List<string>(TypingConfig.DefaultWordBank);
            for (int i = 0; i < n; i++)
                _words.Add(bank[_rng.Next(bank.Count)]);
        }
    }
}
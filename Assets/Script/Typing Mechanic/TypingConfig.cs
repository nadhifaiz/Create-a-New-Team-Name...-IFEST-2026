using System.Collections.Generic;
using UnityEngine;

namespace TypeForward
{
    /// <summary>
    /// Gameplay content & tuning. Create via Assets > Create > TypeForward > Config.
    /// Pure data — no logic lives here, so designers can tweak sentences/combo rules
    /// without touching code.
    /// </summary>
    [CreateAssetMenu(fileName = "TypingConfig", menuName = "TypeForward/Config")]
    public class TypingConfig : ScriptableObject
    {
        [Header("Content")]
        [TextArea(1, 3)]
        public List<string> sentences = new List<string>(DefaultSentences);

        [Header("Combo Rules")]
        public int comboStartsAfterWords = 2;

        public static readonly string[] DefaultSentences =
        {
            "aku suka kucing",
            "hari ini cuaca cerah",
            "dia sedang belajar unity",
            "kita akan menang bersama",
            "waktu terus berjalan maju",
        };
    }
}
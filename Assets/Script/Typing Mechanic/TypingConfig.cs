using System.Collections.Generic;
using UnityEngine;

namespace TypeForward
{
    /// <summary>
    /// Gameplay configuration asset — word bank, duration, refill thresholds.
    /// Create via Assets > Create > TypeForward > Config.
    /// Keeping this separate from TypingTheme means designers can tweak
    /// difficulty/content without touching color/visual settings.
    /// </summary>
    [CreateAssetMenu(fileName = "TypingConfig", menuName = "TypeForward/Config")]
    public class TypingConfig : ScriptableObject
    {
        [Header("Session")]
        public float durationSeconds = 30f;

        [Header("Word Generation")]
        public int initialWordCount = 80;
        public int refillThreshold = 20;
        public int refillAmount = 40;

        [Header("Word Bank")]
        public List<string> words = new List<string>(DefaultWordBank);

        public static readonly string[] DefaultWordBank = new[]
        {
            "time","year","people","way","day","man","thing","woman","life","child",
            "world","school","state","family","student","group","country","problem","hand","part",
            "place","case","week","company","system","program","question","work","government","number",
            "night","point","home","water","room","mother","area","money","story","fact",
            "month","lot","right","study","book","eye","job","word","business","issue",
            "side","kind","head","house","service","friend","father","power","hour","game",
            "line","end","member","law","car","city","community","name","president","team",
            "minute","idea","body","information","back","parent","face","others","level","office",
            "door","health","person","art","war","history","party","result","change","morning",
            "reason","research","girl","guy","moment","air","teacher","force","education","foot",
            "boy","age","policy","process","music","market","sense","nation","plan","college",
            "interest","death","experience","effect","use","class","control","care","field","development",
            "role","effort","rate","heart","drive","risk","bright","build","focus","climb",
            "bridge","spark","quiet","forest","river","mountain","ocean","silence","journey","wonder"
        };
    }
}

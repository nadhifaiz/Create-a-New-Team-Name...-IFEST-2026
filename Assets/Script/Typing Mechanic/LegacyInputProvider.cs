using UnityEngine;

namespace TypeForward
{
    /// <summary>
    /// IInputProvider backed by Unity's legacy Input class (Project Settings > Active Input Handling
    /// must include "Input Manager (Old)"). Swap this out for a New-Input-System provider without
    /// touching TypingSession or the controller.
    /// </summary>
    public class LegacyInputProvider : IInputProvider
    {
        public string ConsumeFrameInput() => Input.inputString;

        public bool WasBackspacePressedThisFrame() => Input.GetKeyDown(KeyCode.Backspace);
    }
}

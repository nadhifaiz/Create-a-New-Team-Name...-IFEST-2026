using UnityEngine;

namespace TypeForward
{
    public class LegacyInputProvider : IInputProvider
    {
        public string ConsumeFrameInput() => Input.inputString;

        public bool WasBackspacePressedThisFrame() => Input.GetKeyDown(KeyCode.Backspace);
    }
}

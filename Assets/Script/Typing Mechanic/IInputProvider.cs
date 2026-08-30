namespace TypeForward
{
    /// <summary>
    /// Abstraction over "where keystrokes come from" so TypingSession/controller
    /// never touch UnityEngine.Input directly. Makes it trivial to swap in the
    /// new Input System, a virtual on-screen keyboard, or a scripted test driver.
    /// </summary>
    public interface IInputProvider
    {
        /// <summary>Characters typed this frame, in order (letters, digits, punctuation, space).</summary>
        string ConsumeFrameInput();

        /// <summary>True on the frame Backspace was pressed.</summary>
        bool WasBackspacePressedThisFrame();
    }
}

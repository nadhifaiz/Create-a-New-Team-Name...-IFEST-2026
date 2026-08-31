namespace TypeForward
{

    public interface IInputProvider
    {
        string ConsumeFrameInput();
        bool WasBackspacePressedThisFrame();
    }
}
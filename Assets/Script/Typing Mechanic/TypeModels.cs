namespace TypeForward
{
    /// <summary>Visual state of a single character, decided by the Controller, drawn by the View.</summary>
    public enum CharState { Pending, Correct, Incorrect, Cursor, Extra }

    /// <summary>One character + how it should be colored. Immutable — View never mutates it.</summary>
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
}
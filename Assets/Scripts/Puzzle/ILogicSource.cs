namespace Puzzle
{
    /// <summary>
    /// Represents a source of a boolean state (active/inactive) that can be used
    /// as an input for logic gates or other puzzle elements.
    /// </summary>
    public interface ILogicSource
    {
        /// <summary>
        /// Gets a value indicating whether the logic source is currently in an "active" state.
        /// </summary>
        bool IsActive { get; }
    }
}
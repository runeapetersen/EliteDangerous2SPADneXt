using System.Collections.Generic;
using EliteDangerous2SPADneXt.GameState;

namespace EliteDangerous2SPADneXt.ChangeHandling
{
    /// <summary>
    /// Defines a handler for processing status updates and identifying changes in the game state.
    /// </summary>
    /// <remarks>
    /// Implementations of this interface are responsible for analyzing a given status update
    /// and returning a collection of changes encapsulated as <see cref="ChangedValue"/> objects.
    /// These changes can represent modifications to various game state properties, enabling
    /// change tracking and subsequent handling.
    /// </remarks>
    public interface IStatusUpdateHandler
    {
        IEnumerable<ChangedValue> HandleUpdate(Status status);
    }
}
using Content.Shared._DEN.Requirements.PlayerRequirements;
using Robust.Shared.Player;

namespace Content.Shared._DEN.Requirements.Managers;

/// <summary>
///     A manager used to check player stats against a list of requirements, getting the pass/fail status of these requirements.
///     This can be used to apply restrictions to character actions, like jobs or traits.
/// </summary>
public interface IPlayerRequirementManager
{
    /// <summary>
    ///     Creates a new PlayerRequirementContext with context fields pre-filled.
    /// </summary>
    /// <param name="session">The session associated with this player.</param>
    /// <returns>A pre-filled requirement context for this player.</returns>
    PlayerRequirementContext GetPlayerContext(ICommonSession session);
}

using Content.Shared.Players.PlayTimeTracking;
using Content.Shared.Preferences;
using Robust.Shared.Prototypes;

namespace Content.Shared._DEN.Requirements.Managers;

/// <summary>
///     A record that represents a list of factors that will be checked against PlayerRequirements.
///     All fields are nullable. A null field represents an optional parameter.
/// </summary>
public partial record PlayerRequirementContext
{
    /// <summary>
    ///     The currently-selected character profile of the player we are going to check.
    /// </summary>
    public HumanoidCharacterProfile? Profile = null;

    /// <summary>
    ///     A dictionary of registered playtimes for the player.
    /// </summary>
    public IReadOnlyDictionary<string, TimeSpan>? Playtimes = null;
}

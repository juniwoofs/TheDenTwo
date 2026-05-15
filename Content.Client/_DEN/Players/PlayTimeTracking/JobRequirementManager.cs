using Content.Shared._DEN.Requirements.Managers;
using Content.Shared._DEN.Requirements.PlayerRequirements;
using Content.Shared.Preferences;
using Content.Shared.Roles;

namespace Content.Client.Players.PlayTimeTracking;

public sealed partial class JobRequirementsManager
{
    /// <summary>
    ///     Check if we pass a given list of requirements.
    ///     A profile may be optionally supplied to replace the one in the context.
    /// </summary>
    /// <param name="profile">A profile associated with the character we're loading in.</param>
    /// <param name="requirements">The requirements to check.</param>
    /// <param name="The context we generated to check requirements.</param>
    /// <returns>Whether we pass the requirements given.</returns>
    private bool PassesRequirements(HumanoidCharacterProfile? profile,
        List<IPlayerRequirement> requirements,
        out PlayerRequirementContext context)
    {
        var session = _playerManager.LocalSession;
        context = session != null
            ? _requirements.GetPlayerContext(session)
            : new();

        if (profile != null)
            context.Profile = profile;

        return SharedPlayerRequirementManager.CheckRequirements(context, requirements);
    }
}

using System.Diagnostics.CodeAnalysis;
using Content.Shared._DEN.Requirements.Managers;
using Content.Shared._DEN.Requirements.PlayerRequirements;
using Content.Shared.Preferences;
using Content.Shared.Preferences.Loadouts;
using Content.Shared.Preferences.Loadouts.Effects;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Shared._DEN.Preferences.Loadouts.Effects;

/// <summary>
/// Checks for a player requirement to be met.
/// </summary>
public sealed partial class PlayerRequirementLoadoutEffect : LoadoutEffect
{
    [DataField(required: true)]
    public IPlayerRequirement Requirement = default!;

    public override bool Validate(HumanoidCharacterProfile profile,
        RoleLoadout loadout,
        ICommonSession? session,
        IDependencyCollection collection,
        [NotNullWhen(false)] out FormattedMessage? reason)
    {
        if (session == null
            // Auto-pass playtime requirements if they're disabled
            || Requirement is PlayerPlaytimeRequirement playtimeReq && playtimeReq.ShouldAutoPass())
        {
            reason = FormattedMessage.Empty;
            return true;
        }

        var requirements = collection.Resolve<IPlayerRequirementManager>();
        var context = requirements.GetPlayerContext(session);
        context.Profile = profile;
        var success = SharedPlayerRequirementManager.CheckRequirement(context, Requirement);

        if (!success)
        {
            var reasonText = Requirement.GetReason(context) ?? string.Empty;
            reason = FormattedMessage.FromMarkupPermissive(reasonText);
            return false;
        }

        reason = null;
        return true;
    }
}

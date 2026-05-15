using System.Diagnostics.CodeAnalysis;
using Content.Shared._DEN.Requirements.Managers;
using Content.Shared.CCVar;
using Content.Shared.Localizations;
using Content.Shared.Players.PlayTimeTracking;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Shared._DEN.Requirements.PlayerRequirements;

/// <summary>
///     An abstract class for playtime requirements that expect a playtime to be within
///     optional minimum and maximum parameters.
/// </summary>
public abstract partial class PlayerPlaytimeRequirement : PlayerRequirement, IPlayerRangeRequirement<TimeSpan>
{
    /// <summary>
    ///     The minimum time you can have in this tracker.
    /// </summary>
    [DataField("minTime")]
    public TimeSpan? Min { get; set; } = null;

    /// <summary>
    ///     The maximum time you can have in this tracker.
    /// </summary>
    [DataField("maxTime")]
    public TimeSpan? Max { get; set; } = null;

    /// <summary>
    ///     The "type" of playtime requirement this is.
    ///     This affects what CVAR is used to turn the timer off.
    /// </summary>
    [DataField]
    public PlaytimeRequirementType RequirementType = PlaytimeRequirementType.Role;

    /// <inheritdoc/>
    public override bool PreCheck(PlayerRequirementContext context)
    {
        // We are always returning "true" if ShouldAutoPass() is true, because otherwise, if the
        // pre-check failed, then it would be possible to fail this requirement as per
        // PlayerRequirement.MustPassPreCheck even when role timers should be ignored anyway.
        return ShouldAutoPass() || context.Playtimes != null;
    }

    /// <inheritdoc/>
    public override bool CheckRequirement(PlayerRequirementContext context)
    {
        // Auto-pass if role timers are disabled.
        if (ShouldAutoPass())
            return true;

        var playtime = GetPlaytime(context);
        if (playtime is null)
            return false;

        return IsInRange(playtime.Value);
    }

    /// <inheritdoc/>
    public override string? GetReason(PlayerRequirementContext? context = null)
    {
        // Do not give a reason if role timers are disabled.
        if (ShouldAutoPass())
            return null;

        // Get the playtime constraint string.
        if (!TryGetRangeConstraintReason(out var constraintReason))
            return null;

        // If there's no "difference reason", then just the constraint is fine.
        // "Must have 2h of playtime overall."
        if (!TryGetRangeDifferenceReason(out var differenceReason, context))
            return constraintReason;

        // "Must have 2h of playtime overall. (Need 1h more)"
        return Loc.GetString("player-requirement-range-with-difference",
            ("range", constraintReason),
            ("difference", differenceReason));
    }

    /// <summary>
    ///     Whether or not this requirement should auto-pass. This applies if role timers
    ///     are disabled, because playtimes shouldn't matter anyway in this case - we shouldn't
    ///     fail playtime requirements ever when role timers are disabled.
    /// </summary>
    /// <returns>
    ///     Whether or not this requirement should auto-pass.
    /// </returns>
    public bool ShouldAutoPass()
    {
        var config = IoCManager.Resolve<IConfigurationManager>();
        var timerEnabled = RequirementType switch
        {
            PlaytimeRequirementType.Role => config.GetCVar(CCVars.GameRoleTimers),
            PlaytimeRequirementType.Loadout => config.GetCVar(CCVars.GameRoleLoadoutTimers),
            _ => throw new ArgumentOutOfRangeException(nameof(RequirementType)),
        };

        return !timerEnabled;
    }

    /// <summary>
    ///     Format a playtime TimeSpan into text to display to the player.
    /// </summary>
    /// <param name="playtime">The playtime to format.</param>
    /// <returns>The formatted playtime, if playtime is not null.</returns>
    private static string FormatPlaytime(TimeSpan playtime)
    {
        var playtimeString = ContentLocalizationManager.FormatPlaytime(playtime);
        return Loc.GetString("player-requirement-format-time",
            ("playtime", playtimeString));
    }

    /// <summary>
    ///     Check if the given playtime is in range.
    /// </summary>
    /// <param name="playtime">The playtime to check.</param>
    /// <returns>Whether or not the playtime is in range.</returns>
    protected bool IsInRange(TimeSpan playtime)
    {
        if (this is IPlayerRangeRequirement<TimeSpan> range)
            return range.IsInRange(playtime);

        return false;
    }

    /// <inheritdoc />
    public string FormatValue(TimeSpan value)
    {
        return FormatPlaytime(value);
    }

    /// <inheritdoc />
    public TimeSpan? GetDifference(TimeSpan value)
    {
        if (this is IPlayerRangeRequirement<TimeSpan> range && range.IsInRange(value))
            return null;

        // Negative value = greater than maximum
        if (Max != null && value > Max)
            return Max - value;

        // Positive value = less than minimum
        if (Min != null && value < Min)
            return Min - value;

        return null;
    }

    /// <inheritdoc />
    public int Sign(TimeSpan difference)
    {
        return Math.Sign(difference.TotalSeconds);
    }

    /// <inheritdoc />
    public string FormatDifferenceText(TimeSpan difference)
    {
        var diffValue = difference.Duration();
        return FormatValue(diffValue);
    }

    /// <summary>
    ///     Get the text to display to the player that represents the range of valid playtimes.
    /// </summary>
    /// <param name="playtimeString">The playtime range description.</param>
    /// <returns>Whether or not this operation was successful.</returns>
    protected virtual bool TryGetRangeConstraintReason([NotNullWhen(true)] out string? playtimeString)
    {
        playtimeString = null;

        if (this is IPlayerRangeRequirement<TimeSpan> range)
        {
            var constraintReason = range.GetRangeConstraintReason();
            playtimeString = Loc.GetString("player-requirement-playtime-constraint-reason",
                ("inverted", Inverted),
                ("constraint", constraintReason));
        }

        return playtimeString != null;
    }

    /// <summary>
    ///     Get the text to display to the player that represents the difference between their
    ///     playtime and the upper/lower bound of acceptable playtimes.
    /// </summary>
    /// <param name="reason">A string representing the player's playtime in relation to the requirement's bounds.</param>
    /// <param name="context">A context that may contain this character's playtimes.</param>
    /// <returns>Whether or not this operation was successful.</returns>
    protected virtual bool TryGetRangeDifferenceReason([NotNullWhen(true)] out string? reason,
        PlayerRequirementContext? context = null)
    {
        reason = null;

        // TODO DEN: I'm just excluding this if it's inverted,
        // because otherwise it can get really confusing. You probably
        // should not be using inverted playtime requirements anyway.
        if (context?.Profile == null || Inverted)
            return false;

        if (this is IPlayerRangeRequirement<TimeSpan> range)
        {
            var playtime = GetPlaytime(context);
            if (playtime != null)
                reason = range.GetDifferenceReason(playtime.Value);
        }

        return reason != null;
    }

    /// <summary>
    ///     Get the playtime associated with this requirement.
    /// </summary>
    /// <param name="context">The context that may or may not contain playtimes.</param>
    /// <returns>The playtime associated with this requirement.</returns>
    protected abstract TimeSpan? GetPlaytime(PlayerRequirementContext context);
}

[Serializable]
public enum PlaytimeRequirementType
{
    Role,
    Loadout
}

/// <summary>
///     Checks if a player's total playtime in a given department fits within a given playtime range.
/// </summary>
public sealed partial class PlayerDepartmentPlaytimeRequirement : PlayerPlaytimeRequirement
{
    /// <summary>
    ///     The department we should check against the requirement.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<DepartmentPrototype> Department = default!;

    /// <inheritdoc />
    protected override TimeSpan? GetPlaytime(PlayerRequirementContext context)
    {
        return GetDepartmentPlaytime(context);
    }

    /// <inheritdoc />
    protected override bool TryGetRangeConstraintReason([NotNullWhen(true)] out string? playtimeString)
    {
        if (!base.TryGetRangeConstraintReason(out playtimeString))
            return false;

        var protoMan = IoCManager.Resolve<IPrototypeManager>();
        var deptName = FormatDepartment(protoMan);

        // E.g. "You must have 2h30m of playtime in the Science department."
        playtimeString = Loc.GetString("player-requirement-department-playtime-reason",
            ("constraint", playtimeString),
            ("department", deptName));

        return true;
    }

    /// <summary>
    ///     Format this requirement's department name, with a color.
    /// </summary>
    /// <param name="protoMan">The prototype manager.</param>
    /// <returns>The department name of this prototype, formatted.</returns>
    private string FormatDepartment(IPrototypeManager protoMan)
    {
        if (!protoMan.Resolve(Department, out var department))
            return Department;

        var deptName = Loc.GetString(department.Name);
        var deptColor = department.Color.ToHex();
        var formattedDept = Loc.GetString("player-requirement-format-department",
            ("color", deptColor),
            ("department", deptName));

        return formattedDept;
    }

    /// <summary>
    ///     Get the total playtime for this department.
    /// </summary>
    /// <param name="context">A definition of parameters to check against the requirement.</param>
    /// <returns>The total playtime of this department. Null if either context playtimes or department is invalid.</returns>
    private TimeSpan? GetDepartmentPlaytime(PlayerRequirementContext context)
    {
        var protoMan = IoCManager.Resolve<IPrototypeManager>();
        var playtime = TimeSpan.Zero;

        if (context.Playtimes == null
            || !protoMan.Resolve(Department, out var department))
            return null;

        // Sum the playtimes of all roles in this department.
        foreach (var roleId in department.Roles)
        {
            if (!protoMan.TryIndex(roleId, out var role))
                continue;

            if (!context.Playtimes.TryGetValue(role.PlayTimeTracker, out var roleTime))
                continue;

            playtime += roleTime;
        }

        return playtime;
    }
}

/// <summary>
///     Checks if a player's total playtime in a given job fits within a given playtime range.
/// </summary>
public sealed partial class PlayerJobPlaytimeRequirement : PlayerPlaytimeRequirement
{
    /// <summary>
    ///     The job we should check against the requirement.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<JobPrototype> Job = default!;

    /// <inheritdoc />
    protected override TimeSpan? GetPlaytime(PlayerRequirementContext context)
    {
        return GetJobPlaytime(context);
    }

    /// <inheritdoc />
    protected override bool TryGetRangeConstraintReason([NotNullWhen(true)] out string? playtimeString)
    {
        if (!base.TryGetRangeConstraintReason(out playtimeString))
            return false;

        var protoMan = IoCManager.Resolve<IPrototypeManager>();
        var jobName = FormatJob(protoMan);

        // E.g. "You must have 2h30m of playtime as a Mime."
        playtimeString = Loc.GetString("player-requirement-job-playtime-reason",
            ("constraint", playtimeString),
            ("job", jobName));

        return true;
    }

    /// <summary>
    ///     Format this requirement's job name, with a color.
    /// </summary>
    /// <param name="protoMan">The prototype manager.</param>
    /// <returns>The department name of this prototype, formatted.</returns>
    private string FormatJob(IPrototypeManager protoMan)
    {
        if (!protoMan.Resolve(Job, out var job))
            return Job;

        var jobName = Loc.GetString(job.Name);

        // Gotta use the department to recolor this role's name.
        var entMan = IoCManager.Resolve<EntityManager>();
        var jobSystem = entMan.System<SharedJobSystem>();
        var deptColor = Color.LightGray.ToHex();
        if (jobSystem.TryGetPrimaryDepartment(Job, out var dept) || jobSystem.TryGetDepartment(Job, out dept))
            deptColor = dept.Color.ToHex();

        var formattedJob = Loc.GetString("player-requirement-format-job",
            ("color", deptColor),
            ("job", jobName));

        return formattedJob;
    }

    /// <summary>
    ///     Get the total playtime for this job.
    /// </summary>
    /// <param name="context">A definition of parameters to check against the requirement.</param>
    /// <returns>The total playtime of this job. Null if either context playtimes or job is invalid.</returns>
    private TimeSpan? GetJobPlaytime(PlayerRequirementContext context)
    {
        var protoMan = IoCManager.Resolve<IPrototypeManager>();
        var playtime = TimeSpan.Zero;

        if (context.Playtimes == null || !protoMan.Resolve(Job, out var job))
            return null;

        if (context.Playtimes.TryGetValue(job.PlayTimeTracker, out var tracker))
            playtime = tracker;

        return playtime;
    }
}

/// <summary>
///     Checks if a player's total overall playtime fits within a given playtime range.
/// </summary>
public sealed partial class PlayerOverallPlaytimeRequirement : PlayerPlaytimeRequirement
{
    /// <inheritdoc />
    protected override TimeSpan? GetPlaytime(PlayerRequirementContext context)
    {
        return GetOverallPlaytime(context);
    }

    /// <inheritdoc />
    protected override bool TryGetRangeConstraintReason([NotNullWhen(true)] out string? playtimeString)
    {
        if (!base.TryGetRangeConstraintReason(out playtimeString))
            return false;

        // E.g. "You must have 300h of playtime overall."
        playtimeString = Loc.GetString("player-requirement-overall-playtime-reason",
            ("constraint", playtimeString));

        return true;
    }

    /// <summary>
    ///     Get the overall playtime for this context.
    /// </summary>
    /// <param name="context">The context being used for checking this requirement.</param>
    /// <returns>The player's overall playtime.</returns>
    private static TimeSpan? GetOverallPlaytime(PlayerRequirementContext context)
    {
        var overallTracker = PlayTimeTrackingShared.TrackerOverall;
        var playtime = TimeSpan.Zero;

        if (context.Playtimes == null)
            return null;

        if (context.Playtimes.TryGetValue(overallTracker, out var tracker))
            playtime = tracker;

        return playtime;
    }
}

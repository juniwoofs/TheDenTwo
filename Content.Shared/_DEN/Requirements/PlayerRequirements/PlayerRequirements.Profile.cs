using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared._DEN.Requirements.Managers;
using Content.Shared._DEN.Traits.Prototypes;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._DEN.Requirements.PlayerRequirements;

/// <summary>
///     Checks if a player's character is within the given age range.
/// </summary>
public sealed partial class PlayerAgeRequirement : PlayerRequirement, IPlayerRangeRequirement<int>
{
    /// <summary>
    ///     Minimum age of the character to pass the requirement.
    /// </summary>
    [DataField]
    public int? Min { get; set; } = null;

    /// <summary>
    ///     Maximum age of the character to pass the requirement.
    /// </summary>
    [DataField]
    public int? Max { get; set; } = null;

    /// <inheritdoc/>
    public override bool PreCheck(PlayerRequirementContext context)
    {
        return context.Profile != null;
    }

    /// <inheritdoc/>
    public override bool CheckRequirement(PlayerRequirementContext context)
    {
        if (context.Profile == null)
            return false;

        if (this is IPlayerRangeRequirement<int> range)
            return range.IsInRange(context.Profile.Age);

        return false;
    }

    /// <inheritdoc/>
    public override string? GetReason(PlayerRequirementContext? context = null)
    {
        if (!TryGetRangeConstraintReason(out var constraintReason))
            return null;

        // "Must be between 20 and 40 years old."
        var rangeReason = Loc.GetString("player-requirement-age-reason",
            ("inverted", Inverted),
            ("constraint", constraintReason));

        if (!TryGetRangeDifferenceReason(out var differenceReason, context))
            return rangeReason;

        return Loc.GetString("player-requirement-range-with-difference",
            ("range", rangeReason),
            ("difference", differenceReason));
    }

    /// <summary>
    ///     Get the text to display to the player that represents the range of valid playtimes.
    /// </summary>
    /// <param name="reason">The age range description.</param>
    /// <returns>Whether or not this operation was successful.</returns>
    private bool TryGetRangeConstraintReason([NotNullWhen(true)] out string? reason)
    {
        reason = null;

        if (this is IPlayerRangeRequirement<int> range)
            reason = range.GetRangeConstraintReason();

        return reason != null;
    }

    /// <summary>
    ///     Get the text to display to the player that represents the difference between their
    ///     character's age and the upper/lower bound of acceptable ages.
    /// </summary>
    /// <param name="reason">A string representing this character's age in relation to the requirement's bounds.</param>
    /// <param name="context">A context that may contain this character's profile.</param>
    /// <returns>Whether or not this operation was successful.</returns>
    private bool TryGetRangeDifferenceReason([NotNullWhen(true)] out string? reason,
        PlayerRequirementContext? context = null)
    {
        reason = null;

        // TODO DEN: I'm just excluding this if it's inverted,
        // because otherwise it can get really confusing. You probably
        // should not be using inverted age requirements anyway.
        if (context?.Profile == null || Inverted)
            return false;

        var age = context.Profile.Age;
        if (this is IPlayerRangeRequirement<int> range)
            reason = range.GetDifferenceReason(age);

        return reason != null;
    }

    /// <inheritdoc />
    public string FormatValue(int value)
    {
        return Loc.GetString("player-requirement-format-number",
            ("number", value.ToString()));
    }

    /// <inheritdoc/>
    public int? GetDifference(int value)
    {
        if (this is IPlayerRangeRequirement<int> range && range.IsInRange(value))
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
    public int Sign(int difference)
    {
        return Math.Sign(difference);
    }

    /// <inheritdoc />
    public string FormatDifferenceText(int difference)
    {
        var diffValue = Math.Abs(difference);
        return FormatValue(diffValue);
    }
}

/// <summary>
///     Checks if a player's character is one of the following species.
/// </summary>
public sealed partial class PlayerSpeciesRequirement : PlayerRequirement
{
    /// <summary>
    ///     Character's profile must be one of these species to pass the requirement.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<SpeciesPrototype>> Species = new();

    /// <inheritdoc/>
    public override bool PreCheck(PlayerRequirementContext context)
    {
        return context.Profile != null;
    }

    /// <inheritdoc/>
    public override bool CheckRequirement(PlayerRequirementContext context)
    {
        if (context.Profile == null)
            return false;

        return Species.Contains(context.Profile.Species);
    }

    /// <inheritdoc/>
    public override string? GetReason(PlayerRequirementContext? context = null)
    {
        var protoMan = IoCManager.Resolve<IPrototypeManager>();

        // Add all species names to a list
        var speciesList = new List<string>();
        foreach (var speciesId in Species)
            speciesList.Add(LocalizeSpecies(speciesId, protoMan));

        var joinedSpecies = string.Join(", ", speciesList);

        // "Must be one of these species: Human, Dwarf, Slimeperson"
        return Loc.GetString("player-requirement-species-reason",
            ("inverted", Inverted),
            ("species", joinedSpecies));
    }

    /// <summary>
    ///     Localizes a species ID into a formatted species name.
    /// </summary>
    /// <param name="speciesId">The ID of the species.</param>
    /// <param name="protoMan">The prototype manager.</param>
    /// <returns>A formatted species name string.</returns>
    private static string LocalizeSpecies(ProtoId<SpeciesPrototype> speciesId, IPrototypeManager protoMan)
    {
        var speciesName = speciesId;
        if (!protoMan.Resolve(speciesId, out var species))
            return speciesName;

        speciesName = Loc.GetString(species.Name);
        return Loc.GetString("player-requirement-format-species", ("species", speciesName));
    }
}

/// <summary>
///     Checks if a player's character has a required number of the given traits.
/// </summary>
public sealed partial class PlayerTraitRequirement : PlayerRequirement
{
    /// <summary>
    ///     Traits that the character needs to have to the pass the requirement.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<EntityTraitPrototype>> Traits = new();

    [DataField]
    public CountRequirement Count;

    /// <inheritdoc/>
    public override bool PreCheck(PlayerRequirementContext context)
    {
        return context.Profile != null;
    }

    /// <inheritdoc/>
    public override bool CheckRequirement(PlayerRequirementContext context)
    {
        if (context.Profile == null)
            return false;

        var profileTraits = context.Profile.EntityTraitPreferences;
        return Count.CheckRequirement(profileTraits, Traits);
    }

    /// <inheritdoc/>
    public override string? GetReason(PlayerRequirementContext? context = null)
    {
        var protoMan = IoCManager.Resolve<IPrototypeManager>();
        var traitNames = Traits.Select(t => LocalizeTrait(t, protoMan));
        var traitList = string.Join(", ", traitNames);
        var constraintReason = Count.GetReason();

        return Loc.GetString("player-requirement-trait-reason",
            ("inverted", Inverted),
            ("constraint", constraintReason),
            ("traits", traitList));
    }

    /// <summary>
    ///     Localizes a trait ID into a formatted trait name.
    /// </summary>
    /// <param name="traitId">The ID of the trait.</param>
    /// <param name="protoMan">The prototype manager.</param>
    /// <returns>A formatted trait name string.</returns>
    private static string LocalizeTrait(ProtoId<EntityTraitPrototype> traitId, IPrototypeManager protoMan)
    {
        var traitName = traitId;

        if (protoMan.Resolve(traitId, out var trait))
            traitName = Loc.GetString(trait.Name);

        return Loc.GetString("player-requirement-format-trait", ("trait", traitName));
    }
}

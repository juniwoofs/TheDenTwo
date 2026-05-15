using System.Linq;
using Content.Shared._DEN.Requirements.Managers;
using JetBrains.Annotations;

namespace Content.Shared._DEN.Requirements.PlayerRequirements;

/// <summary>
///     An abstract class for requirements to determine how many items in a set should be selected.
/// </summary>
/// <remarks>
///     For example: A player selecting multiple traits that overlap with a certain required group of traits.
/// </remarks>
[ImplicitDataDefinitionForInheritors]
[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract partial class CountRequirement
{
    /// <summary>
    ///     Gets a string reason representation for this range.
    /// </summary>
    /// <remarks>
    ///     This would slot into the sentence "Must have [reason] of the following items: [items]".
    /// </remarks>
    /// <example>"At least 1", "between 2 and 5", "all"</example>
    /// <param name="context">An optional context used to add additional information to the reason.</param>
    /// <returns>A string representation of this range's requirement bounds.</returns>
    public abstract string GetReason();

    /// <summary>
    ///     Check if our currently-selected items in a collection meets this
    ///     requirement, against a collection of required items.
    /// </summary>
    /// <typeparam name="T">The type of the collection.</typeparam>
    /// <param name="have">The items we have selected.</param>
    /// <param name="required">The items required for this condition.</param>
    /// <returns>Whether or not we fulfill this requirement.</returns>
    public abstract bool CheckRequirement<T>(IEnumerable<T> have, IEnumerable<T> required);

    /// <summary>
    ///     Get how many of the required items in a collection we currently have.
    /// </summary>
    /// <typeparam name="T">The type of the collection.</typeparam>
    /// <param name="have">The items we have selected.</param>
    /// <param name="required">The items required for this condition.</param>
    /// <returns>How many of the required items we have.</returns>
    protected static int GetFulfilledCount<T>(IEnumerable<T> have, IEnumerable<T> required)
    {
        return have.Intersect(required).Count();
    }
}

/// <summary>
///     To fulfill this requirement, you must have exactly some number of items.
/// </summary>
public sealed partial class ConstantCountRequirement : CountRequirement
{
    /// <summary>
    ///     How many items in the collection you need to pass the requirement.
    /// </summary>
    [DataField]
    public int Count;

    /// <inheritdoc />
    public override string GetReason()
    {
        // "Must have exactly 1 of the following items."
        return Loc.GetString("count-requirement-constant-reason",
            ("count", Count));
    }

    /// <inheritdoc />
    public override bool CheckRequirement<T>(IEnumerable<T> have, IEnumerable<T> required)
    {
        var count = GetFulfilledCount(have, required);
        return count == Count;
    }
}

/// <summary>
///     To fulfill this requirement, you must have a number of items between two values, or either a minimum / maximum.
/// </summary>
public sealed partial class RangeCountRequirement : CountRequirement, IPlayerRangeRequirement<int>
{
    /// <summary>
    ///     Minimum amount of required items you need. Null means no minimum constraint.
    /// </summary>
    [DataField]
    public int? Min { get; set; } = null;

    /// <summary>
    ///     Maximum amount of required items you can have. Null means no maximum constraint.
    /// </summary>
    [DataField]
    public int? Max { get; set; } = null;

    /// <inheritdoc />
    public override string GetReason()
    {
        if (this is IPlayerRangeRequirement<int> range)
            return range.GetRangeConstraintReason();

        return string.Empty;
    }

    /// <inheritdoc />
    public override bool CheckRequirement<T>(IEnumerable<T> have, IEnumerable<T> required)
    {
        var count = GetFulfilledCount(have, required);

        if (this is IPlayerRangeRequirement<int> range)
            return range.IsInRange(count);

        return false;
    }

    /// <inheritdoc />
    public string FormatValue(int value)
    {
        return Loc.GetString("player-requirement-format-number",
            ("number", value.ToString()));
    }

    /// <inheritdoc />
    public int? GetDifference(int value)
    {
        // Unimplemented - unused
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public int Sign(int difference)
    {
        // Unimplemented - unused
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public string FormatDifferenceText(int difference)
    {
        // Unimplemented - unused
        throw new NotImplementedException();
    }
}

/// <summary>
///     To fulfill the requirement, you must have at least one item in the required collection.
/// </summary>
/// <remarks>
///     This is similar to <see cref="RangeCountRequirement"/>, with a Min of 1, but it says "any" in the reason instead.
///     This sounds smoother for inverted requirements; i.e. "Must not have any of the following items."
/// </remarks>
public sealed partial class AnyCountRequirement : CountRequirement
{
    /// <inheritdoc />
    public override string GetReason()
    {
        // "Must have any of the following items."
        return Loc.GetString("count-requirement-any-reason");
    }

    /// <inheritdoc />
    public override bool CheckRequirement<T>(IEnumerable<T> have, IEnumerable<T> required)
    {
        var count = GetFulfilledCount(have, required);
        return count >= 1;
    }
}

/// <summary>
///     To fulfill the requirement, you must have all items in the required collection.
/// </summary>
public sealed partial class AllCountRequirement : CountRequirement
{
    /// <inheritdoc />
    public override string GetReason()
    {
        // "Must have all of the following items."
        return Loc.GetString("count-requirement-all-reason");
    }

    /// <inheritdoc />
    public override bool CheckRequirement<T>(IEnumerable<T> have, IEnumerable<T> required)
    {
        var count = GetFulfilledCount(have, required);
        return count == required.Count();
    }
}

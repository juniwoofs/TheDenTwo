using Content.Shared._DEN.Requirements.PlayerRequirements;
using Robust.Shared.Prototypes;

namespace Content.Shared.Roles;

/// <summary>
/// Collection of job, antag, and ghost-role job requirements for per-server requirement overrides.
/// </summary>
[Prototype]
public sealed partial class JobRequirementOverridePrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    [Obsolete("Use JobRequirements instead")] // DEN
    public Dictionary<ProtoId<JobPrototype>, HashSet<JobRequirement>> Jobs = new ();

    [DataField]
    [Obsolete("Use AntagRequirements instead")] // DEN
    public Dictionary<ProtoId<AntagPrototype>, HashSet<JobRequirement>> Antags = new ();

    // Begin DEN: Use PlayerRequirements

    /// <summary>
    ///     A dictionary of job roles mapped to a list of overriding requirements.
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<JobPrototype>, List<IPlayerRequirement>> JobRequirements = new();

    /// <summary>
    ///     A dictionary of antagonist roles mapped to a list of overriding requirements.
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<JobPrototype>, List<IPlayerRequirement>> AntagRequirements = new();

    // End DEN
}

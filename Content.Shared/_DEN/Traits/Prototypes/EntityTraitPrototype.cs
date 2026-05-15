using Content.Shared._DEN.Requirements.PlayerRequirements;
using Content.Shared._DEN.Traits.TraitFunctions;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Traits;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

#pragma warning disable IDE1006 // Naming Styles
namespace Content.Shared._DEN.Traits.Prototypes;
#pragma warning restore IDE1006 // Naming Styles

[Prototype]
public sealed partial class EntityTraitPrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// The name of this trait.
    /// </summary>
    [DataField]
    public LocId Name { get; private set; } = string.Empty;

    /// <summary>
    /// The description of this trait.
    /// </summary>
    [DataField]
    public LocId? Description { get; private set; }

    /// <summary>
    /// Don't apply this trait to entities this whitelist IS NOT valid for.
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist;

    /// <summary>
    /// Don't apply this trait to entities this whitelist IS valid for. (hence, a blacklist)
    /// </summary>
    [DataField]
    public EntityWhitelist? Blacklist;

    /// <summary>
    /// Trait Price. If negative number, points will be added.
    /// </summary>
    [DataField]
    public int Cost = 0;

    /// <summary>
    /// Adds a trait to a category, allowing you to limit the selection of some traits to the settings of that category.
    /// </summary>
    [DataField]
    public ProtoId<TraitCategoryPrototype>? Category;

    /// <summary>
    /// A list of functions associated with this trait.
    /// </summary>
    [DataField("functions")]
    public List<ITraitFunction> TraitFunctions = new();

    /// <summary>
    /// Whether or not this trait can be selected in the character editor.
    /// </summary>
    [DataField("characterEditorSelectable")]
    public bool Selectable = true;

    /// <summary>
    ///     A list of requirements to use this trait.
    /// </summary>
    [DataField]
    public List<IPlayerRequirement> Requirements = new();
}

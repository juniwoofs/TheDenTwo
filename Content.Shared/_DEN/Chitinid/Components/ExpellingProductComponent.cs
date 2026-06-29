using Robust.Shared.Prototypes;

namespace Content.Shared._DEN.Chitinid.Components;

/// <summary>
/// Handles spawning a prototype after a certain delay.
/// </summary>
[RegisterComponent, AutoGenerateComponentState]
public sealed partial class ExpellingProductComponent : Component
{
    /// <summary>
    /// The prototype to spawn.
    /// </summary>
    [DataField, AutoNetworkedField] public EntProtoId ProductPrototype;

    /// <summary>
    /// When the prototype should be spawned. This is a point in time, not an offset.
    /// </summary>
    [DataField, AutoNetworkedField] public TimeSpan FinishedExpelling;
}
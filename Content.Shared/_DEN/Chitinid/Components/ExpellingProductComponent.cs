using Robust.Shared.Prototypes;

namespace Content.Shared._DEN.Chitinid.Components;

[RegisterComponent, AutoGenerateComponentState]
public sealed partial class ExpellingProductComponent : Component
{
    [DataField, AutoNetworkedField] public EntProtoId ProductPrototype;

    [DataField, AutoNetworkedField] public TimeSpan FinishedExpelling;
}
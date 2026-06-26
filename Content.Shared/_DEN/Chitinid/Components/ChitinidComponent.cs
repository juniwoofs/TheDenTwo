using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DEN.Chitinid.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true), AutoGenerateComponentPause]
public sealed partial class ChitinidComponent : Component
{
    [DataField, AutoNetworkedField] public EntProtoId ProductPrototype = "Chitzite";

    [DataField, AutoNetworkedField] public EntProtoId ExpulsionActionPrototype = "ActionChitzite";

    [DataField, AutoNetworkedField] public SoundSpecifier ActionSound = new SoundPathSpecifier("/Audio/Animals/cat_hiss.ogg");

    [DataField, AutoNetworkedField] public EntityUid? ActionEntity;

    [DataField, AutoNetworkedField] public DamageSpecifier Healing = new()
    {
        DamageDict = new Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2>
        {
            { "Radiation", -0.5f },
        }
    };

    [DataField, AutoNetworkedField] public TimeSpan UpdateInterval = TimeSpan.FromSeconds(1);
    
    [DataField, AutoNetworkedField, AutoPausedField]
    public TimeSpan NextUpdate;
    
    [DataField, AutoNetworkedField, AutoPausedField]
    public TimeSpan ExpulsionTime = TimeSpan.FromSeconds(2.15f);
    
    [DataField, AutoNetworkedField] public FixedPoint2 MaximumAbsorbed = 30.0f;

    [DataField, AutoNetworkedField] public FixedPoint2 TotalAbsorbed = 0.0f;
}
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DEN.Chitinid.Components;

/// <summary>
/// Allows an entity to heal a certain amount of damage up to a maximum amount. When the maximum amount is reached the
/// associated action is given a charge. The action is in charge of resetting TotalAbsorbed to allow healing to resume.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true), AutoGenerateComponentPause]
public sealed partial class ChitinidComponent : Component
{
    /// <summary>
    /// The EntProtoId to spawn with the expulsion action.
    /// </summary>
    [DataField, AutoNetworkedField] public EntProtoId ProductPrototype = "Chitzite";

    /// <summary>
    /// The action prototype to be granted to the entity that has this component.
    /// </summary>
    [DataField, AutoNetworkedField] public EntProtoId ExpulsionActionPrototype = "ActionChitzite";

    /// <summary>
    /// The sound to play when the action is performed.
    /// </summary>
    [DataField, AutoNetworkedField] public SoundSpecifier ActionSound = new SoundPathSpecifier("/Audio/Animals/cat_hiss.ogg");

    /// <summary>
    /// The action entity after it has been granted.
    /// </summary>
    [DataField, AutoNetworkedField] public EntityUid? ActionEntity;

    /// <summary>
    /// The DamageSpecifier used for healing, this occurs every <see cref="UpdateInterval"/>
    /// </summary>
    [DataField, AutoNetworkedField] public DamageSpecifier Healing = new()
    {
        DamageDict = new Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2>
        {
            { "Radiation", -0.5f },
        }
    };

    /// <summary>
    /// How often this component is updated, specifically the amount of time between each 'tick' of healing.
    /// </summary>
    [DataField, AutoNetworkedField] public TimeSpan UpdateInterval = TimeSpan.FromSeconds(1);
    
    /// <summary>
    /// When this component next needs to be updated.
    /// </summary>
    [DataField, AutoNetworkedField, AutoPausedField]
    public TimeSpan NextUpdate;
    
    /// <summary>
    /// The amount of time that the Expulsion action should take. Usually should be equal to length of <see cref="ActionSound"/>
    /// </summary>
    [DataField, AutoNetworkedField, AutoPausedField]
    public TimeSpan ExpulsionTime = TimeSpan.FromSeconds(2.15f);
    
    /// <summary>
    /// The maximum amount of damage that can be absorbed before needing to perform <see cref="ActionEntity"/>
    /// </summary>
    [DataField, AutoNetworkedField] public FixedPoint2 MaximumAbsorbed = 30.0f;

    /// <summary>
    /// The current amount that has been absorbed, building up towards the next charge of <see cref="ActionEntity"/>
    /// </summary>
    [DataField, AutoNetworkedField] public FixedPoint2 TotalAbsorbed = 0.0f;
}
using Content.Shared._DEN.Chitinid.Components;
using Content.Shared.Actions;
using Content.Shared.Charges.Systems;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Shared._DEN.Chitinid.EntitySystems;

/// <summary>
/// Handles healing of a particular damage type up to specified amount, as well as adding a charge to the associated
/// action when the limit is hit. Also provides handling for the ChitinidActionEvent, specifically, spawning a proto
/// and resetting the healed damage with the action.
/// </summary>
public sealed partial class ChitinidSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedChargesSystem _sharedCharges = default!;
    
    public override void Initialize()
    {
        base.Initialize();
        
        SubscribeLocalEvent<ChitinidComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ChitinidComponent, ChitinidActionEvent>(OnChitinidAction);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        // Check all the chitinid components and handle healing and recording damage amount, as well as setting the
        // action state.
        var damageQuery = EntityQueryEnumerator<ChitinidComponent, DamageableComponent>();
        while (damageQuery.MoveNext(out var uid, out var chitinid, out var damageable))
        {
            if (curTime < chitinid.NextUpdate)
                continue;

            chitinid.NextUpdate += chitinid.UpdateInterval;
            DirtyField(uid, chitinid, nameof(ChitinidComponent.NextUpdate));

            if (_mobState.IsDead(uid) || chitinid.TotalAbsorbed >= chitinid.MaximumAbsorbed)
                continue;
            
            if (_damageable.TryChangeDamage((uid, damageable), 
                    chitinid.Healing, 
                    out var delta, 
                    true, 
                    false))
            {
                chitinid.TotalAbsorbed += -delta.GetTotal();
                if (chitinid.ActionEntity is { } action && chitinid.TotalAbsorbed >= chitinid.MaximumAbsorbed)
                {
                    _sharedCharges.SetCharges(action, 1);
                }
            }
        }

        // Handle the time delay for spawning a component with ExpellingProductComponent
        var expulsionQuery = EntityQueryEnumerator<ExpellingProductComponent, ChitinidComponent>();
        while (expulsionQuery.MoveNext(out var uid, out var expulsion, out var chitinid))
        {
            if (curTime < expulsion.FinishedExpelling)
                continue;

            PredictedSpawnNextToOrDrop(expulsion.ProductPrototype, uid);
            chitinid.TotalAbsorbed = 0;
            RemCompDeferred(uid, expulsion);
        }
    }

    /// <summary>
    /// Initialize update times and add the action to the owner.
    /// </summary>
    private void OnMapInit(Entity<ChitinidComponent> entity, ref MapInitEvent evt)
    {
        entity.Comp.NextUpdate = _timing.CurTime + entity.Comp.UpdateInterval;
        var addedAction = _actions.AddAction(entity, entity.Comp.ExpulsionActionPrototype);
        if (addedAction is null)
        {
            Log.Warning($"Failed to add {entity.Comp.ExpulsionActionPrototype} to {ToPrettyString(entity)}");
            return;
        }

        entity.Comp.ActionEntity = addedAction;
    }

    /// <summary>
    /// Check if ingestion is blocked and then use the ExpellingProductComponent to delay item spawning until the sound
    /// is finished playing.
    /// </summary>
    private void OnChitinidAction(Entity<ChitinidComponent> entity, ref ChitinidActionEvent evt)
    {
        var attempt = new IngestionAttemptEvent(IngestionSystem.DefaultFlags);
        RaiseLocalEvent(entity, ref attempt);

        if (attempt.Cancelled && attempt.Blocker is {} blocker)
        {
            _popup.PopupClient(Loc.GetString("chitzite-mask", ("mask", blocker)), entity, entity);
            return;
        }
        
        _popup.PopupPredicted(Loc.GetString("chitzite-cough", ("name", Name(entity))), entity, entity);
        _audio.PlayPredicted(entity.Comp.ActionSound, entity, entity, AudioParams.Default.WithVariation(0.15f));

        var expulsion = EnsureComp<ExpellingProductComponent>(entity);
        expulsion.FinishedExpelling = _timing.CurTime + entity.Comp.ExpulsionTime;
        expulsion.ProductPrototype = entity.Comp.ProductPrototype;
        evt.Handled = true;
    }
}

/// <summary>
/// Sent by the Chitzite expulsion action to trigger the sound, entity spawning, and damage reset.
/// </summary>
public sealed partial class ChitinidActionEvent : InstantActionEvent;
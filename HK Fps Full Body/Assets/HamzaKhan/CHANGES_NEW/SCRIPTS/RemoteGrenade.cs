using UnityEngine.Events;
using UnityEngine;

public class RemoteGrenade : GrenadeItem, IDetonatable
{

    [Header("Remote Grenade Settings")]
    public GameObject RemoteGrenadeObject;

    private HKPlayerItemSystem _PlayerItemSystem;
    private GameObject _clonedGrenade;

    #region Overrides
    public override void StartUse(HKPlayerItemSystem controller)
    {
        ShowAimingIndicators(controller);  // Start aiming visuals.
    }

    public override void HoldUse(HKPlayerItemSystem controller)
    {
        UpdateAimingIndicators(controller);  // Keep updating the aiming visuals.
    }

    public override void ReleaseUse(HKPlayerItemSystem controller)
    {
        RemoteGrenadeObject.SetActive(true);

        ThrowGrenade(controller);

        HideAimingIndicators(controller);
    }

    public override void CancelUse(HKPlayerItemSystem controller)
    {
        HideAimingIndicators(controller);  // Cancel and hide aiming visuals.
    }

    public override bool CanInteract(HKPlayerInteractionBase interactionController)
    {
        return _canInteract;
    }

    public override void ThrowObj(HKPlayerItemSystem controller, Transform releasePosition, float force)
    {
        // Disable current grenade
        gameObject.SetActive(false);

        // Throw a cloned grenade.
        GameObject clonedGrenade = CloneThrowGrenade(UseSeperateThrownObject ? ThrownObject : gameObject, releasePosition.forward * force);

        _clonedGrenade = clonedGrenade;

        _PlayerItemSystem = controller;
    }

    public void TriggerDetonation()
    {
        if (_clonedGrenade == null) return;

        _PlayerItemSystem.OnUseComplete();

        ExplodeGrenadeObj(_clonedGrenade);
    }

    #endregion
}

using System.Collections;
using UnityEngine;

// CHANGES NEW SCRIPT
public class GrenadeItem : ItemBase
{
    public float explosionRadius;

    public Rigidbody _rb;

    public override void StartUse(PlayerController controller)
    {
        ShowAimingIndicators(controller);  // Start aiming visuals.
    }

    public override void HoldUse(PlayerController controller)
    {
        UpdateAimingIndicators(controller);  // Keep updating the aiming visuals.
    }

    public override void ReleaseUse(PlayerController controller)
    {
        ThrowGrenade(controller);  // Execute the throw when the button is released.
    }

    public override void CancelUse(PlayerController controller)
    {
        HideAimingIndicators(controller);  // Cancel and hide aiming visuals.
    }

    private void ShowAimingIndicators(PlayerController controller)
    {
        controller.ShowGrenadeIndicators(_rb);
    }

    private void UpdateAimingIndicators(PlayerController controller)
    {
        // Logic to update decal position and line trajectory
        controller.UpdateGrenadeIndicators(_rb);
    }

    private void HideAimingIndicators(PlayerController controller)
    {
        // Logic to hide the decal and trajectory preview.
        controller.HideGrenadeIndicators(_rb);
    }

    private void ThrowGrenade(PlayerController controller)
    {
        // Actual logic for throwing the grenade.
        controller.ThrowGrenade(_rb);
    }

    public void ExplodeGrenade()
    {
        // TODO Code in here to explode the grenade
        StartCoroutine(ExplodeGrenadeCoroutine());
    }

    private IEnumerator ExplodeGrenadeCoroutine()
    {
        yield return null;
    }
}
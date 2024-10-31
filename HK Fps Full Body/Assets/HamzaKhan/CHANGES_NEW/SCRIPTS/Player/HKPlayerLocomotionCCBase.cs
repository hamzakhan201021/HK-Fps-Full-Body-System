using UnityEngine;
using UnityEngine.Events;

public abstract class HKPlayerLocomotionCCBase : MonoBehaviour
{
    public abstract void MovePlayer(Vector2 moveInputVector, Vector2 lookInput, float targetUpperBodyLayerWeight,
        bool jumpPressed, bool sprintTriggered, bool hasStamina, bool crouchTriggered, bool proneTriggered, bool isCurrentWeaponRifle);

    public abstract void UpdateRotation(Vector2 lookInput, Vector2 moveInput, float leanInput,
        bool isInInteractionRange, bool reloading, bool holstered, bool performingSwitch, bool clipped);

    public abstract void MoveCamera(Vector3 cameraClippedPosition, Vector3 cameraAimedPosition, Vector3 cameraNormalPosition, bool holstered, bool performingSwitch, bool clipped, bool aimed);

    public abstract void OnPerformWeaponSwitchAnim(int animToPlayID);

    public abstract void OnPerformItemAnim(int itemID);

    public abstract void OnHoldItemAnim(int itemID);

    public abstract void OnCancelItemAnim();

    public abstract void OnPlayerDamage(float prevHealth, float newHealth);

    /// <summary>
    /// Returns true if the loco state is walking
    /// </summary>
    /// <returns></returns>
    public abstract bool IsWalking();

    /// <summary>
    /// Returns true if the loco state is sprinting
    /// </summary>
    /// <returns></returns>
    public abstract bool IsSprinting();

    /// <summary>
    /// Returns true if the loco state is crouching
    /// </summary>
    /// <returns></returns>
    public abstract bool IsCrouching();

    /// <summary>
    /// Returns true if the loco state is proning
    /// </summary>
    /// <returns></returns>
    public abstract bool IsProning();

    /// <summary>
    /// Is air state grounded ?
    /// </summary>
    /// <returns></returns>
    public abstract bool IsGrounded();

    public abstract float GetXRotation();

    public abstract void SetXRotation(float xRot);

    public abstract float GetLookUpLimit();

    public abstract float GetLookDownLimit();

    public abstract UnityEvent OnJumpEvent();

    public abstract UnityEvent<float> OnHitGroundEvent();
}

using UnityEngine;

public class PlayerModel : MonoBehaviour
{

    [Space]
    [Header("Main Settings")]
    [SerializeField] private AudioClip _footAudioClip;

    private PlayerController _playerController;

    void Start()
    {
        // Get Controller.
        _playerController = GetComponentInParent<PlayerController>();
    }

    // Animation Events
    #region Events

    /// <summary>
    /// Plays foot step sound.
    /// </summary>
    private void PlayFootSound()
    {
        // check if there is some movement input.
        if (_playerController.Input.Player.Move.ReadValue<Vector2>() != Vector2.zero)
        {
            // play the foot audio clip
            AudioSource.PlayClipAtPoint(_footAudioClip, transform.position, 1f);
        }
    }

    private void WeaponSwitchComplete()
    {
        _playerController.OnWeaponSwitchComplete();
    }

    private void Holster()
    {
        _playerController.Holster();
    }

    private void SwitchWeapon(WeaponSwitchType weaponSwitchType)
    {
        _playerController.SwitchWeapon(weaponSwitchType);
    }

    private void SnapCurrentWeaponToGunSlot()
    {
        _playerController.SnapCurrentWeaponToGunSlot();
    }

    private void SnapCurrentWeaponToRightHandAndAssignIKTargets()
    {
        _playerController.SnapCurrentWeaponToRightHandAndAssignIKTargets();
    }

    private void DropCurrentWeapon()
    {
        _playerController.DropCurrentWeapon();
    }

    private void PickNewWeapon()
    {
        _playerController.PickNewWeapon();
    }

    #endregion
}

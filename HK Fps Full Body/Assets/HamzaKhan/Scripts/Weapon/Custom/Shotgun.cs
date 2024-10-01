using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Shotgun : WeaponBase
{

    // Custom Shotgun Variables.
    [Space]
    [Header("Shotgun Custom")]

    [Space]
    [Header("Animation Names")]
    [SerializeField] private string _loadBulletFromSide = "";
    [SerializeField] private string _loadBulletFromBelowLoop = "";
    [SerializeField] private string _loadBulletFromBelow = "";

    [Space]
    [Header("Animation Time")]
    [SerializeField] private float _loadBulletFromSideAnimationTime;
    [SerializeField] private float _loadBulletFromBelowAnimationTime;
    [SerializeField] private float _loadBulletFromBelowLoopAnimationTime;

    [Space]
    [Header("Shotgun Audio Clips")]
    [SerializeField] private AudioClip _pushBulletInAudioClip;

    // This helps with reloading.
    private int _bulletsLoaded = 0;
    private int _bulletsToLoad = 0;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        // Call The Base classes update.
        base.Update();
    }

    /// <summary>
    /// Custom start reloading function, overrides the original one in the base class but still calls that one at the end,
    /// Instead of doing that functionality itself.
    /// </summary>
    public override void StartReloading()
    {
        // Return if the any of the Conditions meet.
        if (CurrentAmmo >= WeaponData.MagazineSize || _isReloading || _gunShotTimer > 0f) return;

        // Helper Variables.
        int bulletsDifferenceFromMagSize = WeaponData.MagazineSize - CurrentAmmo;
        bool hasEnoughBulletsToLoadComplete = bulletsDifferenceFromMagSize <= TotalAmmo;

        // Check if we have enough bullets to load complete
        if (hasEnoughBulletsToLoadComplete) _bulletsToLoad = bulletsDifferenceFromMagSize;
        else _bulletsToLoad = TotalAmmo;

        // Set the bullets loaded to 0.
        _bulletsLoaded = 0;

        // Check if the mag isnt empty.
        if (CurrentAmmo > 0)
        {
            // Calculate the Time for the weapon to wait until it's able to shoot again.
            WeaponData.ReloadingAnimationTime = (_loadBulletFromBelowLoopAnimationTime * (_bulletsToLoad - 1)) + _loadBulletFromBelowAnimationTime;

            // Play the load bullet from below animation.
            Animator.Play(_loadBulletFromBelow, 0, 0);
        }
        else
        {
            // Calculate the Time for the weapon to wait until it's able to shoot again.
            WeaponData.ReloadingAnimationTime = (_loadBulletFromBelowLoopAnimationTime * (_bulletsToLoad - 2)) + (_loadBulletFromBelowAnimationTime + _loadBulletFromSideAnimationTime);

            // Play the animation for Loading a bullet from the side.
            Animator.Play(_loadBulletFromSide, 0, 0);
        }

        // Base Reload, Instead of doing the base functionality in here, Just call the Base Function.
        base.StartReloading();
    }

    /// <summary>
    /// Called When the Animation that loads the bullet from the side is finished using an Animation Event.
    /// </summary>
    private void OnFinishedLoadBulletFromSide()
    {
        // Manage Bullets Loaded & Ammo.
        _bulletsLoaded += 1;
        CurrentAmmo += 1;
        TotalAmmo -= 1;

        // Check if we still need to load Bullets.
        if (_bulletsLoaded < _bulletsToLoad) Animator.Play(_loadBulletFromBelow, 0, 0);
    }

    /// <summary>
    /// Called When the Animation that loads the bullet from below is at the looping point using an Animation Event.
    /// </summary>
    private void OnFinishedLoadBulletFromBelow()
    {
        // Manage Bullets Loaded & Ammo.
        _bulletsLoaded += 1;
        CurrentAmmo += 1;
        TotalAmmo -= 1;

        // Check if we still need to load Bullets.
        if (_bulletsLoaded < _bulletsToLoad) Animator.Play(_loadBulletFromBelowLoop, 0, 0);
    }

    // Plays the bullet click audio clip.
    private void PlayPushBulletInAudioClip()
    {
        // Play the Bullet in audio clip.
        AudioSource.PlayClipAtPoint(_pushBulletInAudioClip, transform.position, 1.0f);
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(Shotgun))]
public class ShotgunEditor : WeaponBaseEditor
{
    // Just so our inspector becomes alright, And we get the nice button too!
}

#endif
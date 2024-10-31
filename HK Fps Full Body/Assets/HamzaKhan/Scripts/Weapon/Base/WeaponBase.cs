using System.Collections;
using UnityEngine.Events;
using UnityEngine;
using Cinemachine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Rigidbody))]
public class WeaponBase : MonoBehaviour, IInteractable
{

    #region Variables

    [Space]
    [Header("Main Settings")]
    public WeaponSO WeaponData;
    public AudioSource AudioSource;
    public ParticleSystem MuzzleFlashEffect;
    public ParticleSystem ShellEjectEffect;

    public Transform FirePoint;
    public Transform MagParent;

    public GameObject MagPrefab;

    public HandsConstraintType HandsConstraintType;

    public HandsIKTransform HandsIKTargets;
    public HandsRotationConstraintTransforms HandsRotationConstraintTransforms;

    public GameObject Mag;

    public int TotalAmmo;
    public int CurrentAmmo;

    public CinemachineImpulseSource RecoilImpulseSource;
    [Space]
    [Header("Behaviour Settings")]
    public bool UseSway = true;

    [Tooltip("Should this weapon be moved to lef")]
    public ItemStartUseBehaviour ItemUseBehaviour = ItemStartUseBehaviour.MoveToLeftHand;

    public HealthDamageData WeaponHealthDamageData;

    [Space]
    [Header("Audio")]
    public AudioClip MagInOutAudioClip;
    public AudioClip ReloadAudioClip;

    [Space]
    [Header("Animator")]
    public Animator Animator;
    [SerializeField] private string _shootTriggerName = "Shoot";
    [SerializeField] private string _reloadTriggerName = "Reload";

    private int _shootTriggerNameHash;
    private int _reloadTriggerNameHash;

    // RECOIL.
    // Rotation
    private Vector3 _currentRotation;
    private Vector3 _targetRotation;

    [Space]
    [Header("Interactable Settings")]
    public string Message = "Swap (This weapons Name)";

    [Space]
    [Header("Callback Events")]
    [Space(5)]
    public UnityEvent OnShoot;
    public UnityEvent OnAimEnter;
    public UnityEvent OnAimExit;
    public UnityEvent OnReloadStart;

    // CHANGES (
    //public UnityEvent<PlayerController> OnInteraction;
    //public UnityEvent<PlayerController, bool> OnWeaponStartOrStopUse;

    public UnityEvent<HKPlayerInteractionBase> OnInteraction;
    public UnityEvent<HKPlayerWeaponManager, bool> OnWeaponStartOrStopUse;
    // CHANGES )

    private bool _isCurrentWeapon;

    // CHANGES (
    //private PlayerController _controller;

    private HKPlayerWeaponManager _controller;
    // CHANGES )

    [HideInInspector] public WeaponHeatState WeaponHeatState { get; private set; } = WeaponHeatState.cool;
    private float _currentExtraHeat;
    private bool _isCoolingFromOverheat = false;

    public float _gunShotTimer { get; private set; } = 0;
    public bool _isReloading { get; private set; } = false;

    private Rigidbody _weaponRigidbody;

    #endregion

    #region General

    public virtual void Start()
    {
        _shootTriggerNameHash = Animator.StringToHash(_shootTriggerName);
        _reloadTriggerNameHash = Animator.StringToHash(_reloadTriggerName);

        _weaponRigidbody = GetComponent<Rigidbody>();
    }

    // update is called once per frame.
    public virtual void Update()
    {
        // CHANGES (
        //// Handle Sway & Recoil.
        //HandleSwayAndRecoil();
        // CHANGES )

        // Handle Weapon Heating
        HandleWeaponHeating();

        // Handle Shooting
        HandleShooting();
    }

    // CHANGES (
    public virtual void UpdateWeapon(Vector2 lookInput, float lookUpLimit, float lookDownLimit, float xRotation)
    {
        HandleSwayAndRecoil(lookInput, lookUpLimit, lookDownLimit, xRotation);
    }
    // CHANGES )

    // CHANGES COMMENTED OUT = OLD
    //private void HandleSwayAndRecoil()
    private void HandleSwayAndRecoil(Vector2 lookInput, float lookUpLimit, float lookDownLimit, float xRotation)
    {
        // Check if we are the current weapon.
        if (_isCurrentWeapon && UseSway)
        {
            // Lerp the target rotation to zero.
            _targetRotation = Vector3.Lerp(_targetRotation, Vector3.zero, WeaponData.ReturnSpeed * Time.deltaTime);

            // Slerp the current rotation to the target rotation.
            _currentRotation = Vector3.Slerp(_currentRotation, _targetRotation, WeaponData.Snappiness * Time.deltaTime);

            // CHANGES (
            //float lookX = _controller.Input.Player.Look.ReadValue<Vector2>().x * WeaponData.SwayAmount;
            //float lookY = _controller.Input.Player.Look.ReadValue<Vector2>().y * WeaponData.SwayAmount;

            float lookX = lookInput.x * WeaponData.SwayAmount;
            float lookY = lookInput.y * WeaponData.SwayAmount;
            // CHANGES )

            Quaternion swayRotation;
            Quaternion finalRot;

            // CHANGES (
            //float currentLookUpLimit = -_controller.GetLookUpLimit();
            //float currentLookDownLimit = _controller.GetLookDownLimit();

            float currentLookUpLimit = -lookUpLimit;
            float currentLookDownLimit = lookDownLimit;
            // CHANGES )

            // CHANGES OLD CODE HERE IS COMMENTED OUT.
            //if (_controller.GetXRotation() > (currentLookDownLimit - 1) || _controller.GetXRotation() < (currentLookUpLimit + 1))
            if (xRotation > (currentLookDownLimit - 1) || xRotation < (currentLookUpLimit + 1))
            {
                finalRot = Quaternion.AngleAxis(-lookX, Vector3.forward);
            }
            else
            {
                finalRot = Quaternion.AngleAxis(-lookY, Vector3.right) * Quaternion.AngleAxis(-lookX, Vector3.forward);
            }

            swayRotation = Quaternion.Slerp(transform.localRotation, finalRot *
                    Quaternion.Euler(_currentRotation), Time.deltaTime * WeaponData.SwaySmooth);

            transform.localRotation = swayRotation;
        }
    }

    private void HandleWeaponHeating()
    {
        if (!WeaponData.EnableHeatingSystem)
        {
            _currentExtraHeat = 0;

            WeaponHeatState = WeaponHeatState.cool;

            _isCoolingFromOverheat = false;

            return;
        }

        // Set Current Extra Heat.
        _currentExtraHeat = _currentExtraHeat > 0 ? _currentExtraHeat -= WeaponData.ConstantHeatCooldownRatePerSecond * Time.deltaTime : 0f;

        // Check if the weapon is overheated, Set heat state accordingly.
        if (_currentExtraHeat >= WeaponData.ExtraOverheatThreshold)
        {
            WeaponHeatState = WeaponHeatState.overHeated;

            // Getting to over heat limit means we are now cooling from overheat(As shooting won't be allowed)
            _isCoolingFromOverheat = true;
        }
        else if (_currentExtraHeat >= WeaponData.ExtraHeatCoolingThreshold)
        {
            if (_isCoolingFromOverheat) WeaponHeatState = WeaponHeatState.cooling;
            else WeaponHeatState = WeaponHeatState.cool;
        }
        else
        {
            WeaponHeatState = WeaponHeatState.cool;

            _isCoolingFromOverheat = false;
        }
    }

    private void HandleShooting()
    {
        if (_gunShotTimer >= 0f) _gunShotTimer -= Time.deltaTime;
    }

    #endregion

    #region Weapon Mechanics Functions

    /// <summary>
    /// plays the recoil on the weapon.
    /// </summary>
    public virtual ShootResult Shoot()
    {
        ShootResult shootResult = new ShootResult(false, false);

        if (_gunShotTimer > 0f) return shootResult;

        if (WeaponData.WeaponType == WeaponType.knife)
        {
            // Knife Specific Code (Not the best way though!, But preferred).

            // Set shoot result val 02.
            shootResult.BumpRotationEffect = false;

            _gunShotTimer = WeaponData.TimeBetweenShot;
        }
        else
        {
            // Return false if any of the conditions meet.
            if (_isReloading || CurrentAmmo <= 0 || WeaponHeatState != WeaponHeatState.cool) return shootResult;

            shootResult.BumpRotationEffect = true;

            // Get position and direction.
            Vector3 position = FirePoint.position;
            Vector3 direction = RandomizeDirection(FirePoint.forward, WeaponData.MaxBulletSpreadAngle);

            // Perform Shooting.
            if (WeaponData.FiringType == FiringType.raycast)
            {
                // check if we hit something.
                if (Physics.Raycast(position, direction, out RaycastHit hit, WeaponData.MaxShootRange))
                {
                    // get the IHitable interface reference.
                    IHitable iHitable = hit.transform.GetComponent<IHitable>();

                    // check if hitable is not null.
                    if (iHitable != null)
                    {
                        // Hit.
                        iHitable.Hit(hit.transform.gameObject, hit.point, hit.normal, WeaponHealthDamageData);
                    }
                }
            }
            else if (WeaponData.FiringType == FiringType.projectile)
            {
                Projectile projectile = Instantiate(WeaponData.ProjectilePrefab, position, Quaternion.LookRotation(direction)).GetComponent<Projectile>();
                projectile.Fire(WeaponData.ProjectileForce, WeaponHealthDamageData);
            }

            // Deplete Ammo.
            CurrentAmmo -= 1;

            // Add Heat and set Gun Shot timer.
            _currentExtraHeat += WeaponData.ExtraHeatIncreasePerShot;
            _gunShotTimer = WeaponData.TimeBetweenShot;

            // Play Effects.
            MuzzleFlashEffect.Play();
            AudioSource.Play();

            // Check if we should automatically eject a shell out.
            if (WeaponData.EjectShellOnShoot) ShellEjectEffect.Play();

            // Recoil
            // Set the target rotation to the recoil calculation...
            _targetRotation += new Vector3(WeaponData.RecoilX, Random.Range(-WeaponData.RecoilY, WeaponData.RecoilY), Random.Range(-WeaponData.RecoilZ, WeaponData.RecoilZ));

            //// Get Impulse Recoil.
            //Vector3 impulseRecoil = WeaponData.CinemachineRecoilImpulse;

            // CHANGES, USING CLASSIC IMPULSE SYSTEM.
            // Generate impulse.
            //RecoilImpulseSource.GenerateImpulse(new Vector3(Random.Range(-impulseRecoil.x, impulseRecoil.x)
            //    , Random.Range(-impulseRecoil.y, impulseRecoil.y)
            //    , impulseRecoil.z));
            //CameraShakeManager.instance.CameraShakeVelocity(RecoilImpulseSource, new Vector3(Random.Range(-impulseRecoil.x, impulseRecoil.x)
            //    , Random.Range(-impulseRecoil.y, impulseRecoil.y)
            //    , impulseRecoil.z));

            // CLASSIC IMPULSE SYSTEM.
            CameraShakeManager.instance.CameraShakeFromProfile(WeaponData.ShakeProfile, RecoilImpulseSource);
        }

        // Invoke Event
        OnShoot.Invoke();

        // Animator
        Animator.SetTrigger(_shootTriggerNameHash);

        shootResult.ShootActionSuccess = true;

        // If we come here we have successfully shot : Return True.
        return shootResult;
    }

    /// <summary>
    /// Takes in the direction, and max angle, randomizes the direction a bit and returns it.
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="maxAngle"></param>
    /// <returns></returns>
    protected Vector3 RandomizeDirection(Vector3 direction, float maxAngle)
    {
        // Get Random Angles.
        float angleY = Random.Range(-maxAngle, maxAngle);
        float angleX = Random.Range(-maxAngle, maxAngle);

        // Convert to rotations.
        Quaternion rotationY = Quaternion.AngleAxis(angleY, Vector3.up);
        Quaternion rotationX = Quaternion.AngleAxis(angleX, Vector3.right);

        // Final Rotation.
        Quaternion rotation = rotationY * rotationX;

        // Multiply with direction.
        return rotation * direction;
    }

    /// <summary>
    /// Start Reloading, Can be overriden for custom based weapons.
    /// </summary>
    // Reload
    public virtual void StartReloading()
    {
        // Return if the any of the Conditions meet.
        if (CurrentAmmo >= WeaponData.MagazineSize || _isReloading == true || _gunShotTimer > 0f) return;

        // Invoke Event
        OnReloadStart.Invoke();

        // Coroutine Reloading.
        StartCoroutine(ReloadingTime());

        // Set animator trigger.
        Animator.SetTrigger(_reloadTriggerNameHash);
    }

    protected IEnumerator ReloadingTime()
    {
        _isReloading = true;
        yield return new WaitForSeconds(WeaponData.ReloadingAnimationTime);
        _isReloading = false;
    }

    /// <summary>
    /// Plays the mag in out audio clip and parents the mag to the correct hand.
    /// </summary>
    /// <param name="animationEventData"></param>
    public virtual void SetMagParentToHand(AnimationEvent animationEventData)
    {
        // Play the mag out audio.
        AudioSource.PlayClipAtPoint(MagInOutAudioClip, Mag.transform.position, 1.0f);

        // Check if the left hand should be the parent.
        if (animationEventData.stringParameter == "LeftHand")
        {
            // The Hand to parent is left.
            if (HandsIKTargets.LeftHandIKTransform != null) Mag.transform.SetParent(HandsIKTargets.LeftHandIKTransform);
            else if (HandsRotationConstraintTransforms.LeftHandIKTransform != null) Mag.transform.SetParent(HandsRotationConstraintTransforms.LeftHandIKTransform);
        }
        else
        {
            // The Hand to parent is right.
            if (HandsIKTargets.RightHandIKTransform != null) Mag.transform.SetParent(HandsIKTargets.RightHandIKTransform);
            else if (HandsRotationConstraintTransforms.RightHandIKTransform != null) Mag.transform.SetParent(HandsRotationConstraintTransforms.RightHandIKTransform);
        }
    }

    /// <summary>
    /// Sets the mag parent to original and plays the mag in out audio.
    /// </summary>
    public virtual void SetMagToMagParent()
    {
        // Play the mag in audio.
        AudioSource.PlayClipAtPoint(MagInOutAudioClip, Mag.transform.position, 1.0f);

        Mag.transform.SetParent(MagParent);
        Mag.transform.position = MagParent.position;
        Mag.transform.rotation = MagParent.rotation;

        // For Normal weapons will only be calling this function, custom ones may or may not, we need to do the bullets loading code.
        if (TotalAmmo < WeaponData.MagazineSize)
        {
            CurrentAmmo = TotalAmmo;
            TotalAmmo = 0;
        }
        else
        {
            CurrentAmmo = WeaponData.MagazineSize;
            TotalAmmo -= WeaponData.MagazineSize;
        }
    }

    /// <summary>
    /// This Function Is Spawning A mag at the original mag point & Disables Actual Mag.
    /// </summary>
    public virtual void SpawnMagAtMagPosition()
    {
        // Spawn Clone of the (Prefab) Mag.
        Instantiate(MagPrefab, Mag.transform.position, Mag.transform.rotation, null);

        // Disable actual mag.
        Mag.SetActive(false);
    }

    /// <summary>
    /// This Function Sets Active of the mag to true.
    /// </summary>
    public virtual void TakeOutNewMag()
    {
        // Enable actual mag.
        Mag.SetActive(true);
    }

    /// <summary>
    /// This Function plays the reloadAudioClip.
    /// </summary>
    public virtual void PlayAudioClip(AnimationEvent animationData)
    {
        // Get Audio Clip.
        AudioClip providedReloadAudioClip = (AudioClip)animationData.objectReferenceParameter;

        // Based on the audio clip, select correct audio to play.
        if (providedReloadAudioClip != null) AudioSource.PlayClipAtPoint(providedReloadAudioClip, transform.position, 1.0f);
        else AudioSource.PlayClipAtPoint(ReloadAudioClip, transform.position, 1.0f);
    }

    /// <summary>
    /// Play's The Shell Eject effect Particle System.
    /// </summary>
    public virtual void PlayShellEjectEffect()
    {
        ShellEjectEffect.Play();
    }

    // Return The Interaction Range Message
    public virtual string GetMessage()
    {
        return Message;
    }

    // CHANGES (

    //public bool CanInteract(PlayerController playerController)
    //{
    //    return playerController.CurrentWeapon() != this && playerController.CurrentWeapon().WeaponData.WeaponType == WeaponData.WeaponType;
    //}

    public virtual bool CanInteract(HKPlayerInteractionBase interactionController)
    {
        //return weaponManager.CurrentWeapon() != this && weaponManager.CurrentWeapon().WeaponData.WeaponType == WeaponData.WeaponType;
        return interactionController.CurrentWeapon() != this && interactionController.CurrentWeapon().WeaponData.WeaponType == WeaponData.WeaponType;
    }

    // CHANGES )

    // Interact

    // CHANGES (

    //public void Interact(PlayerController playerController)
    //{
    //    // Swap The Weapon.
    //    playerController.SwapWeapon(this);

    //    // Enable Kinematic
    //    _weaponRigidbody.isKinematic = true;

    //    // Invoke Event.
    //    OnInteraction.Invoke(playerController);
    //}

    public virtual void Interact(HKPlayerInteractionBase interactionController)
    {
        // Swap The Weapon.
        interactionController.SwapWeapon(this);

        // Enable Kinematic
        _weaponRigidbody.isKinematic = true;

        // Invoke Event.
        OnInteraction.Invoke(interactionController);
    }
    // CHANGES )

    public virtual bool IsCurrentWeapon()
    {
        return _isCurrentWeapon;
    }

    // CHANGES (
    //public void SetWeaponData(PlayerController controller, bool isCurrentWeapon)
    //{
    //    _controller = controller;
    //    _isCurrentWeapon = isCurrentWeapon;
    //    OnWeaponStartOrStopUse.Invoke(controller, isCurrentWeapon);
    //}

    public virtual void SetWeaponData(HKPlayerWeaponManager controller, bool isCurrentWeapon)
    {
        _controller = controller;
        _isCurrentWeapon = isCurrentWeapon;
        OnWeaponStartOrStopUse.Invoke(controller, isCurrentWeapon);
    }

    // CHANGES )

    public virtual void DropWeapon()
    {
        _weaponRigidbody.isKinematic = false;
    }

    public virtual void AddForceToWeapon(Vector3 force, ForceMode forceMode = ForceMode.Impulse)
    {
        _weaponRigidbody.AddForce(force, forceMode);
    }

    public HKPlayerWeaponManager GetPlayerWeapon()
    {
        return _controller;
    }

    // CHANGES (

    public void InvokeOnAimEnter()
    {
        OnAimEnter.Invoke();
    }

    public void InvokeOnAimExit()
    {
        OnAimExit.Invoke();
    }

    #endregion

    #region Val Returners

    public float GetCurrentExtraHeat()
    {
        return _currentExtraHeat;
    }

    #endregion
}

#region Editor

#if UNITY_EDITOR

[CustomEditor(typeof(WeaponBase))]
public class WeaponBaseEditor : Editor
{

    #region Weapon Setup Editor Window

    // Weapon Setup Window.
    public class SetupHelperEditorWindow : EditorWindow
    {

        #region Variables

        // The references from the class which opened the window....
        private Transform parentTransform;
        private WeaponBase weaponBaseClass;

        // Names of Hand IK targets
        private string leftHandIKTargetName = "LeftHandIKTarget";
        private string leftHandIndexIKTargetName = "LeftHandIndexIKTarget";
        private string leftHandMiddleIKTargetName = "LeftHandMiddleIKTarget";
        private string leftHandPinkyIKTargetName = "LeftHandPinkyIKTarget";
        private string leftHandRingIKTargetName = "LeftHandRingIKTarget";
        private string leftHandThumbIKTarget = "LeftHandThumbIKTarget";

        private string rightHandIKTargetName = "RightHandIKTarget";
        private string rightHandIndexIKTargetName = "RightHandIndexIKTarget";
        private string rightHandMiddleIKTargetName = "RightHandMiddleIKTarget";
        private string rightHandPinkyIKTargetName = "RightHandPinkyIKTarget";
        private string rightHandRingIKTargetName = "RightHandRingIKTarget";
        private string rightHandThumbIKTarget = "RightHandThumbIKTarget";

        // Names of Fingers Rotation Constraints targets
        // Left Hand's Fingers.
        private string leftHandIndex1ConstraintTargetName = "LeftHandIndex1ConstraintTarget";
        private string leftHandIndex2ConstraintTargetName = "LeftHandIndex2ConstraintTarget";
        private string leftHandIndex3ConstraintTargetName = "LeftHandIndex3ConstraintTarget";

        private string leftHandMiddle1ConstraintTargetName = "LeftHandMiddle1ConstraintTarget";
        private string leftHandMiddle2ConstraintTargetName = "LeftHandMiddle2ConstraintTarget";
        private string leftHandMiddle3ConstraintTargetName = "LeftHandMiddle3ConstraintTarget";

        private string leftHandPinky1ConstraintTargetName = "LeftHandPinky1ConstraintTarget";
        private string leftHandPinky2ConstraintTargetName = "LeftHandPinky2ConstraintTarget";
        private string leftHandPinky3ConstraintTargetName = "LeftHandPinky3ConstraintTarget";

        private string leftHandRing1ConstraintTargetName = "LeftHandRing1ConstraintTarget";
        private string leftHandRing2ConstraintTargetName = "LeftHandRing2ConstraintTarget";
        private string leftHandRing3ConstraintTargetName = "LeftHandRing3ConstraintTarget";

        private string leftHandThumb1ConstraintTargetName = "LeftHandThumb1ConstraintTarget";
        private string leftHandThumb2ConstraintTargetName = "LeftHandThumb2ConstraintTarget";
        private string leftHandThumb3ConstraintTargetName = "LeftHandThumb3ConstraintTarget";

        // Right Hand's Fingers.
        private string rightHandIndex1ConstraintTargetName = "RightHandIndex1ConstraintTarget";
        private string rightHandIndex2ConstraintTargetName = "RightHandIndex2ConstraintTarget";
        private string rightHandIndex3ConstraintTargetName = "RightHandIndex3ConstraintTarget";

        private string rightHandMiddle1ConstraintTargetName = "RightHandMiddle1ConstraintTarget";
        private string rightHandMiddle2ConstraintTargetName = "RightHandMiddle2ConstraintTarget";
        private string rightHandMiddle3ConstraintTargetName = "RightHandMiddle3ConstraintTarget";

        private string rightHandPinky1ConstraintTargetName = "RightHandPinky1ConstraintTarget";
        private string rightHandPinky2ConstraintTargetName = "RightHandPinky2ConstraintTarget";
        private string rightHandPinky3ConstraintTargetName = "RightHandPinky3ConstraintTarget";

        private string rightHandRing1ConstraintTargetName = "RightHandRing1ConstraintTarget";
        private string rightHandRing2ConstraintTargetName = "RightHandRing2ConstraintTarget";
        private string rightHandRing3ConstraintTargetName = "RightHandRing3ConstraintTarget";

        private string rightHandThumb1ConstraintTargetName = "RightHandThumb1ConstraintTarget";
        private string rightHandThumb2ConstraintTargetName = "RightHandThumb2ConstraintTarget";
        private string rightHandThumb3ConstraintTargetName = "RightHandThumb3ConstraintTarget";


        private string weaponScriptableObjectName = "WeaponScriptableObject";

        private GameObject muzzleFlashEffectGameObject = null;
        private GameObject shellEjectEffectGameObject = null;

        private GameObject mag = null;

        private Transform meshTransform = null;

        private AudioClip firingAudioClip = null;

        private int pageNumber = 0;


        SerializedProperty handsConstraintTypeProperty;


        private Vector2 scrollViewAmount;

        #endregion

        // Open Window.
        /// <summary>
        /// transform : the transform with the weapon base class.
        /// weaponBase : the weaponBase that needs setup.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="weaponBase"></param>
        public static void OpenWindow(Transform transform, WeaponBase weaponBase)
        {
            // Get the window.
            SetupHelperEditorWindow window = GetWindow<SetupHelperEditorWindow>();

            // Set window stuff...
            window.titleContent = new GUIContent("Weapon Setup");
            window.parentTransform = transform.GetChild(0);
            window.weaponBaseClass = weaponBase;
            window.pageNumber = 0;
            window.Show();
        }

        private void OnGUI()
        {
            if (weaponBaseClass == null)
            {
                // Weapon Base Class was destroyed or this window was opened while assigning weapon base to null.
                Close();
            }

            scrollViewAmount = GUILayout.BeginScrollView(scrollViewAmount);

            // Some Space To Start Nicely.
            GUILayout.Space(5);

            // Begin Horizontal.
            GUILayout.BeginHorizontal();

            // 5 Pixels Space.
            GUILayout.Space(5);

            // Create a new Style.
            GUIStyle stepLabelStyle = new GUIStyle();

            // Set the Styles that we want to change.
            stepLabelStyle.fontSize = 15;
            stepLabelStyle.normal.textColor = Color.white;
            stepLabelStyle.fontStyle = FontStyle.Italic;

            // Display the current step + / + the amount of steps.
            GUILayout.Label("Step " + (pageNumber + 1) + "/" + 4, stepLabelStyle);

            // End Horizontal.
            GUILayout.EndHorizontal();

            // 10 pixels space.
            GUILayout.Space(10);

            // If The first page, Then draw the first page
            if ((pageNumber + 1) == 1) // Page num one.
            {
                DrawPageOne();
            }
            else if ((pageNumber + 1) == 2) // Page num two.
            {
                DrawPageTwo();
            }
            else if ((pageNumber + 1) == 3) // Page num three.
            {
                DrawPageThree();
            }
            else if ((pageNumber + 1) == 4) // Page num four
            {
                DrawPageFour();
            }

            GUILayout.BeginHorizontal();

            // Draw previous button only if the page number isn't 0.
            if ((pageNumber + 1) != 0)
            {
                if (GUILayout.Button("Previous Step"))
                {
                    PreviousPage();
                }
            }

            // Draw next button only if the page number isn't 4, Otherwise draw close button.
            if ((pageNumber + 1) != 4)
            {
                if (GUILayout.Button("Next Step"))
                {
                    NextPage();
                }
            }
            else
            {
                if (GUILayout.Button("Close"))
                {
                    // Finish Setup Code.
                    Close();
                }
            }

            // End horizontal.
            GUILayout.EndHorizontal();

            // Quite a bit of space.
            GUILayout.Space(70);

            // End Scroll View.
            GUILayout.EndScrollView();
        }

        #region Page Changing Functions

        /// <summary>
        /// Changes the page number by one.
        /// </summary>
        private void NextPage()
        {
            pageNumber += 1;
        }

        /// <summary>
        /// Changes the page number by negative one.
        /// </summary>
        private void PreviousPage()
        {
            pageNumber -= 1;
        }

        #endregion

        #region All Page Drawing Functions.

        /// <summary>
        /// Draws the page one.
        /// </summary>
        private void DrawPageOne()
        {
            // Title.
            GUILayout.Label("Create Hands IK Targets", EditorStyles.boldLabel);

            // Space.
            GUILayout.Space(5);

            // Tell what the transform field is for.
            GUILayout.Label("Transform In Which To Create Hands IK Targets");

            // The transform everything will be childed to.
            parentTransform = EditorGUILayout.ObjectField(parentTransform, typeof(Transform), true) as Transform;

            // Space.
            GUILayout.Space(20);

            // Draw the Hands Constraint Type Field.
            weaponBaseClass.HandsConstraintType = (HandsConstraintType)EditorGUILayout.EnumPopup("Hands Constraint Type", weaponBaseClass.HandsConstraintType);

            // Space.
            GUILayout.Space(10);

            // Check if the hands constraint type if ikbased fingers.
            if (weaponBaseClass.HandsConstraintType == HandsConstraintType.IKBasedFingers)
            {
                // Show text fields for IK target names
                DrawTextField("Left Hand IK Target Name", ref leftHandIKTargetName);
                DrawTextField("Left Hand Index IK Target Name", ref leftHandIndexIKTargetName);
                DrawTextField("Left Hand Middle IK Target Name", ref leftHandMiddleIKTargetName);
                DrawTextField("Left Hand Pinky IK Target Name", ref leftHandPinkyIKTargetName);
                DrawTextField("Left Hand Ring IK Target Name", ref leftHandRingIKTargetName);
                DrawTextField("Left Hand Thumb IK Target Name", ref leftHandThumbIKTarget);

                DrawTextField("Right Hand IK Target Name", ref rightHandIKTargetName);
                DrawTextField("Right Hand Index IK Target Name", ref rightHandIndexIKTargetName);
                DrawTextField("Right Hand Middle IK Target Name", ref rightHandMiddleIKTargetName);
                DrawTextField("Right Hand Pinky IK Target Name", ref rightHandPinkyIKTargetName);
                DrawTextField("Right Hand Ring IK Target Name", ref rightHandRingIKTargetName);
                DrawTextField("Right Hand Thumb IK Target Name", ref rightHandThumbIKTarget);
            }
            else
            {
                // Show text fields for IK target names
                DrawTextField("Left Hand IK Target Name", ref leftHandIKTargetName);
                DrawTextField("Left Hand Index1 IK Target Name", ref leftHandIndex1ConstraintTargetName);
                DrawTextField("Left Hand Index2 IK Target Name", ref leftHandIndex2ConstraintTargetName);
                DrawTextField("Left Hand Index3 IK Target Name", ref leftHandIndex3ConstraintTargetName);

                DrawTextField("Left Hand Middle1 IK Target Name", ref leftHandMiddle1ConstraintTargetName);
                DrawTextField("Left Hand Middle2 IK Target Name", ref leftHandMiddle2ConstraintTargetName);
                DrawTextField("Left Hand Middle3 IK Target Name", ref leftHandMiddle3ConstraintTargetName);

                DrawTextField("Left Hand Pinky1 IK Target Name", ref leftHandPinky1ConstraintTargetName);
                DrawTextField("Left Hand Pinky2 IK Target Name", ref leftHandPinky2ConstraintTargetName);
                DrawTextField("Left Hand Pinky3 IK Target Name", ref leftHandPinky3ConstraintTargetName);

                DrawTextField("Left Hand Ring1 IK Target Name", ref leftHandRing1ConstraintTargetName);
                DrawTextField("Left Hand Ring2 IK Target Name", ref leftHandRing2ConstraintTargetName);
                DrawTextField("Left Hand Ring3 IK Target Name", ref leftHandRing3ConstraintTargetName);

                DrawTextField("Left Hand Thumb1 IK Target Name", ref leftHandThumb1ConstraintTargetName);
                DrawTextField("Left Hand Thumb2 IK Target Name", ref leftHandThumb2ConstraintTargetName);
                DrawTextField("Left Hand Thumb3 IK Target Name", ref leftHandThumb3ConstraintTargetName);


                DrawTextField("Right Hand Index1 IK Target Name", ref rightHandIndex1ConstraintTargetName);
                DrawTextField("Right Hand Index2 IK Target Name", ref rightHandIndex2ConstraintTargetName);
                DrawTextField("Right Hand Index3 IK Target Name", ref rightHandIndex3ConstraintTargetName);

                DrawTextField("Right Hand Middle1 IK Target Name", ref rightHandMiddle1ConstraintTargetName);
                DrawTextField("Right Hand Middle2 IK Target Name", ref rightHandMiddle2ConstraintTargetName);
                DrawTextField("Right Hand Middle3 IK Target Name", ref rightHandMiddle3ConstraintTargetName);

                DrawTextField("Right Hand Pinky1 IK Target Name", ref rightHandPinky1ConstraintTargetName);
                DrawTextField("Right Hand Pinky2 IK Target Name", ref rightHandPinky2ConstraintTargetName);
                DrawTextField("Right Hand Pinky3 IK Target Name", ref rightHandPinky3ConstraintTargetName);

                DrawTextField("Right Hand Ring1 IK Target Name", ref rightHandRing1ConstraintTargetName);
                DrawTextField("Right Hand Ring2 IK Target Name", ref rightHandRing2ConstraintTargetName);
                DrawTextField("Right Hand Ring3 IK Target Name", ref rightHandRing3ConstraintTargetName);

                DrawTextField("Right Hand Thumb1 IK Target Name", ref rightHandThumb1ConstraintTargetName);
                DrawTextField("Right Hand Thumb2 IK Target Name", ref rightHandThumb2ConstraintTargetName);
                DrawTextField("Right Hand Thumb3 IK Target Name", ref rightHandThumb3ConstraintTargetName);
            }

            // Space
            GUILayout.Space(20);

            // Draw a Button for Creating Hands IK Targets
            if (GUILayout.Button("Create Hands IK Targets", GUILayout.Height(25)))
            {
                // Create the IK targets.
                CreateHandsConstraintTargets();

                // And go to the next page.
                NextPage();
            }

            // Space
            GUILayout.Space(10);
        }

        /// <summary>
        /// Draws the page two.
        /// </summary>
        private void DrawPageTwo()
        {
            // Page name Label.
            GUILayout.Label("Create Weapon Scriptable Object Settings Asset", EditorStyles.boldLabel);

            // Space.
            GUILayout.Space(10);

            // Draw the text field for the name of the weapon SO that shall be created.
            DrawTextField("Weapon Scriptable Object Name", ref weaponScriptableObjectName);

            // Space
            GUILayout.Space(10);

            // Draw a Button for Creating the Weapon Scriptable Object (SO) Settings.
            if (GUILayout.Button("Create " + weaponScriptableObjectName, GUILayout.Height(25)))
            {
                // Function Handles Weapon SO Creation, Returns true if created, And false if cancelled.
                bool created = CreateWeaponSOAsset("Select Folder To Create " + weaponScriptableObjectName, weaponScriptableObjectName);

                // Check if we have created a weapon SO Asset.
                if (created == true)
                {
                    // Next Page
                    NextPage();
                }
            }

            // Space
            GUILayout.Space(10);
        }

        /// <summary>
        /// Draws the page three.
        /// </summary>
        private void DrawPageThree()
        {
            // Title
            GUILayout.Label("Create Fire Point, Muzzle Flash Effect & Mag Parent", EditorStyles.boldLabel);

            // 10 pixels space
            GUILayout.Space(10);

            // Label for telling what the field below is for.
            EditorGUILayout.LabelField("Assign your weapons Main Mesh Parent(Objects Will be Childed to that)");

            // The transform everything will be childed to.
            meshTransform = EditorGUILayout.ObjectField(meshTransform, typeof(Transform), true) as Transform;

            // 10 pixels space
            GUILayout.Space(10);

            #region Fire Point

            // Draw UI Elements for Fire Point creation.

            // Label for telling what the field below is for.
            EditorGUILayout.LabelField("Assign a Firing Audio Clip");

            // The transform everything will be childed to.
            firingAudioClip = EditorGUILayout.ObjectField(firingAudioClip, typeof(AudioClip), true) as AudioClip;

            // Draw a Button for creating the fire point.
            if (GUILayout.Button("Create Fire Point GameObject", GUILayout.Height(25)))
            {
                // Check if the mesh transform is not assigned, and return if not.
                if (meshTransform == null)
                {
                    Debug.LogWarning("Please Assign The Mesh Transform");
                    return;
                }
                else if (firingAudioClip == null) // And Also if firing audio clip hasn't been assigned.
                {
                    Debug.LogWarning("Please Assign A weapon firing audio clip");
                    return;
                }

                // Create the Fire Point GameObject.
                GameObject firePoint = new GameObject("FirePoint");

                // Set the parent.
                firePoint.transform.parent = meshTransform;

                // Add an AudioSource Component.
                AudioSource firingAudioSource = firePoint.AddComponent<AudioSource>();

                // Relevant Parameters of the Firing Audio Source.
                firingAudioSource.clip = firingAudioClip;
                firingAudioSource.playOnAwake = false;
                firingAudioSource.spatialBlend = 1.0f;

                // Set the weapon's References.
                weaponBaseClass.FirePoint = firePoint.transform;
                weaponBaseClass.AudioSource = firingAudioSource;
            }

            // 10 pixels space
            GUILayout.Space(10);

            #endregion

            #region Muzzle Flash

            // Draw UI Elements for Muzzle Flash Effect creation.

            // Label for telling what the field below is for.
            EditorGUILayout.LabelField("Assign a muzzle flash which shall be spawned");

            // Muzzle Flash Effect to spawn into the mesh.
            muzzleFlashEffectGameObject = EditorGUILayout.ObjectField(muzzleFlashEffectGameObject, typeof(GameObject), true) as GameObject;

            // Draw a Button for creating the fire muzzle Flash Effect.
            if (GUILayout.Button("Create Muzzle Flash Effect", GUILayout.Height(25)))
            {
                // Check if the muzzle flash gameobject has been assigned.
                if (muzzleFlashEffectGameObject != null)
                {
                    // Check if the mesh transform is not assigned, and return if not.
                    if (meshTransform == null)
                    {
                        Debug.LogWarning("Please Assign The Mesh Transform");
                        return;
                    }

                    // Create A new Gameobject or SPAWN.
                    GameObject muzzleFlashEffect = Instantiate(muzzleFlashEffectGameObject);

                    // Set the Name Of the GameObject
                    muzzleFlashEffect.name = "MuzzleFlashEffect";

                    // Set the parent, position, rotation.
                    muzzleFlashEffect.transform.parent = meshTransform;
                    muzzleFlashEffect.transform.position = meshTransform.position;
                    muzzleFlashEffect.transform.rotation = meshTransform.rotation;

                    // Set the weapon's References.
                    weaponBaseClass.MuzzleFlashEffect = muzzleFlashEffect.GetComponent<ParticleSystem>();

                    // Selection
                    Selection.activeGameObject = muzzleFlashEffect;
                }
                else
                {
                    // Log a warning letting the user know that they must assign a muzzle flash to create.
                    Debug.LogWarning("To Create a Muzzle Flash GameObject, Please Assign One.");
                }
            }

            // 10 pixels space
            GUILayout.Space(10);

            #endregion

            #region Shell Eject Effect

            // Draw UI Elements for Shell Eject Effect creation.

            // Label for telling what the field below is for.
            EditorGUILayout.LabelField("Assign a Shell Eject Effect which shall be spawned");

            // Shell Eject effect to spawn into the mesh.
            shellEjectEffectGameObject = EditorGUILayout.ObjectField(shellEjectEffectGameObject, typeof(GameObject), true) as GameObject;

            // Draw a Button for creating the shell eject Effect.
            if (GUILayout.Button("Create Shell Eject Effect", GUILayout.Height(25)))
            {
                // Check if the muzzle flash gameobject has been assigned.
                if (shellEjectEffectGameObject != null)
                {
                    // Check if the mesh transform is not assigned, and return if not.
                    if (meshTransform == null)
                    {
                        // Log Warning.
                        Debug.LogWarning("Please Assign The Mesh Transform");
                        return;
                    }

                    // Create A new Gameobject or SPAWN.
                    GameObject shellEjectEffect = Instantiate(shellEjectEffectGameObject);

                    // Set the Name Of the GameObject
                    shellEjectEffect.name = "ShellEjectEffect";

                    // Set the parent, position, rotation.
                    shellEjectEffect.transform.parent = meshTransform;
                    shellEjectEffect.transform.position = meshTransform.position;
                    shellEjectEffect.transform.rotation = meshTransform.rotation;

                    // Set the weapon's References.
                    weaponBaseClass.ShellEjectEffect = shellEjectEffect.GetComponent<ParticleSystem>();

                    // Selection
                    Selection.activeGameObject = shellEjectEffect;
                }
                else
                {
                    // Log a warning letting the user know that they must assign a muzzle flash to create.
                    Debug.LogWarning("To Create a Shell Efect Flash GameObject, Please Assign One.");
                }
            }

            // 10 pixels space
            GUILayout.Space(10);

            #endregion

            #region Mag parent

            // Label for telling what the field below is for.
            EditorGUILayout.LabelField("Assign your weapons mag for creating a mag parent");

            // The transform everything will be childed to.
            mag = EditorGUILayout.ObjectField(mag, typeof(GameObject), true) as GameObject;

            // Draw a Button for creating a mag parent.
            if (GUILayout.Button("Create Mag Parent", GUILayout.Height(25)))
            {
                if (mag != null)
                {
                    // Create a new GameObject for the MagParent.
                    GameObject magParent = new GameObject("MagParent");

                    // Set Mag Parent's Position and rotation.
                    magParent.transform.position = mag.transform.position;
                    magParent.transform.rotation = mag.transform.rotation;

                    // Set Mag Parent's Parent.
                    magParent.transform.parent = mag.transform.parent;

                    // Child the mag to the mag parent.
                    mag.transform.parent = magParent.transform;

                    // Set script references.
                    weaponBaseClass.Mag = mag;
                    weaponBaseClass.MagParent = magParent.transform;
                }
                else
                {
                    // Tell the user that they must assign there weapon's mag
                    Debug.LogWarning("Please Assign your weapon's mag, So that The Mag Parent can be created");
                }
            }

            // 10 pixels space
            GUILayout.Space(10);

            #endregion
        }

        /// <summary>
        /// Draws the page four.
        /// </summary>
        private void DrawPageFour()
        {
            // Title
            GUILayout.Label("Finishing Weapon Setup", EditorStyles.boldLabel);

            // 10 pixels space
            GUILayout.Space(10);

            // Label.
            GUILayout.Label("Add a Cinemachine Impulse Source for Camera Recoil Effect");

            // Button for adding the CinemachineImpulse source component.
            if (GUILayout.Button("Add Recoil Impulse Component(Cinemachine)", GUILayout.Height(25)))
            {
                // Create Recoil Impulse Source.
                CinemachineImpulseSource recoilImpulseSource = weaponBaseClass.gameObject.AddComponent<CinemachineImpulseSource>();

                // Set Script references.
                weaponBaseClass.RecoilImpulseSource = recoilImpulseSource;
            }

            // 10 pixels space
            GUILayout.Space(10);

            // Label.
            GUILayout.Label("Add a Box collider, Adjust the Bounds to fit,");
            GUILayout.Label("or if you want you can add a different type of Collider yourself");

            // Button for adding a box collider.
            if (GUILayout.Button("Add Box Collider", GUILayout.Height(25)))
            {
                // Add a Box Collider Component.
                weaponBaseClass.gameObject.AddComponent<BoxCollider>();
            }

            // 10 pixels space
            GUILayout.Space(10);

            // Label.
            GUILayout.Label("Add an Animator, So you can create animations for this weapon");

            // Button For adding animator component.
            if (GUILayout.Button("Add Animator Component", GUILayout.Height(25)))
            {
                // Add Animator Component.
                Animator weaponAnimator = weaponBaseClass.gameObject.AddComponent<Animator>();

                // Set weapon Base Classes Reference of the animator.
                weaponBaseClass.Animator = weaponAnimator;
            }

            // 10 pixels space
            GUILayout.Space(10);

            // Label, Set your weapons current ammo and total ammo by default.
            GUILayout.Label("Set your weapons current ammo and total ammo.");

            // Ammo fields.
            weaponBaseClass.TotalAmmo = EditorGUILayout.IntField("Current Ammo", weaponBaseClass.TotalAmmo);
            weaponBaseClass.CurrentAmmo = EditorGUILayout.IntField("Total Ammo", weaponBaseClass.CurrentAmmo);

            // 10 pixels space
            GUILayout.Space(10);

            // Label, Set your Weapon's reloading audio clips.
            GUILayout.Label("Set your Weapon's reloading audio clips");

            // Field for the audio clip
            GUILayout.Label("Mag In/Out");
            weaponBaseClass.MagInOutAudioClip = EditorGUILayout.ObjectField(weaponBaseClass.MagInOutAudioClip, typeof(AudioClip), true) as AudioClip;

            GUILayout.Space(5);

            GUILayout.Label("Reload");
            weaponBaseClass.ReloadAudioClip = EditorGUILayout.ObjectField(weaponBaseClass.ReloadAudioClip, typeof(AudioClip), true) as AudioClip;

            // 10 pixels space
            GUILayout.Space(10);

            // Field for interaction message.
            weaponBaseClass.Message = EditorGUILayout.TextField("Interaction Prompt", weaponBaseClass.Message);

            // 10 pixels space
            GUILayout.Space(10);
        }

        #endregion

        /// <summary>
        /// Helper Function Used for Drawing a Text Field.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="text"></param>
        private void DrawTextField(string label, ref string text)
        {
            // Begin Horizontal.
            GUILayout.BeginHorizontal();

            // Show the Label or name of the Field.
            GUILayout.Label(label, GUILayout.Width(200));

            // Show the Text of the field.
            text = EditorGUILayout.TextField(text);

            // End Horizontal.
            GUILayout.EndHorizontal();
        }

        #region Creation Functions

        /// <summary>
        /// Creates all ik Targets & assgns the references to the HandsIKTargets.
        /// </summary>
        private void CreateHandsConstraintTargets()
        {
            // Create the HandsIKTargets Parent.
            GameObject handsIKTargets = new GameObject("HandsIKTargets");

            // Create LeftHandIKTarget and RightHandIKTarget
            GameObject leftHandIKTarget = new GameObject(leftHandIKTargetName);
            GameObject rightHandIKTarget = new GameObject(rightHandIKTargetName);

            if (weaponBaseClass.HandsConstraintType == HandsConstraintType.IKBasedFingers)
            {
                // Create children for LeftHandIKTarget
                GameObject leftIndex = new GameObject(leftHandIndexIKTargetName);
                GameObject leftMiddle = new GameObject(leftHandMiddleIKTargetName);
                GameObject leftPinky = new GameObject(leftHandPinkyIKTargetName);
                GameObject leftRing = new GameObject(leftHandRingIKTargetName);
                GameObject leftThumb = new GameObject(leftHandThumbIKTarget);

                // Create children for RightHandIKTarget
                GameObject rightIndex = new GameObject(rightHandIndexIKTargetName);
                GameObject rightMiddle = new GameObject(rightHandMiddleIKTargetName);
                GameObject rightPinky = new GameObject(rightHandPinkyIKTargetName);
                GameObject rightRing = new GameObject(rightHandRingIKTargetName);
                GameObject rightThumb = new GameObject(rightHandThumbIKTarget);

                // Set parents for children
                leftIndex.transform.parent = leftHandIKTarget.transform;
                leftMiddle.transform.parent = leftHandIKTarget.transform;
                leftPinky.transform.parent = leftHandIKTarget.transform;
                leftRing.transform.parent = leftHandIKTarget.transform;
                leftThumb.transform.parent = leftHandIKTarget.transform;

                rightIndex.transform.parent = rightHandIKTarget.transform;
                rightMiddle.transform.parent = rightHandIKTarget.transform;
                rightPinky.transform.parent = rightHandIKTarget.transform;
                rightRing.transform.parent = rightHandIKTarget.transform;
                rightThumb.transform.parent = rightHandIKTarget.transform;

                // Set Created Hands IK Target To Hands IK Target Reference.
                // Left Hand
                weaponBaseClass.HandsIKTargets.LeftHandIKTransform = leftHandIKTarget.transform;
                weaponBaseClass.HandsIKTargets.LeftHandIndexIKTransform = leftIndex.transform;
                weaponBaseClass.HandsIKTargets.LeftHandMiddleIKTransform = leftMiddle.transform;
                weaponBaseClass.HandsIKTargets.LeftHandPinkyIKTransform = leftPinky.transform;
                weaponBaseClass.HandsIKTargets.LeftHandRingIKTransform = leftRing.transform;
                weaponBaseClass.HandsIKTargets.LeftHandThumbIKTransform = leftThumb.transform;

                // Right Hand
                weaponBaseClass.HandsIKTargets.RightHandIKTransform = rightHandIKTarget.transform;
                weaponBaseClass.HandsIKTargets.RightHandIndexIKTransform = rightIndex.transform;
                weaponBaseClass.HandsIKTargets.RightHandMiddleIKTransform = rightMiddle.transform;
                weaponBaseClass.HandsIKTargets.RightHandPinkyIKTransform = rightPinky.transform;
                weaponBaseClass.HandsIKTargets.RightHandRingIKTransform = rightRing.transform;
                weaponBaseClass.HandsIKTargets.RightHandThumbIKTransform = rightThumb.transform;
            }
            else
            {
                // Create children for LeftHandIKTarget
                GameObject leftIndex1 = new GameObject(leftHandIndex1ConstraintTargetName);
                GameObject leftIndex2 = new GameObject(leftHandIndex2ConstraintTargetName);
                GameObject leftIndex3 = new GameObject(leftHandIndex3ConstraintTargetName);

                GameObject leftMiddle1 = new GameObject(leftHandMiddle1ConstraintTargetName);
                GameObject leftMiddle2 = new GameObject(leftHandMiddle2ConstraintTargetName);
                GameObject leftMiddle3 = new GameObject(leftHandMiddle3ConstraintTargetName);

                GameObject leftPinky1 = new GameObject(leftHandPinky1ConstraintTargetName);
                GameObject leftPinky2 = new GameObject(leftHandPinky2ConstraintTargetName);
                GameObject leftPinky3 = new GameObject(leftHandPinky3ConstraintTargetName);

                GameObject leftRing1 = new GameObject(leftHandRing1ConstraintTargetName);
                GameObject leftRing2 = new GameObject(leftHandRing2ConstraintTargetName);
                GameObject leftRing3 = new GameObject(leftHandRing3ConstraintTargetName);

                GameObject leftThumb1 = new GameObject(leftHandThumb1ConstraintTargetName);
                GameObject leftThumb2 = new GameObject(leftHandThumb2ConstraintTargetName);
                GameObject leftThumb3 = new GameObject(leftHandThumb3ConstraintTargetName);

                // Create children for RightHandIKTarget
                GameObject rightIndex1 = new GameObject(rightHandIndex1ConstraintTargetName);
                GameObject rightIndex2 = new GameObject(rightHandIndex2ConstraintTargetName);
                GameObject rightIndex3 = new GameObject(rightHandIndex3ConstraintTargetName);

                GameObject rightMiddle1 = new GameObject(rightHandMiddle1ConstraintTargetName);
                GameObject rightMiddle2 = new GameObject(rightHandMiddle2ConstraintTargetName);
                GameObject rightMiddle3 = new GameObject(rightHandMiddle3ConstraintTargetName);

                GameObject rightPinky1 = new GameObject(rightHandPinky1ConstraintTargetName);
                GameObject rightPinky2 = new GameObject(rightHandPinky2ConstraintTargetName);
                GameObject rightPinky3 = new GameObject(rightHandPinky3ConstraintTargetName);

                GameObject rightRing1 = new GameObject(rightHandRing1ConstraintTargetName);
                GameObject rightRing2 = new GameObject(rightHandRing2ConstraintTargetName);
                GameObject rightRing3 = new GameObject(rightHandRing3ConstraintTargetName);

                GameObject rightThumb1 = new GameObject(rightHandThumb1ConstraintTargetName);
                GameObject rightThumb2 = new GameObject(rightHandThumb2ConstraintTargetName);
                GameObject rightThumb3 = new GameObject(rightHandThumb3ConstraintTargetName);

                // Set parents for children
                leftIndex1.transform.parent = leftHandIKTarget.transform;
                leftIndex2.transform.parent = leftIndex1.transform;
                leftIndex3.transform.parent = leftIndex2.transform;

                leftMiddle1.transform.parent = leftHandIKTarget.transform;
                leftMiddle2.transform.parent = leftMiddle1.transform;
                leftMiddle3.transform.parent = leftMiddle2.transform;

                leftPinky1.transform.parent = leftHandIKTarget.transform;
                leftPinky2.transform.parent = leftPinky1.transform;
                leftPinky3.transform.parent = leftPinky2.transform;

                leftRing1.transform.parent = leftHandIKTarget.transform;
                leftRing2.transform.parent = leftRing1.transform;
                leftRing3.transform.parent = leftRing2.transform;

                leftThumb1.transform.parent = leftHandIKTarget.transform;
                leftThumb2.transform.parent = leftThumb1.transform;
                leftThumb3.transform.parent = leftThumb2.transform;


                rightIndex1.transform.parent = rightHandIKTarget.transform;
                rightIndex2.transform.parent = rightIndex1.transform;
                rightIndex3.transform.parent = rightIndex2.transform;

                rightMiddle1.transform.parent = rightHandIKTarget.transform;
                rightMiddle2.transform.parent = rightMiddle1.transform;
                rightMiddle3.transform.parent = rightMiddle2.transform;

                rightPinky1.transform.parent = rightHandIKTarget.transform;
                rightPinky2.transform.parent = rightPinky1.transform;
                rightPinky3.transform.parent = rightPinky2.transform;

                rightRing1.transform.parent = rightHandIKTarget.transform;
                rightRing2.transform.parent = rightRing1.transform;
                rightRing3.transform.parent = rightRing2.transform;

                rightThumb1.transform.parent = rightHandIKTarget.transform;
                rightThumb2.transform.parent = rightThumb1.transform;
                rightThumb3.transform.parent = rightThumb2.transform;

                // Set Created Hands IK Target To Hands IK Target Reference.
                // Left Hand
                weaponBaseClass.HandsRotationConstraintTransforms.LeftHandIKTransform = leftHandIKTarget.transform;

                weaponBaseClass.HandsRotationConstraintTransforms.LeftHandIndex1ConstraintTransform = leftIndex1.transform;
                weaponBaseClass.HandsRotationConstraintTransforms.LeftHandIndex2ConstraintTransform = leftIndex2.transform;
                weaponBaseClass.HandsRotationConstraintTransforms.LeftHandIndex3ConstraintTransform = leftIndex3.transform;

                weaponBaseClass.HandsRotationConstraintTransforms.LeftHandMiddle1ConstraintTransform = leftMiddle1.transform;
                weaponBaseClass.HandsRotationConstraintTransforms.LeftHandMiddle2ConstraintTransform = leftMiddle2.transform;
                weaponBaseClass.HandsRotationConstraintTransforms.LeftHandMiddle3ConstraintTransform = leftMiddle3.transform;

                weaponBaseClass.HandsRotationConstraintTransforms.LeftHandPinky1ConstraintTransform = leftPinky1.transform;
                weaponBaseClass.HandsRotationConstraintTransforms.LeftHandPinky2ConstraintTransform = leftPinky2.transform;
                weaponBaseClass.HandsRotationConstraintTransforms.LeftHandPinky3ConstraintTransform = leftPinky3.transform;

                weaponBaseClass.HandsRotationConstraintTransforms.LeftHandRing1ConstraintTransform = leftRing1.transform;
                weaponBaseClass.HandsRotationConstraintTransforms.LeftHandRing2ConstraintTransform = leftRing2.transform;
                weaponBaseClass.HandsRotationConstraintTransforms.LeftHandRing3ConstraintTransform = leftRing3.transform;

                weaponBaseClass.HandsRotationConstraintTransforms.LeftHandThumb1ConstraintTransform = leftThumb1.transform;
                weaponBaseClass.HandsRotationConstraintTransforms.LeftHandThumb2ConstraintTransform = leftThumb2.transform;
                weaponBaseClass.HandsRotationConstraintTransforms.LeftHandThumb3ConstraintTransform = leftThumb3.transform;

                // Right Hand
                weaponBaseClass.HandsRotationConstraintTransforms.RightHandIKTransform = rightHandIKTarget.transform;

                weaponBaseClass.HandsRotationConstraintTransforms.RightHandIndex1ConstraintTransform = rightIndex1.transform;
                weaponBaseClass.HandsRotationConstraintTransforms.RightHandIndex2ConstraintTransform = rightIndex2.transform;
                weaponBaseClass.HandsRotationConstraintTransforms.RightHandIndex3ConstraintTransform = rightIndex3.transform;

                weaponBaseClass.HandsRotationConstraintTransforms.RightHandMiddle1ConstraintTransform = rightMiddle1.transform;
                weaponBaseClass.HandsRotationConstraintTransforms.RightHandMiddle2ConstraintTransform = rightMiddle2.transform;
                weaponBaseClass.HandsRotationConstraintTransforms.RightHandMiddle3ConstraintTransform = rightMiddle3.transform;

                weaponBaseClass.HandsRotationConstraintTransforms.RightHandPinky1ConstraintTransform = rightPinky1.transform;
                weaponBaseClass.HandsRotationConstraintTransforms.RightHandPinky2ConstraintTransform = rightPinky2.transform;
                weaponBaseClass.HandsRotationConstraintTransforms.RightHandPinky3ConstraintTransform = rightPinky3.transform;

                weaponBaseClass.HandsRotationConstraintTransforms.RightHandRing1ConstraintTransform = rightRing1.transform;
                weaponBaseClass.HandsRotationConstraintTransforms.RightHandRing2ConstraintTransform = rightRing2.transform;
                weaponBaseClass.HandsRotationConstraintTransforms.RightHandRing3ConstraintTransform = rightRing3.transform;

                weaponBaseClass.HandsRotationConstraintTransforms.RightHandThumb1ConstraintTransform = rightThumb1.transform;
                weaponBaseClass.HandsRotationConstraintTransforms.RightHandThumb2ConstraintTransform = rightThumb2.transform;
                weaponBaseClass.HandsRotationConstraintTransforms.RightHandThumb3ConstraintTransform = rightThumb3.transform;
            }

            // Set the parent for both LeftHandIKTarget and RightHandIKTarget
            leftHandIKTarget.transform.parent = handsIKTargets.transform;
            rightHandIKTarget.transform.parent = handsIKTargets.transform;

            // Set Parent, Position & Rotation to parent transform.
            handsIKTargets.transform.parent = parentTransform;

            // Only Set the position and rotation if parent transform is not null
            if (parentTransform != null)
            {
                handsIKTargets.transform.position = parentTransform.position;
                handsIKTargets.transform.rotation = parentTransform.rotation;
            }

            Selection.activeGameObject = handsIKTargets;
        }

        /// <summary>
        /// Handles the creation of a Weapon SO asset & returns true if created and false if it was cancelled.
        /// </summary>
        /// <param name="folderPanelPrompt"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        private bool CreateWeaponSOAsset(string folderPanelPrompt, string assetName)
        {
            // Get the path to Create the Scriptable Object.
            string selectedPath = EditorUtility.OpenFolderPanel(folderPanelPrompt, "Assets", "");

            if (string.IsNullOrEmpty(selectedPath))
            {
                // User Canceled The Folder Selection For Creation.
                Debug.LogWarning("Folder Selection Canceled For creating a Weapon Scriptable Object.");

                // Return false because user has Canceled it.
                return false;
            }

            string relativePath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
            string baseAssetPath = relativePath + "/" + assetName;
            string assetPath = AssetDatabase.GenerateUniqueAssetPath(baseAssetPath + ".asset");

            // Make an Instance for the Weapon Scriptable Object (SO) Settings.
            WeaponSO weaponSO = CreateInstance<WeaponSO>();

            // Create The Asset.
            AssetDatabase.CreateAsset(weaponSO, assetPath);

            // Save Assets.
            AssetDatabase.SaveAssets();

            // Refresh the Asset Database.
            AssetDatabase.Refresh();

            // Set the Selection to the Weapon SO.
            Selection.activeObject = weaponSO;

            // Focus Project Window.
            EditorUtility.FocusProjectWindow();

            // Ping Object Effect.
            EditorGUIUtility.PingObject(weaponSO);

            // Set the reference to the newly created SO.
            weaponBaseClass.WeaponData = weaponSO;

            // Return true because we created a New Scriptable Object.
            return true;
        }

        #endregion
    }

    #endregion

    #region Weapon Creator Window

    // Weapon Creator Window.
    public class WeaponCreator : EditorWindow
    {

        #region Variables

        // The Main Mesh of the Weapon.
        private GameObject mainMeshParent = null;

        // Name of the Weapon
        private string weaponName = "";

        #endregion

        // Show and open the window from the Menu, (Make a menu item).
        [MenuItem("Tools/HK Fps/Weapon/Weapon Creator")]
        public static void ShowWindow()
        {
            GetWindow<WeaponCreator>("WeaponCreator");
        }

        // OnGUI.
        private void OnGUI()
        {
            // Title
            GUILayout.Label("Welcome to HK Fps Weapon Creator", EditorStyles.boldLabel);

            // 10 pixels space.
            GUILayout.Space(10);

            // Label, tell the user to put a reference of there main model mesh.
            GUILayout.Label("Drag your weapon's main mesh parent into the field");

            // Main Mesh GameObject Field.
            mainMeshParent = EditorGUILayout.ObjectField(mainMeshParent, typeof(GameObject), true) as GameObject;

            // 10 pixels space
            GUILayout.Space(10);

            // Label, The Name For your weapon.
            GUILayout.Label("The Name You Want For Your Weapon");

            // Weapon name
            weaponName = EditorGUILayout.TextField(weaponName);

            // 10 pixels space
            GUILayout.Space(10);

            // Draw a Button for Creating the Weapon.
            if (GUILayout.Button("Create Weapon", GUILayout.Height(25)))
            {
                // Check if the Main Mesh Parent isn't.
                if (mainMeshParent == null)
                {
                    Debug.LogWarning("Main Mesh Parent wasn't assigned, Please assign your weapons main mesh parent to this field");
                    return;
                }

                // Check if the weapon has something.
                if (weaponName != "")
                {
                    // Create Weapon.
                    // Create empty gameobject for the weapon.
                    GameObject weapon = new GameObject(weaponName);

                    // Set the Main Mesh Parent's Name
                    mainMeshParent.name = "Mesh";

                    // Set the Position & Rotation.
                    weapon.transform.position = mainMeshParent.transform.position;
                    weapon.transform.rotation = mainMeshParent.transform.rotation;

                    // Parent Handling.
                    weapon.transform.parent = mainMeshParent.transform.parent;
                    mainMeshParent.transform.parent = weapon.transform;

                    // Add WeaponBase script.
                    WeaponBase weaponBaseClass = weapon.AddComponent<WeaponBase>();

                    // Open Setup helper window.
                    SetupHelperEditorWindow.OpenWindow(weapon.transform, weaponBaseClass);

                    // Select the weapon
                    Selection.activeGameObject = weapon;

                    // Close this window as it is no longer needed.
                    Close();
                }
                else
                {
                    // Tell the user to make sure to set the weapon's name before creating.
                    Debug.LogWarning("Please Set the Weapon's Name Before Creating");
                    return;
                }

            }
        }
    }

    #endregion

    #region Custom Inspector Drawing


    public override void OnInspectorGUI()
    {
        // base Inspector
        base.OnInspectorGUI();

        // Space
        EditorGUILayout.Space();

        // Cast.
        WeaponBase weaponBaseReference = (WeaponBase)target;

        EditorGUILayout.LabelField("Weapon Setup Window", EditorStyles.boldLabel);

        // Draw Weapon Setup Window Button.
        if (GUILayout.Button("Open Weapon Setup Window"))
        {
            // Open Creation Window.
            SetupHelperEditorWindow.OpenWindow(weaponBaseReference.transform, weaponBaseReference);
        }
    }

    #endregion
}
#endif
#endregion

public struct ShootResult
{
    public bool ShootActionSuccess;
    public bool BumpRotationEffect;

    public ShootResult(bool shootActionSuccess, bool bumpRotationEffect)
    {
        ShootActionSuccess = shootActionSuccess;
        BumpRotationEffect = bumpRotationEffect;
    }
}
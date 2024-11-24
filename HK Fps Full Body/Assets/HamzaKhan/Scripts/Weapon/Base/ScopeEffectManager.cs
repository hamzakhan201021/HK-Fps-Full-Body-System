using UnityEngine.Rendering.Universal;
using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class ScopeEffectManager : MonoBehaviour
{

    #region Variables

    // Weapon Ref
    [Space]
    [Header("Weapon")]
    [SerializeField] private WeaponBase _weapon;

    [Tooltip("Is There Any Animation That Plays When A Shot Occurs?")]
    [SerializeField] private bool _animationOnShoot = false;

    // Camera ref.
    [Space]
    [Header("Camera")]
    [SerializeField] private Camera _renderCamera;
    [SerializeField] private CinemachineCamera _aimingVirtualCamera;

    [SerializeField] private AnimationCurve _cameraBlendingCurve;

    [SerializeField] private CinemachineBlendDefinition.Styles _cameraBlendingStyle;

    [SerializeField] private float _cameraBlendTime;
    [SerializeField] private float _delayTimeMultiplier = 1.25f;
    [SerializeField] private float _cameraRotationSmoothingSpeed = 15f;

    // Scope Renderer ref.
    [Space]
    [Header("Scope Renderer")]
    [SerializeField] private Renderer _scopeRenderer;
    [Header("Material To Apply Texture")]
    [SerializeField] private Material _materialToCopy;

    // Render Texture.
    [Space]
    [Header("Render Texture")]
    [SerializeField] private int _height = 512;
    [SerializeField] private int _width = 512;
    [SerializeField] private int _depth = 24;

    // Vignette Effect.
    [Space]
    [Header("Vignette Effect")]
    [SerializeField] private float _intensity = 0.7f;
    [SerializeField] private float _smoothness = 0.1f;

    [Space(5)]
    [Header("Colors")]
    [SerializeField] private Color _aimingVignetteColor = Color.black;
    [SerializeField] private Color _notAimingVignetteColor = Color.white;

    [Space(5)]
    [Header("Effect Delay's & Durations")]
    [SerializeField] private float _changeToAimingColorTime = 0.1f;
    [SerializeField] private float _changeToNotAimingColorTime = 0.1f;
    [SerializeField] private float _toAimingEffectDelay = 0.2f;
    [SerializeField] private float _toNotAimingEffectDelay = 0f;

    // The Vignette Effect.
    private Vignette _vignette;

    // Render Texture Variables.
    private RenderTexture _renderTexture;
    private Material _renderTextureMaterial;

    // Courotine
    private Coroutine _ChangeVignetteColorCourotine;
    private Coroutine _OnShootCoroutineHandler;
    private Coroutine _OnReloadCoroutineHandler;

    private CoroutineState _onShootCoroutineState = CoroutineState.notInProgress;
    private CoroutineState _onReloadCoroutineState = CoroutineState.notInProgress;

    private enum CoroutineState
    {
        inProgress,
        notInProgress,
    }

    // Aiming.
    private bool _isAiming = false;

    // Scoped.
    private bool _isScoped = false;

    // Is Clipped.
    private bool _isClipped = false;

    private CinemachineBrain _cinemachineCameraBrain;

    #endregion

    #region General

    // Start is called before the first frame update
    void Start()
    {
        // Initialize.
        Initialize();
    }

    /// <summary>
    /// All Initialization is done in this function.
    /// </summary>
    private void Initialize()
    {
        // Setup Render Texture.
        SetupRenderTexture();

        // Get vignette Effect from volume.
        GlobalVolumeInstance.Instance.GetGlobalVolumeRef().profile.TryGet(out _vignette);

        // Assign Cinemachine Brain reference.
        _cinemachineCameraBrain = CinemachineBrainInstance.Instance.GetCinemachineBrain();

        // Listen to Weapon Events.
        _weapon.OnShoot.AddListener(OnShoot);
        _weapon.OnReloadStart.AddListener(OnReload);
        _weapon.OnAimEnter.AddListener(OnAim);
        _weapon.OnAimExit.AddListener(OnAimExit);
        _weapon.OnWeaponStartOrStopUse.AddListener(OnCheckForScopeEnable);

        // Check if we should enable camera or disable it.
        //OnCheckForScopeEnable(null);
    }

    void Update()
    {
        if (!_weapon.IsCurrentWeapon())
        {
            return;
        }

        HandleScopeSystem();
        HandleCamerasAndEffects();
    }

    private void HandleScopeSystem()
    {
        if (_weapon == null || _weapon.GetPlayerWeapon() == null) return;

        // Check if the Bool that we have stored is different from the actual one.
        if (_isClipped != _weapon.GetPlayerWeapon().IsClipped())
        {
            // Set our bool to the actual one.
            _isClipped = _weapon.GetPlayerWeapon().IsClipped();

            // Check if the new value is false, This means that it was true, Now it is false.
            if (_isClipped == false) OnClippedExit();
            else OnClippedEnter();
        }

        // Check if we are scoped and not clipped and Both the coroutine's are not in progress.
        if (_isScoped && !_weapon.GetPlayerWeapon().IsClipped() && _onShootCoroutineState == CoroutineState.notInProgress
            && _onReloadCoroutineState == CoroutineState.notInProgress) _aimingVirtualCamera.gameObject.SetActive(true);
        else if (_isScoped && !_weapon.GetPlayerWeapon().IsClipped()) _aimingVirtualCamera.gameObject.SetActive(false);
        else _aimingVirtualCamera.gameObject.SetActive(false);
    }

    private void HandleCamerasAndEffects()
    {
        // Set The Aiming Virtual Camera Rotation
        _aimingVirtualCamera.transform.rotation = Quaternion.Slerp(_aimingVirtualCamera.transform.rotation,
            _weapon.transform.rotation * Quaternion.Euler(0f, 0f, -_weapon.transform.rotation.eulerAngles.z),
            _cameraRotationSmoothingSpeed * Time.deltaTime);

        // Vignette Effect Values.
        TrySetVignetteSettings();

        // Camera Blending Values.
        TrySetBrainSettings();
    }

    private void TrySetVignetteSettings()
    {
        if (_vignette == null) return;

        _vignette.intensity.value = _intensity;
        _vignette.smoothness.value = _smoothness;
    }

    private void TrySetBrainSettings()
    {
        if (_cinemachineCameraBrain == null) return;

        _cinemachineCameraBrain.DefaultBlend.Time = _cameraBlendTime;
        _cinemachineCameraBrain.DefaultBlend.Style = _cameraBlendingStyle;
        _cinemachineCameraBrain.DefaultBlend.CustomCurve = _cameraBlendingCurve;
    }

    #endregion

    #region Scoped Pausing System

    // On Shoot Event.
    private void OnShoot()
    {
        // Return if we don't have any animation to play after a shot.
        if (!_animationOnShoot) return;

        // Check if needed to stop coroutine.
        if (_OnShootCoroutineHandler != null) StopCoroutine(_OnShootCoroutineHandler);

        // Start OnShoot Coroutine.
        _OnShootCoroutineHandler = StartCoroutine(OnShootCoroutine(_weapon.WeaponData.TimeBetweenShot - (_cameraBlendTime * _delayTimeMultiplier)));
    }

    // On Reload Event
    private void OnReload()
    {
        // Check if needed to stop coroutine.
        if (_OnReloadCoroutineHandler != null) StopCoroutine(_OnReloadCoroutineHandler);

        // Start OnRelaod Coroutine.
        _OnReloadCoroutineHandler = StartCoroutine(OnReloadCoroutine(_weapon.WeaponData.ReloadingAnimationTime - (_cameraBlendTime * _delayTimeMultiplier)));
    }

    /// <summary>
    /// On Shoot Coroutine.
    /// </summary>
    /// <param name="timeBetweenShot"></param>
    /// <returns></returns>
    IEnumerator OnShootCoroutine(float timeBetweenShot)
    {
        // In Progress
        _onShootCoroutineState = CoroutineState.inProgress;

        // Aim Exit.
        VignetteOnAimExitHandler();

        // Delay.
        yield return new WaitForSeconds(timeBetweenShot);

        // Check if we are still aiming.
        if (_isAiming == true && !_isClipped)
        {
            // Aiming Vignette Effect.
            VignetteOnAimHandler();
        }

        // No longer in progress.
        _onShootCoroutineState = CoroutineState.notInProgress;
    }

    /// <summary>
    /// On Reload Coroutine
    /// </summary>
    /// <param name="reloadingTime"></param>
    /// <returns></returns>
    IEnumerator OnReloadCoroutine(float reloadingTime)
    {
        // In Progress
        _onReloadCoroutineState = CoroutineState.inProgress;

        // Aim Exit.
        VignetteOnAimExitHandler();

        // Delay.
        yield return new WaitForSeconds(reloadingTime);

        // Check if we are still aiming.
        if (_isAiming == true && !_isClipped)
        {
            // Aiming Vignette Effect.
            VignetteOnAimHandler();
        }

        // No longer in progress.
        _onReloadCoroutineState = CoroutineState.notInProgress;
    }

    #endregion

    #region Render Texture

    /// <summary>
    /// Creates And Set's up a render texture for the scope system.
    /// </summary>
    private void SetupRenderTexture()
    {
        // Initialize a new Render Texture
        _renderTexture = new RenderTexture(_height, _width, _depth);
        _renderTexture.Create();

        // Initialize a new Material For Applying the render texture.
        _renderTextureMaterial = Instantiate(_materialToCopy, transform);
        _renderTextureMaterial.mainTexture = _renderTexture;

        // Initialize the camera by setting the target texture and enabling it.
        _renderCamera.targetTexture = _renderTexture;
        _renderCamera.gameObject.SetActive(true);

        // Set the Renderer's Material.
        _scopeRenderer.material = _renderTextureMaterial;
    }

    #endregion

    #region Events

    /// <summary>
    /// Invoked When We start clipping.
    /// </summary>
    private void OnClippedEnter()
    {
        // Check if we are aiming.
        if (_isAiming) VignetteOnAimExitHandler();
    }

    /// <summary>
    /// Invoked When We were clipped and just stopped clipping.
    /// </summary>
    private void OnClippedExit()
    {
        // Check if we are aiming.
        if (_isAiming) VignetteOnAimHandler();
    }

    /// <summary>
    /// On Aim Event, Call Through the Weapon Base Unity Event.
    /// </summary>
    private void OnAim()
    {
        // Is Aiming is true.
        _isAiming = true;

        // Check if the coroutine isn't running.
        if (_onShootCoroutineState == CoroutineState.notInProgress && _onReloadCoroutineState == CoroutineState.notInProgress && !_isClipped) VignetteOnAimHandler();
    }

    /// <summary>
    /// On Aim Exit Event, Call Through the Weapon Base Unity Event.
    /// </summary>
    private void OnAimExit()
    {
        // Is Aiming is False.
        _isAiming = false;

        // Check if the coroutine isn't running.
        if (_onShootCoroutineState == CoroutineState.notInProgress && _onReloadCoroutineState == CoroutineState.notInProgress) VignetteOnAimExitHandler();
    }

    /// <summary>
    /// On Aim Vignette Effect Handler.
    /// </summary>
    private void VignetteOnAimHandler()
    {
        // Stop the coroutine if already started previously.
        if (_ChangeVignetteColorCourotine != null) StopCoroutine(_ChangeVignetteColorCourotine);

        // Scoped.
        _isScoped = true;

        // Start Change Vignette Coroutine For Aiming Effect.
        _ChangeVignetteColorCourotine =
            StartCoroutine(ChangeVignetteColor(_toAimingEffectDelay, _aimingVignetteColor, _changeToAimingColorTime, true));
    }

    /// <summary>
    /// On Aim Exit Vignette Effect Handler.
    /// </summary>
    private void VignetteOnAimExitHandler()
    {
        // Stop the coroutine if already started previously.
        if (_ChangeVignetteColorCourotine != null) StopCoroutine(_ChangeVignetteColorCourotine);

        // Not scoped.
        _isScoped = false;

        // Start Change Vignette Coroutine For Aiming Exit Effect.
        _ChangeVignetteColorCourotine =
            StartCoroutine(ChangeVignetteColor(_toNotAimingEffectDelay, _notAimingVignetteColor, _changeToNotAimingColorTime, false));
    }

    // CHANGES from player controller to waepon manager
    private void OnCheckForScopeEnable(HKPlayerWeaponManager controller, bool isCurrentWeapon)
    {
        _renderCamera.enabled = isCurrentWeapon;

        if (isCurrentWeapon)
        {
            if (controller.IsAiming()) OnAim();
            else OnAimExit();
        }
        else
        {
            _aimingVirtualCamera.gameObject.SetActive(false);

            if (_isScoped)
            {
                VignetteOnAimExitHandler();

                _isScoped = false;
                _isAiming = false;
            }
        }
    }

    #endregion

    #region Vignette Effect

    /// <summary>
    /// Change Vignette Effect Colour.
    /// </summary>
    /// <param name="effectStartDelay"></param>
    /// <param name="color"></param>
    /// <param name="time"></param>
    /// <param name="vignetteActiveAfter"></param>
    /// <returns></returns>
    IEnumerator ChangeVignetteColor(float effectStartDelay, Color color, float time, bool vignetteActiveAfter)
    {
        // Start Delay.
        yield return new WaitForSeconds(effectStartDelay);

        // Ensure Vignette Effect is enabled.
        _vignette.active = true;

        // Store the initial color.
        Color initialColor = _vignette.color.value;

        // ElapsedTime.
        float elapsedTime = 0f;

        // While we still have time left.
        while (elapsedTime < time)
        {
            // Add time to the elapsed amount.
            elapsedTime += Time.deltaTime;

            // Lerp the Vignette Color.
            _vignette.color.value = Color.Lerp(initialColor, color, elapsedTime / time);

            // Return.
            yield return null;
        }

        // Ensure the final Color is Set.
        _vignette.color.value = color;

        // Disable or enable vignette based on given info.
        _vignette.active = vignetteActiveAfter;
    }

    #endregion
}
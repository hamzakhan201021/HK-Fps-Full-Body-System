using UnityEngine;

namespace HKFps
{
    [RequireComponent(typeof(WeaponBase))]
    public class KnifeAnimations : MonoBehaviour
    {

        [Space]
        [Header("Knife Animator")]
        [SerializeField] private Animator _animator;
        [Space]
        [Header("Animator Parameters")]
        [SerializeField] private string _onWeaponStartUseParamName = "OnWeaponStartUse";
        [SerializeField] private string _onWeaponStopUseParamName = "OnWeaponStopUse";

        private int _onWeaponStartUseParamNameHash;
        private int _onWeaponStopUseParamNameHash;

        private WeaponBase _weapon;

        // Start is called before the first frame update
        void Start()
        {
            _weapon = GetComponent<WeaponBase>();
            AnimatorSetup();
            InitEvents();
        }

        private void AnimatorSetup()
        {
            _onWeaponStartUseParamNameHash = Animator.StringToHash(_onWeaponStartUseParamName);
            _onWeaponStopUseParamNameHash = Animator.StringToHash(_onWeaponStopUseParamName);

            _animator.keepAnimatorStateOnDisable = true;
        }

        private void InitEvents()
        {
            _weapon.OnWeaponStartOrStopUse.AddListener(OnWeaponStartStopUseEvent);
        }

        private void OnWeaponStartStopUseEvent(HKPlayerWeaponManager weaponManager, bool state)
        {
            _animator.ResetTrigger(_onWeaponStartUseParamNameHash);
            _animator.ResetTrigger(_onWeaponStopUseParamNameHash);

            _animator.SetTrigger(state ? _onWeaponStartUseParamNameHash : _onWeaponStopUseParamNameHash);
        }
    }
}
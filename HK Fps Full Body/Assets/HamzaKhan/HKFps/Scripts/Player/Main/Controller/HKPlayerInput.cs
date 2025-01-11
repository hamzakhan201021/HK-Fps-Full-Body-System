namespace HKFps
{
    public class HKPlayerInput : HKPlayerInputBase
    {

        private PlayerInputActions _playerInputActions;

        void Awake()
        {
            _playerInputActions = new PlayerInputActions();
            _playerInputActions.Enable();
        }

        public override PlayerInputActions GetInputActions()
        {
            return _playerInputActions;
        }

        private void OnEnable()
        {
            _playerInputActions.Enable();
        }
        private void OnDisable()
        {
            _playerInputActions.Disable();
        }
    }
}
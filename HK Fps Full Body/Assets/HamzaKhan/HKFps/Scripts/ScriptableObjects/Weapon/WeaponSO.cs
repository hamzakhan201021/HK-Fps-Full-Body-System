using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HKFps
{
    [CreateAssetMenu(fileName = "WeaponScriptableObject", menuName = "ScriptableObjects/WeaponScriptableObject")]
    public class WeaponSO : ScriptableObject
    {
        public CameraShakeProfile ShakeProfile;
        [Space]
        // Weapon Settings Header.
        [Header("Weapon Settings")]

        // Weapon UI Settings.
        [Header("Weapon UI")]
        [HideInInspector] public string WeaponName;

        [HideInInspector] public Sprite WeaponIconSprite;
        [HideInInspector] public Sprite CrossHairSprite;

        [HideInInspector] public float WeaponIconImageHeight;
        [HideInInspector] public float WeaponIconImageWidth;

        [Space]
        [HideInInspector] public Vector3 WeaponUIPositionOffset = Vector3.zero;
        [HideInInspector] public Vector3 WeaponUIRotationOffset = Vector3.zero;

        // Weapon Mechanics Settings
        [Header("Weapon Mechanics & Type")]
        [HideInInspector] public float TimeBetweenShot;
        [HideInInspector] public float ReloadingAnimationTime;

        [HideInInspector] public float MaxBulletSpreadAngle = 5f;
        [HideInInspector] public float ShotBumpRotationAmount = 2f;

        [HideInInspector] public int MagazineSize;

        [HideInInspector] public WeaponType WeaponType;
        [HideInInspector] public FiringType FiringType;

        [HideInInspector] public float MaxShootRange;
        [HideInInspector] public float ProjectileForce;

        [HideInInspector] public ForceMode RigidbodyForceMode;

        [HideInInspector] public GameObject ProjectilePrefab;

        [HideInInspector] public bool EjectShellOnShoot = true;

        [Tooltip("Enable to make the weapon heating system take Affect, " +
            "You can add the Heat Effects Manager component to your weapon for heat effects.")]
        [HideInInspector] public bool EnableHeatingSystem = true;
        [HideInInspector] public float ExtraHeatIncreasePerShot = 1.25f;
        [HideInInspector] public float ConstantHeatCooldownRatePerSecond = 2f;
        [HideInInspector] public float ExtraHeatCoolingThreshold = 25f;
        [HideInInspector] public float ExtraOverheatThreshold = 40f;

        // Weapon Clipping Settings.
        [Space]
        [Header("Weapon Clipping Settings")]
        [HideInInspector] public Vector3 ClipProjectorPosition;

        [HideInInspector] public Vector3 NewPosition;
        [HideInInspector] public Vector3 NewRotation;

        [HideInInspector] public Vector3 BoxCastSize;
        [HideInInspector] public Vector3 BoxCastClippedSize;

        // Camera Positions
        [Space]
        [Header("Camera Positions")]
        [HideInInspector] public Vector3 CameraNormalPosition;
        [HideInInspector] public Vector3 CameraAimingPosition;
        [HideInInspector] public Vector3 CameraClippedPosition;

        // Recoil Settings
        [Space]
        [Header("Recoil & Sway Settings")]
        [HideInInspector] public float RecoilX;
        [HideInInspector] public float RecoilY;
        [HideInInspector] public float RecoilZ;
        [HideInInspector] public float Snappiness;
        [HideInInspector] public float ReturnSpeed;

        [HideInInspector] public Vector3 CinemachineRecoilImpulse;
        [HideInInspector] public float SwayAmount = 1.5f;
        [HideInInspector] public float SwaySmooth = 5f;

        // Weapon Offsets
        [Header("Player Offsets")]
        [Header("Position Offset added when childed to gunholder")]
        [HideInInspector] public Vector3 WeaponPositionOffset;

        [HideInInspector] public float SpineConstraintOffsetY;
    }

#if UNITY_EDITOR

    [CanEditMultipleObjects]
    [CustomEditor(typeof(WeaponSO))]
    public class WeaponSOEditor : Editor
    {
        // SerializedProperty fields.

        // Weapon UI Settings.
        SerializedProperty WeaponName;
        SerializedProperty WeaponIconSprite;
        SerializedProperty CrossHairSprite;
        SerializedProperty WeaponIconImageHeight;
        SerializedProperty WeaponIconImageWidth;
        SerializedProperty WeaponUIPositionOffset;
        SerializedProperty WeaponUIRotationOffset;

        // Weapon Mechanics Settings
        SerializedProperty TimeBetweenShot;
        SerializedProperty ReloadingAnimationTime;
        SerializedProperty MaxBulletSpreadAngle;
        SerializedProperty ShotBumpRotationAmount;
        SerializedProperty MaxShootRange;
        SerializedProperty ProjectileForce;
        SerializedProperty MagazineSize;
        SerializedProperty WeaponType;
        SerializedProperty FiringType;

        SerializedProperty RigidbodyForceMode;

        SerializedProperty ProjectilePrefab;

        SerializedProperty EjectShellOnShoot;

        SerializedProperty EnableHeatingSystem;
        SerializedProperty ExtraHeatIncreasePerShot;
        SerializedProperty ConstantHeatCooldownRatePerSecond;
        SerializedProperty ExtraHeatCoolingThreshold;
        SerializedProperty ExtraOverheatThreshold;

        // Weapon Clipping Settings.
        SerializedProperty ClipProjectorPosition;
        SerializedProperty NewPosition;
        SerializedProperty NewRotation;
        SerializedProperty BoxCastSize;
        SerializedProperty BoxCastClippedSize;

        // Camera Positions
        SerializedProperty CameraNormalPosition;
        SerializedProperty CameraAimingPosition;
        SerializedProperty CameraClippedPosition;

        // Recoil Settings
        SerializedProperty RecoilX;
        SerializedProperty RecoilY;
        SerializedProperty RecoilZ;
        SerializedProperty Snappiness;
        SerializedProperty ReturnSpeed;
        SerializedProperty CinemachineRecoilImpulse;
        SerializedProperty SwayAmount;
        SerializedProperty SwaySmooth;

        // Weapon Offsets
        SerializedProperty WeaponPositionOffset;
        SerializedProperty SpineConstraintOffsetY;

        // OnEnable.
        private void OnEnable()
        {
            // Get all the properties.
            GetSerializedPropertyReferences();
        }

        /// <summary>
        /// Gets all SerializedProperty references.
        /// </summary>
        private void GetSerializedPropertyReferences()
        {
            // Weapon UI Settings.
            WeaponName = serializedObject.FindProperty("WeaponName");
            WeaponIconSprite = serializedObject.FindProperty("WeaponIconSprite");
            CrossHairSprite = serializedObject.FindProperty("CrossHairSprite");
            WeaponIconImageHeight = serializedObject.FindProperty("WeaponIconImageHeight");
            WeaponIconImageWidth = serializedObject.FindProperty("WeaponIconImageWidth");
            WeaponUIPositionOffset = serializedObject.FindProperty("WeaponUIPositionOffset");
            WeaponUIRotationOffset = serializedObject.FindProperty("WeaponUIRotationOffset");

            // Weapon Mechanics Settings
            TimeBetweenShot = serializedObject.FindProperty("TimeBetweenShot");
            ReloadingAnimationTime = serializedObject.FindProperty("ReloadingAnimationTime");
            MaxBulletSpreadAngle = serializedObject.FindProperty("MaxBulletSpreadAngle");
            ShotBumpRotationAmount = serializedObject.FindProperty("ShotBumpRotationAmount");
            MaxShootRange = serializedObject.FindProperty("MaxShootRange");
            ProjectileForce = serializedObject.FindProperty("ProjectileForce");
            MagazineSize = serializedObject.FindProperty("MagazineSize");
            WeaponType = serializedObject.FindProperty("WeaponType");
            FiringType = serializedObject.FindProperty("FiringType");

            RigidbodyForceMode = serializedObject.FindProperty("RigidbodyForceMode");

            ProjectilePrefab = serializedObject.FindProperty("ProjectilePrefab");

            EjectShellOnShoot = serializedObject.FindProperty("EjectShellOnShoot");

            EnableHeatingSystem = serializedObject.FindProperty("EnableHeatingSystem");
            ExtraHeatIncreasePerShot = serializedObject.FindProperty("ExtraHeatIncreasePerShot");
            ConstantHeatCooldownRatePerSecond = serializedObject.FindProperty("ConstantHeatCooldownRatePerSecond");
            ExtraHeatCoolingThreshold = serializedObject.FindProperty("ExtraHeatCoolingThreshold");
            ExtraOverheatThreshold = serializedObject.FindProperty("ExtraOverheatThreshold");

            // Recoil Settings
            ClipProjectorPosition = serializedObject.FindProperty("ClipProjectorPosition");
            NewPosition = serializedObject.FindProperty("NewPosition");
            NewRotation = serializedObject.FindProperty("NewRotation");
            BoxCastSize = serializedObject.FindProperty("BoxCastSize");
            BoxCastClippedSize = serializedObject.FindProperty("BoxCastClippedSize");

            // Camera Positions
            CameraNormalPosition = serializedObject.FindProperty("CameraNormalPosition");
            CameraAimingPosition = serializedObject.FindProperty("CameraAimingPosition");
            CameraClippedPosition = serializedObject.FindProperty("CameraClippedPosition");

            // Recoil Settings
            RecoilX = serializedObject.FindProperty("RecoilX");
            RecoilY = serializedObject.FindProperty("RecoilY");
            RecoilZ = serializedObject.FindProperty("RecoilZ");
            Snappiness = serializedObject.FindProperty("Snappiness");
            ReturnSpeed = serializedObject.FindProperty("ReturnSpeed");
            CinemachineRecoilImpulse = serializedObject.FindProperty("CinemachineRecoilImpulse");
            SwayAmount = serializedObject.FindProperty("SwayAmount");
            SwaySmooth = serializedObject.FindProperty("SwaySmooth");

            // Weapon Offsets
            WeaponPositionOffset = serializedObject.FindProperty("WeaponPositionOffset");
            SpineConstraintOffsetY = serializedObject.FindProperty("SpineConstraintOffsetY");
        }

        // override Inspector method.
        public override void OnInspectorGUI()
        {
            // draw default inspector.
            base.OnInspectorGUI();

            // update the serializedObject Update.
            serializedObject.Update();

            // Draw properties

            // Weapon UI Settings.
            DrawPropertyField(WeaponName);
            DrawPropertyField(WeaponIconSprite);
            DrawPropertyField(CrossHairSprite);
            DrawPropertyField(WeaponIconImageHeight);
            DrawPropertyField(WeaponIconImageWidth);
            DrawPropertyField(WeaponUIPositionOffset);
            DrawPropertyField(WeaponUIRotationOffset);

            // Weapon Mechanics Settings
            DrawPropertyField(TimeBetweenShot);
            DrawPropertyField(ReloadingAnimationTime);
            DrawPropertyField(MaxBulletSpreadAngle);
            DrawPropertyField(ShotBumpRotationAmount);
            DrawPropertyField(MagazineSize);
            DrawPropertyField(WeaponType);
            DrawPropertyField(FiringType);

            // check if firing type is raycast.
            if (FiringType.enumValueIndex == 0f)
            {
                // draw maxshoot range.
                DrawPropertyField(MaxShootRange);
            }
            else
            {
                // draw projectile fields.
                DrawPropertyField(ProjectileForce);

                DrawPropertyField(RigidbodyForceMode);

                DrawPropertyField(ProjectilePrefab);
            }

            DrawPropertyField(EjectShellOnShoot);

            // Heating Settings
            DrawPropertyField(EnableHeatingSystem);

            if (EnableHeatingSystem.boolValue == true)
            {
                DrawPropertyField(ExtraHeatIncreasePerShot);
                DrawPropertyField(ConstantHeatCooldownRatePerSecond);
                DrawPropertyField(ExtraHeatCoolingThreshold);
                DrawPropertyField(ExtraOverheatThreshold);
            }

            // Weapon Clipping Settings.
            DrawPropertyField(ClipProjectorPosition);
            DrawPropertyField(NewPosition);
            DrawPropertyField(NewRotation);
            DrawPropertyField(BoxCastSize);
            DrawPropertyField(BoxCastClippedSize);

            // Camera Settings
            DrawPropertyField(CameraNormalPosition);
            DrawPropertyField(CameraAimingPosition);
            DrawPropertyField(CameraClippedPosition);

            // Recoil Settings
            DrawPropertyField(RecoilX);
            DrawPropertyField(RecoilY);
            DrawPropertyField(RecoilZ);
            DrawPropertyField(Snappiness);
            DrawPropertyField(ReturnSpeed);
            DrawPropertyField(CinemachineRecoilImpulse);
            DrawPropertyField(SwayAmount);
            DrawPropertyField(SwaySmooth);

            DrawPropertyField(WeaponPositionOffset);
            DrawPropertyField(SpineConstraintOffsetY);

            // apply the modified properties.
            serializedObject.ApplyModifiedProperties();
        }

        // draw Property field function.
        private void DrawPropertyField(SerializedProperty property)
        {
            // draw a property.
            EditorGUILayout.PropertyField(property);
        }
    }
#endif
}
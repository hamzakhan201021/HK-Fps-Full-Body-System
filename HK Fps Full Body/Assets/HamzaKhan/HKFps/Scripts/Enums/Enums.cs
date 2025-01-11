namespace HKFps
{
    public enum FiringType
    {
        raycast,
        projectile
    }
    public enum HandsConstraintType
    {
        IKBasedFingers,
        RotationConstraintBasedFingers,
    }
    public enum MatchingType
    {
        local,
        world,
    }
    public enum LocomotionState
    {
        walking,
        sprinting,
        crouching,
        proning,
    }
    public enum AimState
    {
        normal,
        aiming,
    }
    public enum AirState
    {
        grounded,
        inAir,
    }
    public enum WeaponClippedState
    {
        clipped,
        normal,
    }
    public enum WeaponHeatState
    {
        cool,
        cooling,
        overHeated,
    }
    public enum WeaponSwitchType
    {
        SwitchBetweenRifle,
        SwitchBetweenPistolOrKnife,
        SwitchFromPistolOrKnifeToRifle,
        SwitchFromRifleToPistol,
        SwitchFromRifleToKnife,
    }
    public enum WeaponType
    {
        rifle,
        pistol,
        knife
    }
    public enum ItemStartUseBehaviour
    {
        MoveToLeftHand,
        DisableWeapon,
    }
    public enum BodyPart
    {
        Head,
        Body,
        Arm,
        Leg,
    }
}
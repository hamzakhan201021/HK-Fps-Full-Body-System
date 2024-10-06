using UnityEngine;
using UnityEditor;
using UnityEngine.Animations.Rigging;
using UnityEngine.ProBuilder.Shapes;

public class CharacterCreatorSwitcherSystemWindow : EditorWindow
{

    [MenuItem("Window/DEMOS/CharacterCreatorSwitcher")]
    public static void ShowWindow()
    {
        GetWindow<CharacterCreatorSwitcherSystemWindow>();
    }

    private GameObject playerModel = null;    

    private void OnGUI()
    {
        GUILayout.Space(10);

        GUIStyle gUIStyle = new GUIStyle();

        gUIStyle.fontStyle = FontStyle.Bold;
        gUIStyle.normal.textColor = Color.white;
        gUIStyle.alignment = TextAnchor.MiddleCenter;
        gUIStyle.fontSize = 22;

        GUILayout.Label("HK FPS Character Creator/Switcher!", gUIStyle);

        GUILayout.Space(10);

        GUILayout.Label("Assign your Player Model With the Animator");

        // (MUHAMMAD NOTE) This is how to make an object field of any class type (rigidbody, scripts etc.)
        playerModel = EditorGUILayout.ObjectField(playerModel, typeof(GameObject), true) as GameObject;

        GUILayout.Space(10);

        // (MUHAMMAD NOTE) This is how to add a button and trigger a function on it's click
        if (GUILayout.Button("Setup Character"))
        {
            SetupCharacter();
        }

        

        /* (MUHAMMAD NOTE) Use your brain, whatever you want to do will almost always 
         * be in the GUILayout.WHATEVER(); 
         * or EditorGUILayout 
         * or EditorGUI*/
    }


    private string rigWeightSetterRigPropertyName = "rig";
    private string rigLayersPropertyName = "m_RigLayers";
    private string rigLayersRigPropertyName = "m_Rig";
    private string rigLayersActivePropertyName = "m_Active";
    private string _iKConstraintsPropertyName = "_iKConstraints";

    private string _playerUIPropertyName = "_playerUI";
    private string _playerAnimatorName = "_playerAnimator";
    private string _gunHolderAnimatorName = "_gunHolderAnimator";
    private string _centerSpinePosName = "_centerSpinePos";
    private string _cameraHolderName = "_cameraHolder";
    private string _headTransformName = "_headTransform";
    private string _gunHolderName = "_gunHolder";
    private string _iKBasedFingersWeightModifierName = "_iKBasedFingersWeightModifier";
    private string _rotationConstraitBasedFingersWeightModifierName = "_rotationConstraitBasedFingersWeightModifier";

    private void SetupCharacter()
    {
        if (playerModel == null)
        {
            Debug.LogWarning("Player Model has not been assigned, please ensure to assign the player model before you setup Character");
            return;
        }

        #region Player & PlayerModel
        // (MUHAMMAD NOTE) This is how to create a game object
        GameObject playerParentGameObject = new GameObject("HKFpsPlayer");
        GameObject rig = new GameObject("Rig 1");

        // (MUHAMMAD NOTE) This is how we add a component/script to a game object.
        playerParentGameObject.AddComponent<CharacterController>();
        playerParentGameObject.AddComponent<PlayerController>();
        playerParentGameObject.AddComponent<PlayerUI>();



        playerModel.transform.SetParent(playerParentGameObject.transform);
        playerModel.AddComponent<Animator>();
        Animator playerModelAnimator = playerModel.GetComponent<Animator>();
        playerModelAnimator.applyRootMotion = false;

        playerModel.AddComponent<PlayerModel>();
        playerModel.AddComponent<RigBuilder>();

        rig.transform.SetParent(playerModel.transform);

        rig.AddComponent<Rig>();

        //SerializedObject playerControllerSObj = new SerializedObject(playerParentGameObject.GetComponent<PlayerController>());
        //SerializedProperty playerUIProperty = playerControllerSObj.FindProperty(_playerUIPropertyName);
        //playerUIProperty.objectReferenceValue = playerParentGameObject.GetComponent<PlayerUI>();
        //playerControllerSObj.ApplyModifiedProperties();

        //SerializedObject rigWeightSetterSObj = new SerializedObject(rig.AddComponent<RigWeightSetter>());
        //SerializedProperty rigProperty = rigWeightSetterSObj.FindProperty(rigWeightSetterRigPropertyName);
        //rigProperty.objectReferenceValue = rig.GetComponent<Rig>();
        //rigWeightSetterSObj.ApplyModifiedProperties();

       

        // /// /// //
        SerializedObject rigBuilderSObj = new SerializedObject(playerModel.GetComponent<RigBuilder>());
        SerializedProperty rigLayersProperty = rigBuilderSObj.FindProperty(rigLayersPropertyName);

        rigLayersProperty.arraySize++;
        SerializedProperty newElement = rigLayersProperty.GetArrayElementAtIndex(rigLayersProperty.arraySize - 1);

        newElement.FindPropertyRelative(rigLayersRigPropertyName).objectReferenceValue = rig.GetComponent<Rig>();
        newElement.FindPropertyRelative(rigLayersActivePropertyName).boolValue = true;

        rigBuilderSObj.ApplyModifiedProperties();


        #endregion

        #region GameObject's Creation
        GameObject body = new GameObject("Body");
        GameObject hands = new GameObject("Hands");

        GameObject spineRotRig = new GameObject("SpineRotRig");
        GameObject spine1RotRig = new GameObject("Spine1RotRig");
        GameObject spine2RotRig = new GameObject("Spine2RotRig");
        GameObject headAimRig = new GameObject("HeadAimRig");

        GameObject iKBasedFingersWeight = new GameObject("IKBasedFingersWeight");
        GameObject rotationConstrainedFingersWeight = new GameObject("RotationConstrainedFingersWeight");
        GameObject handsIKWeight = new GameObject("HandsIKWeightModifier");
        GameObject leftHandIK = new GameObject("LeftHandIK");
        GameObject rightHandIK = new GameObject("RightHandIK");


        GameObject directRotationMatchFingersL = new GameObject("DirectRotationMatchFingers");
        GameObject iKBasedFingersL = new GameObject("IKBasedFingers");

        GameObject directRotationMatchFingersR = new GameObject("DirectRotationMatchFingers");
        GameObject iKBasedFingersR = new GameObject("IKBasedFingers");



        GameObject leftHandIndex1Constraint = new GameObject("LeftHandIndex1Constraint");
        GameObject leftHandIndex2Constraint = new GameObject("LeftHandIndex2Constraint");
        GameObject leftHandIndex3Constraint = new GameObject("LeftHandIndex3Constraint");

        GameObject leftHandMiddle1Constraint = new GameObject("LeftHandMiddle1Constraint");
        GameObject leftHandMiddle2Constraint = new GameObject("LeftHandMiddle2Constraint");
        GameObject leftHandMiddle3Constraint = new GameObject("LeftHandMiddle3Constraint");

        GameObject leftHandPinky1Constraint = new GameObject("LeftHandPinky1Constraint");
        GameObject leftHandPinky2Constraint = new GameObject("LeftHandPinky2Constraint");
        GameObject leftHandPinky3Constraint = new GameObject("LeftHandPinky3Constraint");

        GameObject leftHandRing1Constraint = new GameObject("LeftHandRing1Constraint");
        GameObject leftHandRing2Constraint = new GameObject("LeftHandRing2Constraint");
        GameObject leftHandRing3Constraint = new GameObject("LeftHandRing3Constraint");

        GameObject leftHandThumb1Constraint = new GameObject("LeftHandThumb1Constraint");
        GameObject leftHandThumb2Constraint = new GameObject("LeftHandThumb2Constraint");
        GameObject leftHandThumb3Constraint = new GameObject("LeftHandThumb3Constraint");

        GameObject leftHandIndexIK = new GameObject("LeftHandIndexIK");
        GameObject leftHandMiddleIK = new GameObject("LeftHandMiddleIK");
        GameObject leftHandPinkyIK = new GameObject("LeftHandPinkyIK");
        GameObject leftHandRingIK = new GameObject("LeftHandRingIK");
        GameObject leftHandThumbIK = new GameObject("LeftHandThumbIK");



        GameObject rightHandIndex1Constraint = new GameObject("RightHandIndex1Constraint");
        GameObject rightHandIndex2Constraint = new GameObject("RightHandIndex2Constraint");
        GameObject rightHandIndex3Constraint = new GameObject("RightHandIndex3Constraint");

        GameObject rightHandMiddle1Constraint = new GameObject("RightHandMiddle1Constraint");
        GameObject rightHandMiddle2Constraint = new GameObject("RightHandMiddle2Constraint");
        GameObject rightHandMiddle3Constraint = new GameObject("RightHandMiddle3Constraint");

        GameObject rightHandPinky1Constraint = new GameObject("RightHandPinky1Constraint");
        GameObject rightHandPinky2Constraint = new GameObject("RightHandPinky2Constraint");
        GameObject rightHandPinky3Constraint = new GameObject("RightHandPinky3Constraint");

        GameObject rightHandRing1Constraint = new GameObject("RightHandRing1Constraint");
        GameObject rightHandRing2Constraint = new GameObject("RightHandRing2Constraint");
        GameObject rightHandRing3Constraint = new GameObject("RightHandRing3Constraint");

        GameObject rightHandThumb1Constraint = new GameObject("RightHandThumb1Constraint");
        GameObject rightHandThumb2Constraint = new GameObject("RightHandThumb2Constraint");
        GameObject rightHandThumb3Constraint = new GameObject("RightHandThumb3Constraint");

        GameObject rightHandIndexIK = new GameObject("RightHandIndexIK");
        GameObject rightHandMiddleIK = new GameObject("RightHandMiddleIK");
        GameObject rightHandPinkyIK = new GameObject("RightHandPinkyIK");
        GameObject rightHandRingIK = new GameObject("RightHandRingIK");
        GameObject rightHandThumbIK = new GameObject("RightHandThumbIK");


        //

        GameObject leftHandConstraintIndex1Target = new GameObject("LeftHandIndex1ConstraintTarget");
        GameObject leftHandConstraintIndex2Target = new GameObject("LeftHandIndex2ConstraintTarget");
        GameObject leftHandConstraintIndex3Target = new GameObject("LeftHandIndex3ConstraintTarget");

        GameObject leftHandConstraintMiddle1Target = new GameObject("LeftHandMiddle1ConstraintTarget");
        GameObject leftHandConstraintMiddle2Target = new GameObject("LeftHandMiddle2ConstraintTarget");
        GameObject leftHandConstraintMiddle3Target = new GameObject("LeftHandMiddle3ConstraintTarget");

        GameObject leftHandConstraintPinky1Target = new GameObject("LeftHandPinky1ConstraintTarget");
        GameObject leftHandConstraintPinky2Target = new GameObject("LeftHandPinky2ConstraintTarget");
        GameObject leftHandConstraintPinky3Target = new GameObject("LeftHandPinky3ConstraintTarget");

        GameObject leftHandConstraintRing1Target = new GameObject("LeftHandRing1ConstraintTarget");
        GameObject leftHandConstraintRing2Target = new GameObject("LeftHandRing2ConstraintTarget");
        GameObject leftHandConstraintRing3Target = new GameObject("LeftHandRing3ConstraintTarget");

        GameObject leftHandConstraintThumb1Target = new GameObject("LeftHandThumb1ConstraintTarget");
        GameObject leftHandConstraintThumb2Target = new GameObject("LeftHandThumb2ConstraintTarget");
        GameObject leftHandConstraintThumb3Target = new GameObject("LeftHandThumb3ConstraintTarget");


        GameObject leftHandIndexIKTarget = new GameObject("LeftHandIndexIKTarget");
        GameObject leftHandMiddleIKTarget = new GameObject("LeftHandMiddleIKTarget");
        GameObject leftHandPinkyIKTarget = new GameObject("LeftHandPinkyIKTarget");
        GameObject leftHandRingIKTarget = new GameObject("LeftHandRingIKTarget");
        GameObject leftHandThumbIKTarget = new GameObject("LeftHandThumbIKTarget");



        GameObject rightHandConstraintIndex1Target = new GameObject("RightHandIndex1ConstraintTarget");
        GameObject rightHandConstraintIndex2Target = new GameObject("RightHandIndex2ConstraintTarget");
        GameObject rightHandConstraintIndex3Target = new GameObject("RightHandIndex3ConstraintTarget");

        GameObject rightHandConstraintMiddle1Target = new GameObject("RightHandMiddle1ConstraintTarget");
        GameObject rightHandConstraintMiddle2Target = new GameObject("RightHandMiddle2ConstraintTarget");
        GameObject rightHandConstraintMiddle3Target = new GameObject("RightHandMiddle3ConstraintTarget");

        GameObject rightHandConstraintPinky1Target = new GameObject("RightHandPinky1ConstraintTarget");
        GameObject rightHandConstraintPinky2Target = new GameObject("RightHandPinky2ConstraintTarget");
        GameObject rightHandConstraintPinky3Target = new GameObject("RightHandPinky3ConstraintTarget");

        GameObject rightHandConstraintRing1Target = new GameObject("RightHandRing1ConstraintTarget");
        GameObject rightHandConstraintRing2Target = new GameObject("RightHandRing2ConstraintTarget");
        GameObject rightHandConstraintRing3Target = new GameObject("RightHandRing3ConstraintTarget");

        GameObject rightHandConstraintThumb1Target = new GameObject("RightHandThumb1ConstraintTarget");
        GameObject rightHandConstraintThumb2Target = new GameObject("RightHandThumb2ConstraintTarget");
        GameObject rightHandConstraintThumb3Target = new GameObject("RightHandThumb3ConstraintTarget");


        GameObject rightHandIndexIKTarget = new GameObject("RightHandIndexIKTarget");
        GameObject rightHandMiddleIKTarget = new GameObject("RightHandMiddleIKTarget");
        GameObject rightHandPinkyIKTarget = new GameObject("RightHandPinkyIKTarget");
        GameObject rightHandRingIKTarget = new GameObject("RightHandRingIKTarget");
        GameObject rightHandThumbIKTarget = new GameObject("RightHandThumbIKTarget");


        GameObject iKBasedFingersTargetL = new GameObject("IKBasedFingersTarget");
        GameObject directRotationMatchFingersTargetL = new GameObject("DirectRotationMatchFingers");

        GameObject iKBasedFingersTargetR = new GameObject("IKBasedFingersTarget");
        GameObject directRotationMatchFingersTargetR = new GameObject("directRotationMatchFingers");


        GameObject leftHandIKTarget = new GameObject("LeftHandIKTarget");
        GameObject rightHandIKTarget = new GameObject("RightHandIKTarget");

        // Spine Follower
        GameObject spineFollower = new GameObject("SpineFollower");

        GameObject centerSpinePos = new GameObject("CenterSpinePos");
        GameObject clipProjectorPos = new GameObject("ClipProjectorPos");

        GameObject gunHolderAnimated = new GameObject("GunHolderAnimated");
        GameObject cameraHolder = new GameObject("CameraHolder");
        GameObject gunHolder = new GameObject("GunHolder");

        GameObject clipProjector = new GameObject("ClipProjector");
        GameObject interactPointer = new GameObject("InteractPointer");
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);

        // Slots/Sockets
        GameObject slotsSockets = new GameObject("Slots/Sockets");

        GameObject rightHandSlot = new GameObject("RightHandSlot");
        GameObject playerBackSlots = new GameObject("PlayerBackSlot");
        GameObject rightUpLegSlot = new GameObject("RightUpLegSlot");

        GameObject rifleSlotOne = new GameObject("RifleSlotOne");
        GameObject rifleSlotTwo = new GameObject("RifleSlotTwo");

        GameObject pistolSlot = new GameObject("PistolSlot");

        GameObject knifeSlotPosition = new GameObject("KnifeSlotPosition");
        GameObject knifeSlot = new GameObject("KnifeSlot");

        #endregion

        #region Parenting
        spineRotRig.transform.SetParent(body.transform);
        spine1RotRig.transform.SetParent(body.transform);
        spine2RotRig.transform.SetParent(body.transform);
        headAimRig.transform.SetParent(body.transform);

        iKBasedFingersWeight.transform.SetParent(hands.transform);
        rotationConstrainedFingersWeight.transform.SetParent(hands.transform);
        handsIKWeight.transform.SetParent(hands.transform);
        leftHandIK.transform.SetParent(hands.transform);
        rightHandIK.transform.SetParent(hands.transform);

        directRotationMatchFingersL.transform.SetParent(leftHandIK.transform);
        iKBasedFingersL.transform.SetParent(leftHandIK.transform);

        directRotationMatchFingersR.transform.SetParent(rightHandIK.transform);
        iKBasedFingersR.transform.SetParent(rightHandIK.transform);

        body.transform.SetParent(rig.transform);
        hands.transform.SetParent(rig.transform);
        
        leftHandIndexIK.transform.SetParent(iKBasedFingersL.transform);
        leftHandMiddleIK.transform.SetParent(iKBasedFingersL.transform);
        leftHandPinkyIK.transform.SetParent(iKBasedFingersL.transform);
        leftHandRingIK.transform.SetParent(iKBasedFingersL.transform);
        leftHandThumbIK.transform.SetParent(iKBasedFingersL.transform);

        leftHandIndex1Constraint.transform.SetParent(directRotationMatchFingersL.transform);
        leftHandIndex2Constraint.transform.SetParent(leftHandIndex1Constraint.transform);
        leftHandIndex3Constraint.transform.SetParent(leftHandIndex2Constraint.transform);

        leftHandMiddle1Constraint.transform.SetParent(directRotationMatchFingersL.transform);
        leftHandMiddle2Constraint.transform.SetParent(leftHandMiddle1Constraint.transform);
        leftHandMiddle3Constraint.transform.SetParent(leftHandMiddle2Constraint.transform);

        leftHandPinky1Constraint.transform.SetParent(directRotationMatchFingersL.transform);
        leftHandPinky2Constraint.transform.SetParent(leftHandPinky1Constraint.transform);
        leftHandPinky3Constraint.transform.SetParent(leftHandPinky2Constraint.transform);

        leftHandRing1Constraint.transform.SetParent(directRotationMatchFingersL.transform);
        leftHandRing2Constraint.transform.SetParent(leftHandRing1Constraint.transform);
        leftHandRing3Constraint.transform.SetParent(leftHandRing2Constraint.transform);

        leftHandThumb1Constraint.transform.SetParent(directRotationMatchFingersL.transform);
        leftHandThumb2Constraint.transform.SetParent(leftHandThumb1Constraint.transform);
        leftHandThumb3Constraint.transform.SetParent(leftHandThumb2Constraint.transform);


        rightHandIndexIK.transform.SetParent(iKBasedFingersR.transform);
        rightHandMiddleIK.transform.SetParent(iKBasedFingersR.transform);
        rightHandPinkyIK.transform.SetParent(iKBasedFingersR.transform);
        rightHandRingIK.transform.SetParent(iKBasedFingersR.transform);
        rightHandThumbIK.transform.SetParent(iKBasedFingersR.transform);

        rightHandIndex1Constraint.transform.SetParent(directRotationMatchFingersR.transform);
        rightHandIndex2Constraint.transform.SetParent(rightHandIndex1Constraint.transform);
        rightHandIndex3Constraint.transform.SetParent(rightHandIndex2Constraint.transform);

        rightHandMiddle1Constraint.transform.SetParent(directRotationMatchFingersR.transform);
        rightHandMiddle2Constraint.transform.SetParent(rightHandMiddle1Constraint.transform);
        rightHandMiddle3Constraint.transform.SetParent(rightHandMiddle2Constraint.transform);

        rightHandPinky1Constraint.transform.SetParent(directRotationMatchFingersR.transform);
        rightHandPinky2Constraint.transform.SetParent(rightHandPinky1Constraint.transform);
        rightHandPinky3Constraint.transform.SetParent(rightHandPinky2Constraint.transform);

        rightHandRing1Constraint.transform.SetParent(directRotationMatchFingersR.transform);
        rightHandRing2Constraint.transform.SetParent(rightHandRing1Constraint.transform);
        rightHandRing3Constraint.transform.SetParent(rightHandRing2Constraint.transform);

        rightHandThumb1Constraint.transform.SetParent(directRotationMatchFingersR.transform);
        rightHandThumb2Constraint.transform.SetParent(rightHandThumb1Constraint.transform);
        rightHandThumb3Constraint.transform.SetParent(rightHandThumb2Constraint.transform);



        // Right hand and Left hand IK parenting

        // Left hand
        iKBasedFingersTargetL.transform.SetParent(leftHandIKTarget.transform);
        directRotationMatchFingersTargetL.transform.SetParent(leftHandIKTarget.transform);

        leftHandIndexIKTarget.transform.SetParent(iKBasedFingersTargetL.transform);
        leftHandMiddleIKTarget.transform.SetParent(iKBasedFingersTargetL.transform);
        leftHandPinkyIKTarget.transform.SetParent(iKBasedFingersTargetL.transform);
        leftHandRingIKTarget.transform.SetParent(iKBasedFingersTargetL.transform);
        leftHandThumbIKTarget.transform.SetParent(iKBasedFingersTargetL.transform);

        leftHandConstraintIndex1Target.transform.SetParent(directRotationMatchFingersTargetL.transform);
        leftHandConstraintIndex2Target.transform.SetParent(leftHandConstraintIndex1Target.transform);
        leftHandConstraintIndex3Target.transform.SetParent(leftHandConstraintIndex2Target.transform);


        leftHandConstraintMiddle1Target.transform.SetParent(directRotationMatchFingersTargetL.transform);
        leftHandConstraintMiddle2Target.transform.SetParent(leftHandConstraintMiddle1Target.transform);
        leftHandConstraintMiddle3Target.transform.SetParent(leftHandConstraintMiddle2Target.transform);


        leftHandConstraintPinky1Target.transform.SetParent(directRotationMatchFingersTargetL.transform);
        leftHandConstraintPinky2Target.transform.SetParent(leftHandConstraintPinky1Target.transform);
        leftHandConstraintPinky3Target.transform.SetParent(leftHandConstraintPinky2Target.transform);

        leftHandConstraintRing1Target.transform.SetParent(directRotationMatchFingersTargetL.transform);
        leftHandConstraintRing2Target.transform.SetParent(leftHandConstraintRing1Target.transform);
        leftHandConstraintRing3Target.transform.SetParent(leftHandConstraintRing2Target.transform);

        leftHandConstraintThumb1Target.transform.SetParent(directRotationMatchFingersTargetL.transform);
        leftHandConstraintThumb2Target.transform.SetParent(leftHandConstraintThumb1Target.transform);
        leftHandConstraintThumb3Target.transform.SetParent(leftHandConstraintThumb2Target.transform);
     

        
        // Right
        iKBasedFingersTargetR.transform.SetParent(rightHandIKTarget.transform);
        directRotationMatchFingersTargetR.transform.SetParent(rightHandIKTarget.transform);

        rightHandIndexIKTarget.transform.SetParent(iKBasedFingersTargetR.transform);
        rightHandMiddleIKTarget.transform.SetParent(iKBasedFingersTargetR.transform);
        rightHandPinkyIKTarget.transform.SetParent(iKBasedFingersTargetR.transform);
        rightHandRingIKTarget.transform.SetParent(iKBasedFingersTargetR.transform);
        rightHandThumbIKTarget.transform.SetParent(iKBasedFingersTargetR.transform);

        rightHandConstraintIndex1Target.transform.SetParent(directRotationMatchFingersTargetR.transform);
        rightHandConstraintIndex2Target.transform.SetParent(rightHandConstraintIndex1Target.transform);
        rightHandConstraintIndex3Target.transform.SetParent(rightHandConstraintIndex2Target.transform);

        rightHandConstraintMiddle1Target.transform.SetParent(directRotationMatchFingersTargetR.transform);
        rightHandConstraintMiddle2Target.transform.SetParent(rightHandConstraintMiddle1Target.transform);
        rightHandConstraintMiddle3Target.transform.SetParent(rightHandConstraintMiddle2Target.transform);

        rightHandConstraintPinky1Target.transform.SetParent(directRotationMatchFingersTargetR.transform);
        rightHandConstraintPinky2Target.transform.SetParent(rightHandConstraintPinky1Target.transform);
        rightHandConstraintPinky3Target.transform.SetParent(rightHandConstraintPinky2Target.transform);

        rightHandConstraintRing1Target.transform.SetParent(directRotationMatchFingersTargetR.transform);
        rightHandConstraintRing2Target.transform.SetParent(rightHandConstraintRing1Target.transform);
        rightHandConstraintRing3Target.transform.SetParent(rightHandConstraintRing2Target.transform);

        rightHandConstraintThumb1Target.transform.SetParent(directRotationMatchFingersTargetR.transform);
        rightHandConstraintThumb2Target.transform.SetParent(rightHandConstraintThumb1Target.transform);
        rightHandConstraintThumb3Target.transform.SetParent(rightHandConstraintThumb2Target.transform);

        // IK targets
        leftHandIKTarget.transform.SetParent(spineFollower.transform);
        rightHandIKTarget.transform.SetParent(spineFollower.transform);

        // Spine Follower 
        spineFollower.transform.SetParent(playerParentGameObject.transform);

        centerSpinePos.transform.SetParent(spineFollower.transform);
        clipProjectorPos.transform.SetParent(spineFollower.transform);

        gunHolderAnimated.transform.SetParent(centerSpinePos.transform);
        cameraHolder.transform.SetParent(centerSpinePos.transform);

        gunHolder.transform.SetParent(gunHolderAnimated.transform);

        clipProjector.transform.SetParent(clipProjectorPos.transform);
        interactPointer.transform.SetParent(clipProjectorPos.transform);

        visual.transform.SetParent(clipProjector.transform);
        visual.SetActive(false);

        // Slots/Sockets
        slotsSockets.transform.SetParent(playerParentGameObject.transform);

        rightHandSlot.transform.SetParent(slotsSockets.transform);
        playerBackSlots.transform.SetParent(slotsSockets.transform);
        rightUpLegSlot.transform.SetParent(slotsSockets.transform);

        rifleSlotOne.transform.SetParent(playerBackSlots.transform);
        rifleSlotTwo.transform.SetParent(playerBackSlots.transform);

        pistolSlot.transform.SetParent(rightUpLegSlot.transform);
        knifeSlotPosition.transform.SetParent(rightUpLegSlot.transform);

        knifeSlot.transform.SetParent(knifeSlotPosition.transform);

        #endregion

        #region Add Components
        MultiRotationConstraint spineConstraint = spineRotRig.AddComponent<MultiRotationConstraint>();
        MultiRotationConstraint spine1Constraint = spine1RotRig.AddComponent<MultiRotationConstraint>();
        MultiRotationConstraint spine2Constraint = spine2RotRig.AddComponent<MultiRotationConstraint>();
        MultiRotationConstraint headConstraint = headAimRig.AddComponent<MultiRotationConstraint>();

        ConstraintsWeightModifier iKBasedFingersWeightModifier = iKBasedFingersWeight.AddComponent<ConstraintsWeightModifier>();
        ConstraintsWeightModifier rotationConstrainedFingersWeightModifier = rotationConstrainedFingersWeight.AddComponent<ConstraintsWeightModifier>();
        ConstraintsWeightModifier handsIKWeightModifier = handsIKWeight.AddComponent<ConstraintsWeightModifier>();


        MatchRotationToTarget leftHandIndex1Target = leftHandIndex1Constraint.AddComponent<MatchRotationToTarget>();
        MatchRotationToTarget leftHandIndex2Target = leftHandIndex2Constraint.AddComponent<MatchRotationToTarget>();
        MatchRotationToTarget leftHandIndex3Target = leftHandIndex3Constraint.AddComponent<MatchRotationToTarget>();

        MatchRotationToTarget leftHandMiddle1Target = leftHandMiddle1Constraint.AddComponent<MatchRotationToTarget>();
        MatchRotationToTarget leftHandMiddle2Target = leftHandMiddle2Constraint.AddComponent<MatchRotationToTarget>();
        MatchRotationToTarget leftHandMiddle3Target = leftHandMiddle3Constraint.AddComponent<MatchRotationToTarget>();

        MatchRotationToTarget leftHandPinky1Target = leftHandPinky1Constraint.AddComponent<MatchRotationToTarget>();
        MatchRotationToTarget leftHandPinky2Target = leftHandPinky2Constraint.AddComponent<MatchRotationToTarget>();
        MatchRotationToTarget leftHandPinky3Target = leftHandPinky3Constraint.AddComponent<MatchRotationToTarget>();

        MatchRotationToTarget leftHandRing1Target = leftHandRing1Constraint.AddComponent<MatchRotationToTarget>();
        MatchRotationToTarget leftHandRing2Target = leftHandRing2Constraint.AddComponent<MatchRotationToTarget>();
        MatchRotationToTarget leftHandRing3Target = leftHandRing3Constraint.AddComponent<MatchRotationToTarget>();

        MatchRotationToTarget leftHandThumb1Target = leftHandThumb1Constraint.AddComponent<MatchRotationToTarget>();
        MatchRotationToTarget leftHandThumb2Target = leftHandThumb2Constraint.AddComponent<MatchRotationToTarget>();
        MatchRotationToTarget leftHandThumb3Target = leftHandThumb3Constraint.AddComponent<MatchRotationToTarget>();

        

        MatchRotationToTarget rightHandIndex1Target = rightHandIndex1Constraint.AddComponent<MatchRotationToTarget>();
        MatchRotationToTarget rightHandIndex2Target = rightHandIndex2Constraint.AddComponent<MatchRotationToTarget>();
        MatchRotationToTarget rightHandIndex3Target = rightHandIndex3Constraint.AddComponent<MatchRotationToTarget>();

        MatchRotationToTarget rightHandMiddle1Target = rightHandMiddle1Constraint.AddComponent<MatchRotationToTarget>();
        MatchRotationToTarget rightHandMiddle2Target = rightHandMiddle2Constraint.AddComponent<MatchRotationToTarget>();
        MatchRotationToTarget rightHandMiddle3Target = rightHandMiddle3Constraint.AddComponent<MatchRotationToTarget>();

        MatchRotationToTarget rightHandPinky1Target = rightHandPinky1Constraint.AddComponent<MatchRotationToTarget>();
        MatchRotationToTarget rightHandPinky2Target = rightHandPinky2Constraint.AddComponent<MatchRotationToTarget>();
        MatchRotationToTarget rightHandPinky3Target = rightHandPinky3Constraint.AddComponent<MatchRotationToTarget>();

        MatchRotationToTarget rightHandRing1Target = rightHandRing1Constraint.AddComponent<MatchRotationToTarget>();
        MatchRotationToTarget rightHandRing2Target = rightHandRing2Constraint.AddComponent<MatchRotationToTarget>();
        MatchRotationToTarget rightHandRing3Target = rightHandRing3Constraint.AddComponent<MatchRotationToTarget>();

        MatchRotationToTarget rightHandThumb1Target = rightHandThumb1Constraint.AddComponent<MatchRotationToTarget>();
        MatchRotationToTarget rightHandThumb2Target = rightHandThumb2Constraint.AddComponent<MatchRotationToTarget>();
        MatchRotationToTarget rightHandThumb3Target = rightHandThumb3Constraint.AddComponent<MatchRotationToTarget>();

        TwoBoneIKConstraint leftHandBoneIK = leftHandIK.AddComponent<TwoBoneIKConstraint>();
        TwoBoneIKConstraint rightHandBoneIK = rightHandIK.AddComponent<TwoBoneIKConstraint>();


        // Left hand IK
        TwoBoneIKConstraint leftHandIndexIKConstraint = leftHandIndexIK.AddComponent<TwoBoneIKConstraint>();
        TwoBoneIKConstraint leftHandMiddleIKConstraint = leftHandMiddleIK.AddComponent<TwoBoneIKConstraint>();
        TwoBoneIKConstraint leftHandPinkyIKConstraint = leftHandPinkyIK.AddComponent<TwoBoneIKConstraint>();
        TwoBoneIKConstraint leftHandRingIKConstraint = leftHandRingIK.AddComponent<TwoBoneIKConstraint>();
        TwoBoneIKConstraint leftHandThumbIKConstraint = leftHandThumbIK.AddComponent<TwoBoneIKConstraint>();

        // Right hand IK
        TwoBoneIKConstraint rightHandIndexIKConstraint = rightHandIndexIK.AddComponent<TwoBoneIKConstraint>();
        TwoBoneIKConstraint rightHandMiddleIKConstraint = rightHandMiddleIK.AddComponent<TwoBoneIKConstraint>();
        TwoBoneIKConstraint rightHandPinkyIKConstraint = rightHandPinkyIK.AddComponent<TwoBoneIKConstraint>();
        TwoBoneIKConstraint rightHandRingIKConstraint = rightHandRingIK.AddComponent<TwoBoneIKConstraint>();
        TwoBoneIKConstraint rightHandThumbIKConstraint = rightHandThumbIK.AddComponent<TwoBoneIKConstraint>();

        // Spine Follower
        FollowTransformPosLateUpdate spineFollowerPosLateUpdate = spineFollower.AddComponent<FollowTransformPosLateUpdate>();

        GunHolderAnimated gunHolderAnim = gunHolderAnimated.AddComponent<GunHolderAnimated>();
        Slot gunHolderSlot = gunHolder.AddComponent<Slot>();

        FollowLocalPositionWithAxis interactPointerLocalPosition = interactPointer.AddComponent<FollowLocalPositionWithAxis>();

        // Slots/Sockets
        FollowTransformPosAndRot rightHandSlotFollow = rightHandSlot.AddComponent<FollowTransformPosAndRot>();
        FollowTransformPosAndRot playerBackSlotsFollow = playerBackSlots.AddComponent<FollowTransformPosAndRot>();
        FollowTransformPosAndRot rightUpLegSlotFollow = rightUpLegSlot.AddComponent<FollowTransformPosAndRot>();

        Slot rifleSlotOneSlot = rifleSlotOne.AddComponent<Slot>();
        Slot rifleSlotTwoSlot = rifleSlotTwo.AddComponent<Slot>();
        
        Slot pistolSlotSlot = pistolSlot.AddComponent<Slot>();
        
        Slot knifeSlotSlot = knifeSlot.AddComponent<Slot>();

        FollowTransformPosAndRot leftHandIndexIKTargetFollow = leftHandIndexIKTarget.AddComponent<FollowTransformPosAndRot>();
        FollowTransformPosAndRot leftHandMiddleIKTargetFollow = leftHandMiddleIKTarget.AddComponent<FollowTransformPosAndRot>();
        FollowTransformPosAndRot leftHandPinkyIKTargetFollow = leftHandPinkyIKTarget.AddComponent<FollowTransformPosAndRot>();
        FollowTransformPosAndRot leftHandRingIKTargetFollow = leftHandRingIKTarget.AddComponent<FollowTransformPosAndRot>();
        FollowTransformPosAndRot leftHandThumbIKTargetFollow = leftHandThumbIKTarget.AddComponent<FollowTransformPosAndRot>();


        FollowTransformPosAndRot rightHandIndexIKTargetFollow = rightHandIndexIKTarget.AddComponent<FollowTransformPosAndRot>();
        FollowTransformPosAndRot rightHandMiddleIKTargetFollow = rightHandMiddleIKTarget.AddComponent<FollowTransformPosAndRot>();
        FollowTransformPosAndRot rightHandPinkyIKTargetFollow = rightHandPinkyIKTarget.AddComponent<FollowTransformPosAndRot>();
        FollowTransformPosAndRot rightHandRingIKTargetFollow = rightHandRingIKTarget.AddComponent<FollowTransformPosAndRot>();
        FollowTransformPosAndRot rightHandThumbIKTargetFollow = rightHandThumbIKTarget.AddComponent<FollowTransformPosAndRot>();

        // Left side target constraint

        FollowTransformRot leftHandIndex1ConstraintTargetFollow = leftHandConstraintIndex1Target.AddComponent<FollowTransformRot>();
        FollowTransformRot leftHandIndex2ConstraintTargetFollow = leftHandConstraintIndex2Target.AddComponent<FollowTransformRot>();
        FollowTransformRot leftHandIndex3ConstraintTargetFollow = leftHandConstraintIndex3Target.AddComponent<FollowTransformRot>();

        FollowTransformRot leftHandMiddle1ConstraintTargetFollow = leftHandConstraintMiddle1Target.AddComponent<FollowTransformRot>();
        FollowTransformRot leftHandMiddle2ConstraintTargetFollow = leftHandConstraintMiddle2Target.AddComponent<FollowTransformRot>();
        FollowTransformRot leftHandMiddle3ConstraintTargetFollow = leftHandConstraintMiddle3Target.AddComponent<FollowTransformRot>();

        FollowTransformRot leftHandPinky1ConstraintTargetFollow = leftHandConstraintPinky1Target.AddComponent<FollowTransformRot>();
        FollowTransformRot leftHandPinky2ConstraintTargetFollow = leftHandConstraintPinky2Target.AddComponent<FollowTransformRot>();
        FollowTransformRot leftHandPinky3ConstraintTargetFollow = leftHandConstraintPinky3Target.AddComponent<FollowTransformRot>();

        FollowTransformRot leftHandRing1ConstraintTargetFollow = leftHandConstraintRing1Target.AddComponent<FollowTransformRot>();
        FollowTransformRot leftHandRing2ConstraintTargetFollow = leftHandConstraintRing2Target.AddComponent<FollowTransformRot>();
        FollowTransformRot leftHandRing3ConstraintTargetFollow = leftHandConstraintRing3Target.AddComponent<FollowTransformRot>();

        FollowTransformRot leftHandThumb1ConstraintTargetFollow = leftHandConstraintThumb1Target.AddComponent<FollowTransformRot>();
        FollowTransformRot leftHandThumb2ConstraintTargetFollow = leftHandConstraintThumb2Target.AddComponent<FollowTransformRot>();
        FollowTransformRot leftHandThumb3ConstraintTargetFollow = leftHandConstraintThumb3Target.AddComponent<FollowTransformRot>();

        // Right side target constraint

        FollowTransformRot rightHandIndex1ConstraintTargetFollow = rightHandConstraintIndex1Target.AddComponent<FollowTransformRot>();
        FollowTransformRot rightHandIndex2ConstraintTargetFollow = rightHandConstraintIndex2Target.AddComponent<FollowTransformRot>();
        FollowTransformRot rightHandIndex3ConstraintTargetFollow = rightHandConstraintIndex3Target.AddComponent<FollowTransformRot>();

        FollowTransformRot rightHandMiddle1ConstraintTargetFollow = rightHandConstraintMiddle1Target.AddComponent<FollowTransformRot>();
        FollowTransformRot rightHandMiddle2ConstraintTargetFollow = rightHandConstraintMiddle2Target.AddComponent<FollowTransformRot>();
        FollowTransformRot rightHandMiddle3ConstraintTargetFollow = rightHandConstraintMiddle3Target.AddComponent<FollowTransformRot>();

        FollowTransformRot rightHandPinky1ConstraintTargetFollow = rightHandConstraintPinky1Target.AddComponent<FollowTransformRot>();
        FollowTransformRot rightHandPinky2ConstraintTargetFollow = rightHandConstraintPinky2Target.AddComponent<FollowTransformRot>();
        FollowTransformRot rightHandPinky3ConstraintTargetFollow = rightHandConstraintPinky3Target.AddComponent<FollowTransformRot>();

        FollowTransformRot rightHandRing1ConstraintTargetFollow = rightHandConstraintRing1Target.AddComponent<FollowTransformRot>();
        FollowTransformRot rightHandRing2ConstraintTargetFollow = rightHandConstraintRing2Target.AddComponent<FollowTransformRot>();
        FollowTransformRot rightHandRing3ConstraintTargetFollow = rightHandConstraintRing3Target.AddComponent<FollowTransformRot>();

        FollowTransformRot rightHandThumb1ConstraintTargetFollow = rightHandConstraintThumb1Target.AddComponent<FollowTransformRot>();
        FollowTransformRot rightHandThumb2ConstraintTargetFollow = rightHandConstraintThumb2Target.AddComponent<FollowTransformRot>();
        FollowTransformRot rightHandThumb3ConstraintTargetFollow = rightHandConstraintThumb3Target.AddComponent<FollowTransformRot>();



        #endregion

        #region Body Constraints
        spineConstraint.weight = 0.3f;
        spine1Constraint.weight = 0.3f;
        spine2Constraint.weight = 1f;
        headConstraint.weight = 1f;

        headConstraint.data.constrainedZAxis = false;

        spineConstraint.data.constrainedObject = playerModelAnimator.GetBoneTransform(HumanBodyBones.Spine);
        spine1Constraint.data.constrainedObject = playerModelAnimator.GetBoneTransform(HumanBodyBones.Chest);
        spine2Constraint.data.constrainedObject = playerModelAnimator.GetBoneTransform(HumanBodyBones.UpperChest);
        headConstraint.data.constrainedObject = playerModelAnimator.GetBoneTransform(HumanBodyBones.Head);

        spineConstraint.data.maintainOffset = true;
        spine1Constraint.data.maintainOffset = true;
        spine2Constraint.data.maintainOffset = true;
        headConstraint.data.maintainOffset = true;

        spine1Constraint.data.offset = new Vector3(0, 50, 0);
        spine2Constraint.data.offset = new Vector3(0, 50, 0);
        #endregion

        #region Constraints

        // Rig
        leftHandBoneIK.data.root = playerModelAnimator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        leftHandBoneIK.data.mid = playerModelAnimator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        leftHandBoneIK.data.tip = playerModelAnimator.GetBoneTransform(HumanBodyBones.LeftHand);
        leftHandBoneIK.data.target = leftHandIKTarget.transform;


        rightHandBoneIK.data.root = playerModelAnimator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        rightHandBoneIK.data.mid = playerModelAnimator.GetBoneTransform(HumanBodyBones.RightLowerArm);
        rightHandBoneIK.data.tip = playerModelAnimator.GetBoneTransform(HumanBodyBones.RightHand);
        rightHandBoneIK.data.target = leftHandIKTarget.transform;


        Transform tipLeftIndex = playerModelAnimator.GetBoneTransform(HumanBodyBones.LeftIndexDistal);
        leftHandIndexIKConstraint.data.tip = tipLeftIndex;
        leftHandIndexIKConstraint.data.mid = tipLeftIndex.parent;
        leftHandIndexIKConstraint.data.root = tipLeftIndex.parent.parent;
        leftHandIndexIKConstraint.data.target = leftHandIndexIKTarget.transform;

        Transform tipLeftMiddle = playerModelAnimator.GetBoneTransform(HumanBodyBones.LeftMiddleDistal);
        leftHandMiddleIKConstraint.data.tip = tipLeftMiddle;
        leftHandMiddleIKConstraint.data.mid = tipLeftMiddle.parent;
        leftHandMiddleIKConstraint.data.root = tipLeftMiddle.parent.parent;
        leftHandMiddleIKConstraint.data.target = leftHandMiddleIKTarget.transform;

        Transform tipLeftPinky = playerModelAnimator.GetBoneTransform(HumanBodyBones.LeftLittleDistal);
        leftHandPinkyIKConstraint.data.tip = tipLeftPinky;
        leftHandPinkyIKConstraint.data.mid = tipLeftPinky.parent;
        leftHandPinkyIKConstraint.data.root = tipLeftPinky.parent.parent;
        leftHandPinkyIKConstraint.data.target = leftHandPinkyIKTarget.transform;

        Transform tipLeftRing = playerModelAnimator.GetBoneTransform(HumanBodyBones.LeftRingDistal);
        leftHandRingIKConstraint.data.tip = tipLeftRing;
        leftHandRingIKConstraint.data.mid = tipLeftRing.parent;
        leftHandRingIKConstraint.data.root = tipLeftRing.parent.parent;
        leftHandRingIKConstraint.data.target = leftHandRingIKTarget.transform;

        Transform tipLeftThumb = playerModelAnimator.GetBoneTransform(HumanBodyBones.LeftThumbDistal);
        leftHandThumbIKConstraint.data.tip = tipLeftThumb;
        leftHandThumbIKConstraint.data.mid = tipLeftThumb.parent;
        leftHandThumbIKConstraint.data.root = tipLeftThumb.parent.parent;
        leftHandThumbIKConstraint.data.target = leftHandThumbIKTarget.transform;


        // 

        Transform tipRightIndex = playerModelAnimator.GetBoneTransform(HumanBodyBones.RightIndexDistal);
        rightHandIndexIKConstraint.data.tip = tipRightIndex;
        rightHandIndexIKConstraint.data.mid = tipRightIndex.parent;
        rightHandIndexIKConstraint.data.root = tipRightIndex.parent.parent;
        rightHandIndexIKConstraint.data.target = rightHandIndexIKTarget.transform;

        Transform tipRightMiddle = playerModelAnimator.GetBoneTransform(HumanBodyBones.RightMiddleDistal);
        rightHandMiddleIKConstraint.data.tip = tipRightMiddle;
        rightHandMiddleIKConstraint.data.mid = tipRightMiddle.parent;
        rightHandMiddleIKConstraint.data.root = tipRightMiddle.parent.parent;
        rightHandMiddleIKConstraint.data.target = rightHandMiddleIKTarget.transform;

        Transform tipRightPinky = playerModelAnimator.GetBoneTransform(HumanBodyBones.RightLittleDistal);
        rightHandPinkyIKConstraint.data.tip = tipRightPinky;
        rightHandPinkyIKConstraint.data.mid = tipRightPinky.parent;
        rightHandPinkyIKConstraint.data.root = tipRightPinky.parent.parent;
        rightHandPinkyIKConstraint.data.target = rightHandPinkyIKTarget.transform;

        Transform tipRightRing = playerModelAnimator.GetBoneTransform(HumanBodyBones.RightRingDistal);
        rightHandRingIKConstraint.data.tip = tipRightRing;
        rightHandRingIKConstraint.data.mid = tipRightRing.parent;
        rightHandRingIKConstraint.data.root = tipRightRing.parent.parent;
        rightHandRingIKConstraint.data.target = rightHandRingIKTarget.transform;

        Transform tipRightThumb = playerModelAnimator.GetBoneTransform(HumanBodyBones.RightThumbDistal);
        rightHandThumbIKConstraint.data.tip = tipRightThumb;
        rightHandThumbIKConstraint.data.mid = tipRightThumb.parent;
        rightHandThumbIKConstraint.data.root = tipRightThumb.parent.parent;
        rightHandThumbIKConstraint.data.target = rightHandThumbIKTarget.transform;


        // SpineFollower
        spineFollowerPosLateUpdate.Target = playerModelAnimator.GetBoneTransform(HumanBodyBones.UpperChest);

        gunHolderSlot.SmoothTime = 0.2f;

        // Slots/Sockets
        rightHandSlotFollow.Target = playerModelAnimator.GetBoneTransform(HumanBodyBones.RightHand);
        playerBackSlotsFollow.Target = playerModelAnimator.GetBoneTransform(HumanBodyBones.UpperChest);

        rightUpLegSlotFollow.Target = playerModelAnimator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        rightUpLegSlotFollow.MaintainOffset = true;


        rifleSlotOneSlot.SmoothTime = 0.2f;
        rifleSlotTwoSlot.SmoothTime = 0.2f;
        pistolSlotSlot.SmoothTime = 0.2f;
        knifeSlotSlot.SmoothTime = 0.2f;



        #endregion

        #region Constraints Weight Modifier
        SerializedProperty constraintsWeightModifierProperty = GetPropertyFromObj(iKBasedFingersWeightModifier, _iKConstraintsPropertyName);

        CreateAndAssignElement(constraintsWeightModifierProperty, leftHandIndexIK);
        CreateAndAssignElement(constraintsWeightModifierProperty, leftHandMiddleIK);
        CreateAndAssignElement(constraintsWeightModifierProperty, leftHandPinkyIK);
        CreateAndAssignElement(constraintsWeightModifierProperty, leftHandRingIK);
        CreateAndAssignElement(constraintsWeightModifierProperty, leftHandThumbIK);
        CreateAndAssignElement(constraintsWeightModifierProperty, rightHandIndexIK);
        CreateAndAssignElement(constraintsWeightModifierProperty, rightHandMiddleIK);
        CreateAndAssignElement(constraintsWeightModifierProperty, rightHandPinkyIK);
        CreateAndAssignElement(constraintsWeightModifierProperty, rightHandRingIK);
        CreateAndAssignElement(constraintsWeightModifierProperty, rightHandThumbIK);

        SerializedProperty rotationConstrainedWeight = GetPropertyFromObj(rotationConstrainedFingersWeightModifier, _iKConstraintsPropertyName);
             


        CreateAndAssignElement(rotationConstrainedWeight, leftHandIndex1Constraint);
        CreateAndAssignElement(rotationConstrainedWeight, leftHandIndex2Constraint);
        CreateAndAssignElement(rotationConstrainedWeight, leftHandIndex3Constraint);

        CreateAndAssignElement(rotationConstrainedWeight, leftHandMiddle1Constraint);
        CreateAndAssignElement(rotationConstrainedWeight, leftHandMiddle2Constraint);
        CreateAndAssignElement(rotationConstrainedWeight, leftHandMiddle3Constraint);

        CreateAndAssignElement(rotationConstrainedWeight, leftHandPinky1Constraint);
        CreateAndAssignElement(rotationConstrainedWeight, leftHandPinky2Constraint);
        CreateAndAssignElement(rotationConstrainedWeight, leftHandPinky3Constraint);

        CreateAndAssignElement(rotationConstrainedWeight, leftHandRing1Constraint);
        CreateAndAssignElement(rotationConstrainedWeight, leftHandRing2Constraint);
        CreateAndAssignElement(rotationConstrainedWeight, leftHandRing3Constraint);

        CreateAndAssignElement(rotationConstrainedWeight, leftHandThumb1Constraint);
        CreateAndAssignElement(rotationConstrainedWeight, leftHandThumb2Constraint);
        CreateAndAssignElement(rotationConstrainedWeight, leftHandThumb3Constraint);


        CreateAndAssignElement(rotationConstrainedWeight, rightHandIndex1Constraint);
        CreateAndAssignElement(rotationConstrainedWeight, rightHandIndex2Constraint);
        CreateAndAssignElement(rotationConstrainedWeight, rightHandIndex3Constraint);

        CreateAndAssignElement(rotationConstrainedWeight, rightHandMiddle1Constraint);
        CreateAndAssignElement(rotationConstrainedWeight, rightHandMiddle2Constraint);
        CreateAndAssignElement(rotationConstrainedWeight, rightHandMiddle3Constraint);

        CreateAndAssignElement(rotationConstrainedWeight, rightHandPinky1Constraint);
        CreateAndAssignElement(rotationConstrainedWeight, rightHandPinky2Constraint);
        CreateAndAssignElement(rotationConstrainedWeight, rightHandPinky3Constraint);

        CreateAndAssignElement(rotationConstrainedWeight, rightHandRing1Constraint);
        CreateAndAssignElement(rotationConstrainedWeight, rightHandRing2Constraint);
        CreateAndAssignElement(rotationConstrainedWeight, rightHandRing3Constraint);

        CreateAndAssignElement(rotationConstrainedWeight, rightHandThumb1Constraint);
        CreateAndAssignElement(rotationConstrainedWeight, rightHandThumb2Constraint);
        CreateAndAssignElement(rotationConstrainedWeight, rightHandThumb3Constraint);

        SerializedProperty handsIKModifier = GetPropertyFromObj(handsIKWeightModifier, _iKConstraintsPropertyName);

        CreateAndAssignElement(handsIKModifier, leftHandIK);
        CreateAndAssignElement(handsIKModifier, rightHandIK);


        #endregion

        #region player Refs
        PlayerController controller = playerParentGameObject.GetComponent<PlayerController>();
        AssignPrivatePropertyObj(controller, playerParentGameObject.GetComponent<PlayerUI>(), _playerUIPropertyName);
        AssignPrivatePropertyObj(controller, playerModelAnimator, _playerAnimatorName);
        AssignPrivatePropertyObj(controller, gunHolderAnim, _gunHolderAnimatorName);
        AssignPrivatePropertyObj(controller, centerSpinePos.transform, _centerSpinePosName);
        AssignPrivatePropertyObj(controller, cameraHolder.transform, _cameraHolderName);
        AssignPrivatePropertyObj(controller, playerModelAnimator.GetBoneTransform(HumanBodyBones.Head), _headTransformName);
        AssignPrivatePropertyObj(controller, gunHolder.transform, _gunHolderName);
        AssignPrivatePropertyObj(controller, iKBasedFingersWeight.GetComponent<ConstraintsWeightModifier>(), _iKBasedFingersWeightModifierName);
        AssignPrivatePropertyObj(controller, rotationConstrainedFingersWeight.GetComponent<ConstraintsWeightModifier>(), _rotationConstraitBasedFingersWeightModifierName);

        AssignPrivatePropertyObj(rig.AddComponent<RigWeightSetter>(), rig.GetComponent<Rig>(), rigWeightSetterRigPropertyName);
        #endregion
    }

    void CreateAndAssignElement(SerializedProperty property, UnityEngine.Object objValue)
    {
        property.arraySize++;
        SerializedProperty newElementProperty = property.GetArrayElementAtIndex(property.arraySize - 1);

        newElementProperty.objectReferenceValue = objValue;

        property.serializedObject.ApplyModifiedProperties();
    }

    SerializedProperty GetPropertyFromObj(UnityEngine.Object obj, string propertyName)
    {
        return new SerializedObject(obj).FindProperty(propertyName);
    }

    void AssignPrivatePropertyObj(Component component, UnityEngine.Object objReference, string propertyName)
    {
        SerializedObject serializedObject = new SerializedObject(component);
        SerializedProperty rigProperty = serializedObject.FindProperty(propertyName);
        rigProperty.objectReferenceValue = objReference;
        serializedObject.ApplyModifiedProperties();
    }

    
}

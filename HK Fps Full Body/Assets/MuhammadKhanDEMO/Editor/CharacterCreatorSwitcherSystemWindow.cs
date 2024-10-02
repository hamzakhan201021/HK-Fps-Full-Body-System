using UnityEngine;
using UnityEditor;
using UnityEngine.Animations.Rigging;
using System;
using UnityEngine.UI;

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

        SerializedObject rigWeightSetterSObj = new SerializedObject(rig.AddComponent<RigWeightSetter>());
        SerializedProperty rigProperty = rigWeightSetterSObj.FindProperty(rigWeightSetterRigPropertyName);
        rigProperty.objectReferenceValue = rig.GetComponent<Rig>();
        rigWeightSetterSObj.ApplyModifiedProperties();

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


    }
}

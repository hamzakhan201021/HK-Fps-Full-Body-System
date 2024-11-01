using UnityEngine.Animations.Rigging;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System;

public class CharacterCreatorSwitcherSystemWindow : EditorWindow
{

    private GameObject PlayerSystemPrefab;

    [MenuItem("Tools/HK Fps/Character/Character Wizard")]
    public static void ShowWindow()
    {
        GetWindow<CharacterCreatorSwitcherSystemWindow>();
    }

    private void OnEnable()
    {
        PlayerSystemPrefab = FindAssetByExactName<GameObject>("PlayerSystem");
    }

    private GameObject playerModel = null;
    private GameObject newPlayerModel = null;

    private bool _characterCreator = true;

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

        playerModel = EditorGUILayout.ObjectField(playerModel, typeof(GameObject), true) as GameObject;

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();

        Color defaultColor = GUI.backgroundColor;

        if (_characterCreator) GUI.backgroundColor = Color.blue;

        if (GUILayout.Button("Create Character", GUILayout.Height(40)))
        {
            _characterCreator = true;
        }

        GUI.backgroundColor = defaultColor;

        if (!_characterCreator) GUI.backgroundColor = Color.blue;

        if (GUILayout.Button("Switch Character", GUILayout.Height(40)))
        {
            _characterCreator = false;
        }

        GUI.backgroundColor = defaultColor;

        GUILayout.EndHorizontal();

        if (_characterCreator)
        {
            DrawCreatorGUI();
        }
        else
        {
            DrawSwitcherGUI();
        }
    }

    private void DrawCreatorGUI()
    {
        GUILayout.Space(10);

        GUILayout.Label("Setup your Character!");

        GUILayout.Space(50);

        if (GUILayout.Button("Setup Character", GUILayout.Height(75)))
        {
            SetupCharacter();

            playerModel = null;
        }
    }

    private void DrawSwitcherGUI()
    {
        GUILayout.Space(10);

        GUILayout.Label("Switch your Character!");

        GUILayout.Space(50);

        if (GUILayout.Button("Switch Character", GUILayout.Height(75)))
        {
            SwitchCharacter();

            playerModel = null;
        }

        newPlayerModel = EditorGUILayout.ObjectField(newPlayerModel, typeof(GameObject), true) as GameObject;
    }

    private void SetupCharacter()
    {
        if (playerModel == null)
        {
            Debug.Log("Please make sure to assign a humanoid player model");
            return;
        }

        GameObject playerSystem = Instantiate(PlayerSystemPrefab);

        playerSystem.name = PlayerSystemPrefab.name;

        Transform player = playerSystem.transform.Find("Player");

        Animator playerAnim = player.GetComponentInChildren<Animator>();
        Animator playerModelAnimator = playerModel.GetComponent<Animator>();

        Avatar playerModelAvatar = playerModelAnimator.avatar;

        DestroyImmediate(playerModelAnimator);
        CopyComponent(playerModel, playerAnim);

        Animator characterAnim = playerModel.GetComponent<Animator>();
        
        playerModel.GetComponent<Animator>().avatar = playerModelAvatar;

        CopyComponent(playerModel.gameObject, playerAnim.GetComponent<HKPlayerAnimEventModel>());
        CopyComponent(playerModel.gameObject, playerAnim.GetComponent<RigBuilder>());
        CopyComponent(playerModel.gameObject, playerAnim.GetComponent<BoneRenderer>());

        playerModel.transform.SetParent(playerAnim.transform.parent);
        playerModel.transform.SetSiblingIndex(0);

        if (PrefabUtility.IsPartOfPrefabInstance(playerModel))
        {
            PrefabUtility.UnpackPrefabInstance(playerModel, PrefabUnpackMode.Completely, InteractionMode.UserAction);
        }

        playerAnim.transform.Find("Rig 1").SetParent(playerModel.transform);

        playerSystem.transform.position = playerModel.transform.position;

        TransitionHumanoidCharacter(playerAnim, characterAnim);

        DestroyImmediate(playerAnim.gameObject);

        // Assign problematic references.
        HKPlayerManager hKPlayerManager = player.GetComponent<HKPlayerManager>();
        HKPlayerLocomotionCC hKPlayerLocomotionCC = player.GetComponent<HKPlayerLocomotionCC>();
        HKPlayerHealthSystem hKPlayerHealth = player.GetComponent<HKPlayerHealthSystem>();

        AssignPrivatePropertyObj(hKPlayerManager, characterAnim.GetComponent<HKPlayerAnimEventModel>(), "_hKPlayerAnimEventModel");
        AssignPrivatePropertyObj(hKPlayerLocomotionCC, characterAnim, "_playerAnimator");
        AssignPrivatePropertyObj(hKPlayerLocomotionCC, characterAnim.GetBoneTransform(HumanBodyBones.Head), "_headTransform");

        List<Rigidbody> ragdollRigidbodies = new List<Rigidbody>
        {
            characterAnim.GetBoneTransform(HumanBodyBones.Hips).GetComponent<Rigidbody>(),
            characterAnim.GetBoneTransform(HumanBodyBones.LeftUpperLeg).GetComponent<Rigidbody>(),
            characterAnim.GetBoneTransform(HumanBodyBones.LeftLowerLeg).GetComponent<Rigidbody>(),
            characterAnim.GetBoneTransform(HumanBodyBones.RightUpperLeg).GetComponent<Rigidbody>(),
            characterAnim.GetBoneTransform(HumanBodyBones.RightLowerLeg).GetComponent<Rigidbody>(),
            characterAnim.GetBoneTransform(HumanBodyBones.Spine).GetComponent<Rigidbody>(),
            characterAnim.GetBoneTransform(HumanBodyBones.LeftUpperArm).GetComponent<Rigidbody>(),
            characterAnim.GetBoneTransform(HumanBodyBones.LeftLowerArm).GetComponent<Rigidbody>(),
            characterAnim.GetBoneTransform(HumanBodyBones.Head).GetComponent<Rigidbody>(),
            characterAnim.GetBoneTransform(HumanBodyBones.RightUpperArm).GetComponent<Rigidbody>(),
            characterAnim.GetBoneTransform(HumanBodyBones.RightLowerArm).GetComponent<Rigidbody>(),
        };

        AssignPrivatePropertyList(hKPlayerHealth, ragdollRigidbodies.Cast<UnityEngine.Object>().ToList(), "_ragdollRigidbodies");

        FollowTransformPosLateUpdate spineFollower = player.Find("SpineHierarchies").Find("SpineFollower").GetComponent<FollowTransformPosLateUpdate>();
        spineFollower.Target = characterAnim.GetBoneTransform(HumanBodyBones.UpperChest);

        Transform centerSpinePos = spineFollower.transform.GetChild(0);

        FollowTransformPosAndRot playerBackSlots = centerSpinePos.Find("PlayerBackSlots").GetComponent<FollowTransformPosAndRot>();
        playerBackSlots.Target = characterAnim.GetBoneTransform(HumanBodyBones.UpperChest);

        Transform handsFollowers = centerSpinePos.Find("HandsFollow");
        FollowTransformPosAndRot leftHandSlotDynamic = handsFollowers.Find("LeftHandSlotDynamic").GetComponent<FollowTransformPosAndRot>();
        FollowTransformPosAndRot rightHandSlotDynamic = handsFollowers.Find("RightHandSlotDynamic").GetComponent<FollowTransformPosAndRot>();
        FollowTransformPosAndRot rightHandSlotBone = handsFollowers.Find("RightHandBoneFollower").GetComponent<FollowTransformPosAndRot>();

        leftHandSlotDynamic.Target = characterAnim.GetBoneTransform(HumanBodyBones.LeftHand);
        rightHandSlotDynamic.Target = characterAnim.GetBoneTransform(HumanBodyBones.RightHand);
        rightHandSlotBone.Target = characterAnim.GetBoneTransform(HumanBodyBones.RightHand);


        FollowTransformPosLateUpdate spineBone = player.Find("SpineHierarchies").Find("SpineBone").GetComponent<FollowTransformPosLateUpdate>();
        FollowTransformPosLateUpdate spineBone1 = spineBone.transform.Find("SpineBone1").GetComponent<FollowTransformPosLateUpdate>();
        FollowTransformPosLateUpdate spineBone2 = spineBone1.transform.Find("SpineBone2").GetComponent<FollowTransformPosLateUpdate>();

        spineBone.Target = characterAnim.GetBoneTransform(HumanBodyBones.Spine);
        spineBone1.Target = characterAnim.GetBoneTransform(HumanBodyBones.Chest);
        spineBone2.Target = characterAnim.GetBoneTransform(HumanBodyBones.UpperChest);

        FollowTransformPosAndRot rightUpLegSlot = player.GetChild(2).Find("RightUpLegSlot").GetComponent<FollowTransformPosAndRot>();
        rightUpLegSlot.Target = characterAnim.GetBoneTransform(HumanBodyBones.RightUpperLeg);

        Rig cRig = characterAnim.GetComponentInChildren<Rig>();

        Transform bodyRigP = cRig.transform.Find("Body");

        bodyRigP.Find("SpineRotRig").GetComponent<MultiRotationConstraint>().data.constrainedObject = characterAnim.GetBoneTransform(HumanBodyBones.Spine);
        bodyRigP.Find("Spine1RotRig").GetComponent<MultiRotationConstraint>().data.constrainedObject = characterAnim.GetBoneTransform(HumanBodyBones.Chest);
        bodyRigP.Find("Spine2RotRig").GetComponent<MultiRotationConstraint>().data.constrainedObject = characterAnim.GetBoneTransform(HumanBodyBones.UpperChest);
        bodyRigP.Find("HeadAimRig").GetComponent<MultiRotationConstraint>().data.constrainedObject = characterAnim.GetBoneTransform(HumanBodyBones.Head);

        Transform handsRigP = cRig.transform.Find("Hands");

        TwoBoneIKConstraint leftHandIKConstraint = handsRigP.Find("LeftHandIK").GetComponent<TwoBoneIKConstraint>();
        TwoBoneIKConstraint rightHandIKConstraint = handsRigP.Find("RightHandIK").GetComponent<TwoBoneIKConstraint>();

        leftHandIKConstraint.data.tip = characterAnim.GetBoneTransform(HumanBodyBones.LeftHand);
        leftHandIKConstraint.data.mid = characterAnim.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        leftHandIKConstraint.data.root = characterAnim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        rightHandIKConstraint.data.tip = characterAnim.GetBoneTransform(HumanBodyBones.RightHand);
        rightHandIKConstraint.data.mid = characterAnim.GetBoneTransform(HumanBodyBones.RightLowerArm);
        rightHandIKConstraint.data.root = characterAnim.GetBoneTransform(HumanBodyBones.RightUpperArm);

        Transform leftHandIKBased = leftHandIKConstraint.transform.Find("IKBasedFingers");
        SetLeftHandIKConstraintsTargets(leftHandIKBased, characterAnim);

        Transform rightHandIKBased = rightHandIKConstraint.transform.Find("IKBasedFingers");
        SetRightHandIKConstraintsTargets(rightHandIKBased, characterAnim);

        Transform leftHandFKBased = leftHandIKConstraint.transform.Find("DirectRotationMatchFingers");
        SetLeftHandFKConstraintTargets(leftHandFKBased, characterAnim);

        Transform rightHandFKBased = rightHandIKConstraint.transform.Find("DirectRotationMatchFingers");
        SetRightHandFKConstraintTargets(rightHandFKBased, characterAnim);

        SetAllLayers(playerModel, characterAnim);
    }

    private void SwitchCharacter()
    {
        if (playerModel == null || newPlayerModel == null)
        {
            Debug.Log("Please make sure to assign a humanoid player model");
            return;
        }

        Transform player = playerModel.transform.parent;

        Animator playerAnim = player.GetComponentInChildren<Animator>();
        Animator playerModelAnimator = newPlayerModel.GetComponent<Animator>();

        Avatar playerModelAvatar = playerModelAnimator.avatar;

        DestroyImmediate(playerModelAnimator);
        CopyComponent(newPlayerModel, playerAnim);

        Animator characterAnim = newPlayerModel.GetComponent<Animator>();

        newPlayerModel.GetComponent<Animator>().avatar = playerModelAvatar;

        CopyComponent(newPlayerModel.gameObject, playerAnim.GetComponent<HKPlayerAnimEventModel>());
        CopyComponent(newPlayerModel.gameObject, playerAnim.GetComponent<RigBuilder>());
        CopyComponent(newPlayerModel.gameObject, playerAnim.GetComponent<BoneRenderer>());

        newPlayerModel.transform.SetParent(playerAnim.transform.parent);
        newPlayerModel.transform.SetSiblingIndex(0);

        if (PrefabUtility.IsPartOfPrefabInstance(newPlayerModel))
        {
            PrefabUtility.UnpackPrefabInstance(newPlayerModel, PrefabUnpackMode.Completely, InteractionMode.UserAction);
        }

        playerAnim.transform.Find("Rig 1").SetParent(newPlayerModel.transform);

        //playerSystem.transform.position = playerModel.transform.position;

        TransitionHumanoidCharacter(playerAnim, characterAnim);

        DestroyImmediate(playerAnim.gameObject);

        // Assign problematic references.
        HKPlayerManager hKPlayerManager = player.GetComponent<HKPlayerManager>();
        HKPlayerLocomotionCC hKPlayerLocomotionCC = player.GetComponent<HKPlayerLocomotionCC>();
        HKPlayerHealthSystem hKPlayerHealth = player.GetComponent<HKPlayerHealthSystem>();

        AssignPrivatePropertyObj(hKPlayerManager, characterAnim.GetComponent<HKPlayerAnimEventModel>(), "_hKPlayerAnimEventModel");
        AssignPrivatePropertyObj(hKPlayerLocomotionCC, characterAnim, "_playerAnimator");
        AssignPrivatePropertyObj(hKPlayerLocomotionCC, characterAnim.GetBoneTransform(HumanBodyBones.Head), "_headTransform");

        List<Rigidbody> ragdollRigidbodies = new List<Rigidbody>
        {
            characterAnim.GetBoneTransform(HumanBodyBones.Hips).GetComponent<Rigidbody>(),
            characterAnim.GetBoneTransform(HumanBodyBones.LeftUpperLeg).GetComponent<Rigidbody>(),
            characterAnim.GetBoneTransform(HumanBodyBones.LeftLowerLeg).GetComponent<Rigidbody>(),
            characterAnim.GetBoneTransform(HumanBodyBones.RightUpperLeg).GetComponent<Rigidbody>(),
            characterAnim.GetBoneTransform(HumanBodyBones.RightLowerLeg).GetComponent<Rigidbody>(),
            characterAnim.GetBoneTransform(HumanBodyBones.Spine).GetComponent<Rigidbody>(),
            characterAnim.GetBoneTransform(HumanBodyBones.LeftUpperArm).GetComponent<Rigidbody>(),
            characterAnim.GetBoneTransform(HumanBodyBones.LeftLowerArm).GetComponent<Rigidbody>(),
            characterAnim.GetBoneTransform(HumanBodyBones.Head).GetComponent<Rigidbody>(),
            characterAnim.GetBoneTransform(HumanBodyBones.RightUpperArm).GetComponent<Rigidbody>(),
            characterAnim.GetBoneTransform(HumanBodyBones.RightLowerArm).GetComponent<Rigidbody>(),
        };

        AssignPrivatePropertyList(hKPlayerHealth, ragdollRigidbodies.Cast<UnityEngine.Object>().ToList(), "_ragdollRigidbodies");

        FollowTransformPosLateUpdate spineFollower = player.Find("SpineHierarchies").Find("SpineFollower").GetComponent<FollowTransformPosLateUpdate>();
        spineFollower.Target = characterAnim.GetBoneTransform(HumanBodyBones.UpperChest);

        Transform centerSpinePos = spineFollower.transform.GetChild(0);

        FollowTransformPosAndRot playerBackSlots = centerSpinePos.Find("PlayerBackSlots").GetComponent<FollowTransformPosAndRot>();
        playerBackSlots.Target = characterAnim.GetBoneTransform(HumanBodyBones.UpperChest);

        Transform handsFollowers = centerSpinePos.Find("HandsFollow");
        FollowTransformPosAndRot leftHandSlotDynamic = handsFollowers.Find("LeftHandSlotDynamic").GetComponent<FollowTransformPosAndRot>();
        FollowTransformPosAndRot rightHandSlotDynamic = handsFollowers.Find("RightHandSlotDynamic").GetComponent<FollowTransformPosAndRot>();
        FollowTransformPosAndRot rightHandSlotBone = handsFollowers.Find("RightHandBoneFollower").GetComponent<FollowTransformPosAndRot>();

        leftHandSlotDynamic.Target = characterAnim.GetBoneTransform(HumanBodyBones.LeftHand);
        rightHandSlotDynamic.Target = characterAnim.GetBoneTransform(HumanBodyBones.RightHand);
        rightHandSlotBone.Target = characterAnim.GetBoneTransform(HumanBodyBones.RightHand);


        FollowTransformPosLateUpdate spineBone = player.Find("SpineHierarchies").Find("SpineBone").GetComponent<FollowTransformPosLateUpdate>();
        FollowTransformPosLateUpdate spineBone1 = spineBone.transform.Find("SpineBone1").GetComponent<FollowTransformPosLateUpdate>();
        FollowTransformPosLateUpdate spineBone2 = spineBone1.transform.Find("SpineBone2").GetComponent<FollowTransformPosLateUpdate>();

        spineBone.Target = characterAnim.GetBoneTransform(HumanBodyBones.Spine);
        spineBone1.Target = characterAnim.GetBoneTransform(HumanBodyBones.Chest);
        spineBone2.Target = characterAnim.GetBoneTransform(HumanBodyBones.UpperChest);

        FollowTransformPosAndRot rightUpLegSlot = player.GetChild(2).Find("RightUpLegSlot").GetComponent<FollowTransformPosAndRot>();
        rightUpLegSlot.Target = characterAnim.GetBoneTransform(HumanBodyBones.RightUpperLeg);

        Rig cRig = characterAnim.GetComponentInChildren<Rig>();

        Transform bodyRigP = cRig.transform.Find("Body");

        bodyRigP.Find("SpineRotRig").GetComponent<MultiRotationConstraint>().data.constrainedObject = characterAnim.GetBoneTransform(HumanBodyBones.Spine);
        bodyRigP.Find("Spine1RotRig").GetComponent<MultiRotationConstraint>().data.constrainedObject = characterAnim.GetBoneTransform(HumanBodyBones.Chest);
        bodyRigP.Find("Spine2RotRig").GetComponent<MultiRotationConstraint>().data.constrainedObject = characterAnim.GetBoneTransform(HumanBodyBones.UpperChest);
        bodyRigP.Find("HeadAimRig").GetComponent<MultiRotationConstraint>().data.constrainedObject = characterAnim.GetBoneTransform(HumanBodyBones.Head);

        Transform handsRigP = cRig.transform.Find("Hands");

        TwoBoneIKConstraint leftHandIKConstraint = handsRigP.Find("LeftHandIK").GetComponent<TwoBoneIKConstraint>();
        TwoBoneIKConstraint rightHandIKConstraint = handsRigP.Find("RightHandIK").GetComponent<TwoBoneIKConstraint>();

        leftHandIKConstraint.data.tip = characterAnim.GetBoneTransform(HumanBodyBones.LeftHand);
        leftHandIKConstraint.data.mid = characterAnim.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        leftHandIKConstraint.data.root = characterAnim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        rightHandIKConstraint.data.tip = characterAnim.GetBoneTransform(HumanBodyBones.RightHand);
        rightHandIKConstraint.data.mid = characterAnim.GetBoneTransform(HumanBodyBones.RightLowerArm);
        rightHandIKConstraint.data.root = characterAnim.GetBoneTransform(HumanBodyBones.RightUpperArm);

        Transform leftHandIKBased = leftHandIKConstraint.transform.Find("IKBasedFingers");
        SetLeftHandIKConstraintsTargets(leftHandIKBased, characterAnim);

        Transform rightHandIKBased = rightHandIKConstraint.transform.Find("IKBasedFingers");
        SetRightHandIKConstraintsTargets(rightHandIKBased, characterAnim);

        Transform leftHandFKBased = leftHandIKConstraint.transform.Find("DirectRotationMatchFingers");
        SetLeftHandFKConstraintTargets(leftHandFKBased, characterAnim);

        Transform rightHandFKBased = rightHandIKConstraint.transform.Find("DirectRotationMatchFingers");
        SetRightHandFKConstraintTargets(rightHandFKBased, characterAnim);

        SetAllLayers(newPlayerModel, characterAnim);
    }

    private void SetAllLayers(GameObject playerModel, Animator animator)
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        int ragdollLayer = LayerMask.NameToLayer("Ragdoll");
        int shootableLayer = LayerMask.NameToLayer("Shootable");

        // Player Layers
        SetLayerRecursively(playerModel, playerLayer);

        // Ragdoll Layer
        SetLayer(animator.GetBoneTransform(HumanBodyBones.Hips).gameObject, ragdollLayer);

        SetLayer(animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).gameObject, ragdollLayer);
        SetLayer(animator.GetBoneTransform(HumanBodyBones.RightUpperLeg).gameObject, ragdollLayer);

        SetLayer(animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).gameObject, ragdollLayer);
        SetLayer(animator.GetBoneTransform(HumanBodyBones.RightLowerLeg).gameObject, ragdollLayer);

        SetLayer(animator.GetBoneTransform(HumanBodyBones.Spine).gameObject, ragdollLayer);

        SetLayer(animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).gameObject, ragdollLayer);
        SetLayer(animator.GetBoneTransform(HumanBodyBones.RightUpperArm).gameObject, ragdollLayer);

        SetLayer(animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).gameObject, ragdollLayer);
        SetLayer(animator.GetBoneTransform(HumanBodyBones.RightLowerArm).gameObject, ragdollLayer);

        SetLayer(animator.GetBoneTransform(HumanBodyBones.Head).gameObject, ragdollLayer);

        // Shootable Layer
        SetLayer(animator.GetBoneTransform(HumanBodyBones.Hips).Find("HipHitBox").gameObject, shootableLayer);
        SetLayer(animator.GetBoneTransform(HumanBodyBones.Spine).Find("SpineHitBox").gameObject, shootableLayer);
        SetLayer(animator.GetBoneTransform(HumanBodyBones.Chest).Find("Spine1HitBox").gameObject, shootableLayer);
        SetLayer(animator.GetBoneTransform(HumanBodyBones.UpperChest).Find("Spine2HitBox").gameObject, shootableLayer);

        SetLayer(animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).Find("LeftHipHitBox").gameObject, shootableLayer);
        SetLayer(animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).Find("LeftLegHitBox").gameObject, shootableLayer);
        SetLayer(animator.GetBoneTransform(HumanBodyBones.LeftFoot).Find("LeftFootHitBox").gameObject, shootableLayer);

        SetLayer(animator.GetBoneTransform(HumanBodyBones.RightUpperLeg).Find("RightHipHitBox").gameObject, shootableLayer);
        SetLayer(animator.GetBoneTransform(HumanBodyBones.RightLowerLeg).Find("RightLegHitBox").gameObject, shootableLayer);
        SetLayer(animator.GetBoneTransform(HumanBodyBones.RightFoot).Find("RightFootHitBox").gameObject, shootableLayer);

        SetLayer(animator.GetBoneTransform(HumanBodyBones.LeftShoulder).Find("LeftShoulderHitBox").gameObject, shootableLayer);
        SetLayer(animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).Find("LeftArmHitBox").gameObject, shootableLayer);
        SetLayer(animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).Find("LeftElbowHitBox").gameObject, shootableLayer);
        SetLayer(animator.GetBoneTransform(HumanBodyBones.LeftHand).Find("LeftHandHitBox").gameObject, shootableLayer);

        SetLayer(animator.GetBoneTransform(HumanBodyBones.RightShoulder).Find("RightShoulderHitBox").gameObject, shootableLayer);
        SetLayer(animator.GetBoneTransform(HumanBodyBones.RightUpperArm).Find("RightArmHitBox").gameObject, shootableLayer);
        SetLayer(animator.GetBoneTransform(HumanBodyBones.RightLowerArm).Find("RightElbowHitBox").gameObject, shootableLayer);
        SetLayer(animator.GetBoneTransform(HumanBodyBones.RightHand).Find("RightHandHitBox").gameObject, shootableLayer);

        SetLayer(animator.GetBoneTransform(HumanBodyBones.Head).Find("HeadHitBoxBack").gameObject, shootableLayer);
        SetLayer(animator.GetBoneTransform(HumanBodyBones.Head).Find("HeadHitBoxFront").gameObject, shootableLayer);
        SetLayer(animator.GetBoneTransform(HumanBodyBones.Head).Find("HeadHitBoxFrontEnd").gameObject, shootableLayer);
    }

    private void SetLeftHandIKConstraintsTargets(Transform leftHandIKBased, Animator characterAnim)
    {
        TwoBoneIKConstraint leftHandIndexIK = leftHandIKBased.Find("LeftHandIndexIK").GetComponent<TwoBoneIKConstraint>();
        leftHandIndexIK.data.tip = characterAnim.GetBoneTransform(HumanBodyBones.LeftIndexDistal);
        leftHandIndexIK.data.mid = characterAnim.GetBoneTransform(HumanBodyBones.LeftIndexIntermediate);
        leftHandIndexIK.data.root = characterAnim.GetBoneTransform(HumanBodyBones.LeftIndexProximal);

        TwoBoneIKConstraint leftHandMiddleIK = leftHandIKBased.Find("LeftHandMiddleIK").GetComponent<TwoBoneIKConstraint>();
        leftHandMiddleIK.data.tip = characterAnim.GetBoneTransform(HumanBodyBones.LeftMiddleDistal);
        leftHandMiddleIK.data.mid = characterAnim.GetBoneTransform(HumanBodyBones.LeftMiddleIntermediate);
        leftHandMiddleIK.data.root = characterAnim.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);

        TwoBoneIKConstraint leftHandPinkyIK = leftHandIKBased.Find("LeftHandPinkyIK").GetComponent<TwoBoneIKConstraint>();
        leftHandPinkyIK.data.tip = characterAnim.GetBoneTransform(HumanBodyBones.LeftLittleDistal);
        leftHandPinkyIK.data.mid = characterAnim.GetBoneTransform(HumanBodyBones.LeftLittleIntermediate);
        leftHandPinkyIK.data.root = characterAnim.GetBoneTransform(HumanBodyBones.LeftLittleProximal);

        TwoBoneIKConstraint leftHandRingIK = leftHandIKBased.Find("LeftHandRingIK").GetComponent<TwoBoneIKConstraint>();
        leftHandRingIK.data.tip = characterAnim.GetBoneTransform(HumanBodyBones.LeftRingDistal);
        leftHandRingIK.data.mid = characterAnim.GetBoneTransform(HumanBodyBones.LeftRingIntermediate);
        leftHandRingIK.data.root = characterAnim.GetBoneTransform(HumanBodyBones.LeftRingProximal);

        TwoBoneIKConstraint leftHandThumbIK = leftHandIKBased.Find("LeftHandThumbIK").GetComponent<TwoBoneIKConstraint>();
        leftHandThumbIK.data.tip = characterAnim.GetBoneTransform(HumanBodyBones.LeftThumbDistal);
        leftHandThumbIK.data.mid = characterAnim.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate);
        leftHandThumbIK.data.root = characterAnim.GetBoneTransform(HumanBodyBones.LeftThumbProximal);
    }

    private void SetRightHandIKConstraintsTargets(Transform rightHandIKBased, Animator characterAnim)
    {
        TwoBoneIKConstraint rightHandIndexIK = rightHandIKBased.Find("RightHandIndexIK").GetComponent<TwoBoneIKConstraint>();
        rightHandIndexIK.data.tip = characterAnim.GetBoneTransform(HumanBodyBones.RightIndexDistal);
        rightHandIndexIK.data.mid = characterAnim.GetBoneTransform(HumanBodyBones.RightIndexIntermediate);
        rightHandIndexIK.data.root = characterAnim.GetBoneTransform(HumanBodyBones.RightIndexProximal);

        TwoBoneIKConstraint rightHandMiddleIK = rightHandIKBased.Find("RightHandMiddleIK").GetComponent<TwoBoneIKConstraint>();
        rightHandMiddleIK.data.tip = characterAnim.GetBoneTransform(HumanBodyBones.RightMiddleDistal);
        rightHandMiddleIK.data.mid = characterAnim.GetBoneTransform(HumanBodyBones.RightMiddleIntermediate);
        rightHandMiddleIK.data.root = characterAnim.GetBoneTransform(HumanBodyBones.RightMiddleProximal);

        TwoBoneIKConstraint rightHandPinkyIK = rightHandIKBased.Find("RightHandPinkyIK").GetComponent<TwoBoneIKConstraint>();
        rightHandPinkyIK.data.tip = characterAnim.GetBoneTransform(HumanBodyBones.RightLittleDistal);
        rightHandPinkyIK.data.mid = characterAnim.GetBoneTransform(HumanBodyBones.RightLittleIntermediate);
        rightHandPinkyIK.data.root = characterAnim.GetBoneTransform(HumanBodyBones.RightLittleProximal);

        TwoBoneIKConstraint rightHandRingIK = rightHandIKBased.Find("RightHandRingIK").GetComponent<TwoBoneIKConstraint>();
        rightHandRingIK.data.tip = characterAnim.GetBoneTransform(HumanBodyBones.RightRingDistal);
        rightHandRingIK.data.mid = characterAnim.GetBoneTransform(HumanBodyBones.RightRingIntermediate);
        rightHandRingIK.data.root = characterAnim.GetBoneTransform(HumanBodyBones.RightRingProximal);

        TwoBoneIKConstraint rightHandThumbIK = rightHandIKBased.Find("RightHandThumbIK").GetComponent<TwoBoneIKConstraint>();
        rightHandThumbIK.data.tip = characterAnim.GetBoneTransform(HumanBodyBones.RightThumbDistal);
        rightHandThumbIK.data.mid = characterAnim.GetBoneTransform(HumanBodyBones.RightThumbIntermediate);
        rightHandThumbIK.data.root = characterAnim.GetBoneTransform(HumanBodyBones.RightThumbProximal);
    }

    private void SetLeftHandFKConstraintTargets(Transform leftHandFKBased, Animator characterAnim)
    {
        // Index Finger
        MatchRotationToTarget leftHandFKIndex1 = leftHandFKBased.Find("LeftHandIndex1Constraint").GetComponent<MatchRotationToTarget>();
        AssignPrivatePropertyObj(leftHandFKIndex1, characterAnim.GetBoneTransform(HumanBodyBones.LeftIndexProximal), "_constrained");
        AssignPrivatePropertyObj(leftHandFKIndex1.transform.GetChild(0).GetComponent<MatchRotationToTarget>(), characterAnim.GetBoneTransform(HumanBodyBones.LeftIndexIntermediate), "_constrained");
        AssignPrivatePropertyObj(leftHandFKIndex1.transform.GetChild(0).GetChild(0).GetComponent<MatchRotationToTarget>(), characterAnim.GetBoneTransform(HumanBodyBones.LeftIndexDistal), "_constrained");

        // Middle Finger
        MatchRotationToTarget leftHandFKMiddle1 = leftHandFKBased.Find("LeftHandMiddle1Constraint").GetComponent<MatchRotationToTarget>();
        AssignPrivatePropertyObj(leftHandFKMiddle1, characterAnim.GetBoneTransform(HumanBodyBones.LeftMiddleProximal), "_constrained");
        AssignPrivatePropertyObj(leftHandFKMiddle1.transform.GetChild(0).GetComponent<MatchRotationToTarget>(), characterAnim.GetBoneTransform(HumanBodyBones.LeftMiddleIntermediate), "_constrained");
        AssignPrivatePropertyObj(leftHandFKMiddle1.transform.GetChild(0).GetChild(0).GetComponent<MatchRotationToTarget>(), characterAnim.GetBoneTransform(HumanBodyBones.LeftMiddleDistal), "_constrained");

        // Pinky Finger
        MatchRotationToTarget leftHandFKPinky1 = leftHandFKBased.Find("LeftHandPinky1Constraint").GetComponent<MatchRotationToTarget>();
        AssignPrivatePropertyObj(leftHandFKPinky1, characterAnim.GetBoneTransform(HumanBodyBones.LeftLittleProximal), "_constrained");
        AssignPrivatePropertyObj(leftHandFKPinky1.transform.GetChild(0).GetComponent<MatchRotationToTarget>(), characterAnim.GetBoneTransform(HumanBodyBones.LeftLittleIntermediate), "_constrained");
        AssignPrivatePropertyObj(leftHandFKPinky1.transform.GetChild(0).GetChild(0).GetComponent<MatchRotationToTarget>(), characterAnim.GetBoneTransform(HumanBodyBones.LeftLittleDistal), "_constrained");

        // Ring Finger
        MatchRotationToTarget leftHandFKRing1 = leftHandFKBased.Find("LeftHandRing1Constraint").GetComponent<MatchRotationToTarget>();
        AssignPrivatePropertyObj(leftHandFKRing1, characterAnim.GetBoneTransform(HumanBodyBones.LeftRingProximal), "_constrained");
        AssignPrivatePropertyObj(leftHandFKRing1.transform.GetChild(0).GetComponent<MatchRotationToTarget>(), characterAnim.GetBoneTransform(HumanBodyBones.LeftRingIntermediate), "_constrained");
        AssignPrivatePropertyObj(leftHandFKRing1.transform.GetChild(0).GetChild(0).GetComponent<MatchRotationToTarget>(), characterAnim.GetBoneTransform(HumanBodyBones.LeftRingDistal), "_constrained");

        // Thumb
        MatchRotationToTarget leftHandFKThumb1 = leftHandFKBased.Find("LeftHandThumb1Constraint").GetComponent<MatchRotationToTarget>();
        AssignPrivatePropertyObj(leftHandFKThumb1, characterAnim.GetBoneTransform(HumanBodyBones.LeftThumbProximal), "_constrained");
        AssignPrivatePropertyObj(leftHandFKThumb1.transform.GetChild(0).GetComponent<MatchRotationToTarget>(), characterAnim.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate), "_constrained");
        AssignPrivatePropertyObj(leftHandFKThumb1.transform.GetChild(0).GetChild(0).GetComponent<MatchRotationToTarget>(), characterAnim.GetBoneTransform(HumanBodyBones.LeftThumbDistal), "_constrained");
    }

    private void SetRightHandFKConstraintTargets(Transform rightHandFKBased, Animator characterAnim)
    {
        // Index Finger
        MatchRotationToTarget rightHandFKIndex1 = rightHandFKBased.Find("RightHandIndex1Constraint").GetComponent<MatchRotationToTarget>();
        AssignPrivatePropertyObj(rightHandFKIndex1, characterAnim.GetBoneTransform(HumanBodyBones.RightIndexProximal), "_constrained");
        AssignPrivatePropertyObj(rightHandFKIndex1.transform.GetChild(0).GetComponent<MatchRotationToTarget>(), characterAnim.GetBoneTransform(HumanBodyBones.RightIndexIntermediate), "_constrained");
        AssignPrivatePropertyObj(rightHandFKIndex1.transform.GetChild(0).GetChild(0).GetComponent<MatchRotationToTarget>(), characterAnim.GetBoneTransform(HumanBodyBones.RightIndexDistal), "_constrained");

        // Middle Finger
        MatchRotationToTarget rightHandFKMiddle1 = rightHandFKBased.Find("RightHandMiddle1Constraint").GetComponent<MatchRotationToTarget>();
        AssignPrivatePropertyObj(rightHandFKMiddle1, characterAnim.GetBoneTransform(HumanBodyBones.RightMiddleProximal), "_constrained");
        AssignPrivatePropertyObj(rightHandFKMiddle1.transform.GetChild(0).GetComponent<MatchRotationToTarget>(), characterAnim.GetBoneTransform(HumanBodyBones.RightMiddleIntermediate), "_constrained");
        AssignPrivatePropertyObj(rightHandFKMiddle1.transform.GetChild(0).GetChild(0).GetComponent<MatchRotationToTarget>(), characterAnim.GetBoneTransform(HumanBodyBones.RightMiddleDistal), "_constrained");

        // Pinky Finger
        MatchRotationToTarget rightHandFKPinky1 = rightHandFKBased.Find("RightHandPinky1Constraint").GetComponent<MatchRotationToTarget>();
        AssignPrivatePropertyObj(rightHandFKPinky1, characterAnim.GetBoneTransform(HumanBodyBones.RightLittleProximal), "_constrained");
        AssignPrivatePropertyObj(rightHandFKPinky1.transform.GetChild(0).GetComponent<MatchRotationToTarget>(), characterAnim.GetBoneTransform(HumanBodyBones.RightLittleIntermediate), "_constrained");
        AssignPrivatePropertyObj(rightHandFKPinky1.transform.GetChild(0).GetChild(0).GetComponent<MatchRotationToTarget>(), characterAnim.GetBoneTransform(HumanBodyBones.RightLittleDistal), "_constrained");

        // Ring Finger
        MatchRotationToTarget rightHandFKRing1 = rightHandFKBased.Find("RightHandRing1Constraint").GetComponent<MatchRotationToTarget>();
        AssignPrivatePropertyObj(rightHandFKRing1, characterAnim.GetBoneTransform(HumanBodyBones.RightRingProximal), "_constrained");
        AssignPrivatePropertyObj(rightHandFKRing1.transform.GetChild(0).GetComponent<MatchRotationToTarget>(), characterAnim.GetBoneTransform(HumanBodyBones.RightRingIntermediate), "_constrained");
        AssignPrivatePropertyObj(rightHandFKRing1.transform.GetChild(0).GetChild(0).GetComponent<MatchRotationToTarget>(), characterAnim.GetBoneTransform(HumanBodyBones.RightRingDistal), "_constrained");

        // Thumb
        MatchRotationToTarget rightHandFKThumb1 = rightHandFKBased.Find("RightHandThumb1Constraint").GetComponent<MatchRotationToTarget>();
        AssignPrivatePropertyObj(rightHandFKThumb1, characterAnim.GetBoneTransform(HumanBodyBones.RightThumbProximal), "_constrained");
        AssignPrivatePropertyObj(rightHandFKThumb1.transform.GetChild(0).GetComponent<MatchRotationToTarget>(), characterAnim.GetBoneTransform(HumanBodyBones.RightThumbIntermediate), "_constrained");
        AssignPrivatePropertyObj(rightHandFKThumb1.transform.GetChild(0).GetChild(0).GetComponent<MatchRotationToTarget>(), characterAnim.GetBoneTransform(HumanBodyBones.RightThumbDistal), "_constrained");
    }


    void SetLayer(GameObject obj, int layer)
    {
        obj.layer = layer;
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    private T FindAssetByExactName<T>(string assetName) where T : UnityEngine.Object
    {
        // Find assets of type T
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);

            // Check if the loaded asset has the exact name we're looking for
            if (asset != null && asset.name.Equals(assetName))
            {
                return asset;
            }
        }

        Debug.LogWarning($"Asset with name '{assetName}' and type '{typeof(T).Name}' not found.");
        return null;
    }

    private void TransitionHumanoidCharacter(Animator playerAnim, Animator characterAnim)
    {
        #region Hips & Legs

        // Hip
        Transform playerAnimHip = playerAnim.GetBoneTransform(HumanBodyBones.Hips);
        Transform characterAnimHip = characterAnim.GetBoneTransform(HumanBodyBones.Hips);

        CopyComponent(characterAnimHip.gameObject, playerAnimHip.GetComponent<BoxCollider>());
        CopyComponent(characterAnimHip.gameObject, playerAnimHip.GetComponent<Rigidbody>());

        playerAnimHip.Find("HipHitBox").SetParent(characterAnimHip);

        // Left Upper Leg
        Transform playerAnimLeftUpperLeg = playerAnim.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        Transform characterAnimLeftUpperLeg = characterAnim.GetBoneTransform(HumanBodyBones.LeftUpperLeg);

        CopyComponent(characterAnimLeftUpperLeg.gameObject, playerAnimLeftUpperLeg.GetComponent<CapsuleCollider>());
        CopyComponent(characterAnimLeftUpperLeg.gameObject, playerAnimLeftUpperLeg.GetComponent<Rigidbody>());
        CopyComponent(characterAnimLeftUpperLeg.gameObject, playerAnimLeftUpperLeg.GetComponent<CharacterJoint>());
        CopyComponent(characterAnimLeftUpperLeg.gameObject, playerAnimLeftUpperLeg.GetComponent<HKPlayerBodyPartHitBox>());

        playerAnimLeftUpperLeg.Find("LeftHipHitBox").SetParent(characterAnimLeftUpperLeg);

        // Right Upper Leg
        Transform playerAnimRightUpperLeg = playerAnim.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        Transform characterAnimRightUpperLeg = characterAnim.GetBoneTransform(HumanBodyBones.RightUpperLeg);

        CopyComponent(characterAnimRightUpperLeg.gameObject, playerAnimRightUpperLeg.GetComponent<CapsuleCollider>());
        CopyComponent(characterAnimRightUpperLeg.gameObject, playerAnimRightUpperLeg.GetComponent<Rigidbody>());
        CopyComponent(characterAnimRightUpperLeg.gameObject, playerAnimRightUpperLeg.GetComponent<CharacterJoint>());
        CopyComponent(characterAnimRightUpperLeg.gameObject, playerAnimRightUpperLeg.GetComponent<HKPlayerBodyPartHitBox>());

        playerAnimRightUpperLeg.Find("RightHipHitBox").SetParent(characterAnimRightUpperLeg);

        // Left Leg
        Transform playerAnimLeftLeg = playerAnim.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        Transform characterAnimLeftLeg = characterAnim.GetBoneTransform(HumanBodyBones.LeftLowerLeg);

        CopyComponent(characterAnimLeftLeg.gameObject, playerAnimLeftLeg.GetComponent<CapsuleCollider>());
        CopyComponent(characterAnimLeftLeg.gameObject, playerAnimLeftLeg.GetComponent<Rigidbody>());
        CopyComponent(characterAnimLeftLeg.gameObject, playerAnimLeftLeg.GetComponent<CharacterJoint>());
        CopyComponent(characterAnimLeftLeg.gameObject, playerAnimLeftLeg.GetComponent<HKPlayerBodyPartHitBox>());

        playerAnimLeftLeg.Find("LeftLegHitBox").SetParent(characterAnimLeftLeg);

        // Right Leg
        Transform playerAnimRightLeg = playerAnim.GetBoneTransform(HumanBodyBones.RightLowerLeg);
        Transform characterAnimRightLeg = characterAnim.GetBoneTransform(HumanBodyBones.RightLowerLeg);

        CopyComponent(characterAnimRightLeg.gameObject, playerAnimRightLeg.GetComponent<CapsuleCollider>());
        CopyComponent(characterAnimRightLeg.gameObject, playerAnimRightLeg.GetComponent<Rigidbody>());
        CopyComponent(characterAnimRightLeg.gameObject, playerAnimRightLeg.GetComponent<CharacterJoint>());
        CopyComponent(characterAnimRightLeg.gameObject, playerAnimRightLeg.GetComponent<HKPlayerBodyPartHitBox>());

        playerAnimRightLeg.Find("RightLegHitBox").SetParent(characterAnimRightLeg);

        // Left Foot
        Transform playerAnimLeftFoot = playerAnim.GetBoneTransform(HumanBodyBones.LeftFoot);
        Transform characterAnimLeftFoot = characterAnim.GetBoneTransform(HumanBodyBones.LeftFoot);

        CopyComponent(characterAnimLeftFoot.gameObject, playerAnimLeftFoot.GetComponent<HKPlayerBodyPartHitBox>());

        playerAnimLeftFoot.Find("LeftFootHitBox").SetParent(characterAnimLeftFoot);

        // Right Foot
        Transform playerAnimRightFoot = playerAnim.GetBoneTransform(HumanBodyBones.RightFoot);
        Transform characterAnimRightFoot = characterAnim.GetBoneTransform(HumanBodyBones.RightFoot);

        CopyComponent(characterAnimRightFoot.gameObject, playerAnimRightFoot.GetComponent<HKPlayerBodyPartHitBox>());

        playerAnimRightFoot.Find("RightFootHitBox").SetParent(characterAnimRightFoot);

        #endregion

        #region Spine/Chest/UpperChest

        // Spine1
        Transform playerAnimSpine = playerAnim.GetBoneTransform(HumanBodyBones.Spine);
        Transform characterAnimSpine = characterAnim.GetBoneTransform(HumanBodyBones.Spine);

        CopyComponent(characterAnimSpine.gameObject, playerAnimSpine.GetComponent<BoxCollider>());
        CopyComponent(characterAnimSpine.gameObject, playerAnimSpine.GetComponent<Rigidbody>());
        CopyComponent(characterAnimSpine.gameObject, playerAnimSpine.GetComponent<CharacterJoint>());
        CopyComponent(characterAnimSpine.gameObject, playerAnimSpine.GetComponent<HKPlayerBodyPartHitBox>());

        playerAnimSpine.Find("SpineHitBox").SetParent(characterAnimSpine);

        // Chest
        Transform playerAnimChest = playerAnim.GetBoneTransform(HumanBodyBones.Chest);
        Transform characterAnimChest = characterAnim.GetBoneTransform(HumanBodyBones.Chest);

        CopyComponent(characterAnimChest.gameObject, playerAnimChest.GetComponent<HKPlayerBodyPartHitBox>());

        playerAnimChest.Find("Spine1HitBox").SetParent(characterAnimChest);

        // Upper Chest
        Transform playerAnimUpperChest = playerAnim.GetBoneTransform(HumanBodyBones.UpperChest);
        Transform characterAnimUpperChest = characterAnim.GetBoneTransform(HumanBodyBones.UpperChest);

        CopyComponent(characterAnimUpperChest.gameObject, playerAnimUpperChest.GetComponent<HKPlayerBodyPartHitBox>());

        playerAnimUpperChest.Find("Spine2HitBox").SetParent(characterAnimUpperChest);

        playerAnimUpperChest.Find("LeftHandIKTarget").SetParent(characterAnimUpperChest);
        playerAnimUpperChest.Find("RightHandIKTarget").SetParent(characterAnimUpperChest);

        #endregion

        #region Hands

        // Left Shoulder
        Transform playerAnimLeftShoulder = playerAnim.GetBoneTransform(HumanBodyBones.LeftShoulder);
        Transform characterAnimLeftShoulder = characterAnim.GetBoneTransform(HumanBodyBones.LeftShoulder);

        CopyComponent(characterAnimLeftShoulder.gameObject, playerAnimLeftShoulder.GetComponent<HKPlayerBodyPartHitBox>());

        playerAnimLeftShoulder.Find("LeftShoulderHitBox").SetParent(characterAnimLeftShoulder);

        // Right Shoulder
        Transform playerAnimRightShoulder = playerAnim.GetBoneTransform(HumanBodyBones.RightShoulder);
        Transform characterAnimRightShoulder = characterAnim.GetBoneTransform(HumanBodyBones.RightShoulder);

        CopyComponent(characterAnimRightShoulder.gameObject, playerAnimRightShoulder.GetComponent<HKPlayerBodyPartHitBox>());

        playerAnimRightShoulder.Find("RightShoulderHitBox").SetParent(characterAnimRightShoulder);

        // Left Arm
        Transform playerAnimLeftArm = playerAnim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        Transform characterAnimLeftArm = characterAnim.GetBoneTransform(HumanBodyBones.LeftUpperArm);

        CopyComponent(characterAnimLeftArm.gameObject, playerAnimLeftArm.GetComponent<CapsuleCollider>());
        CopyComponent(characterAnimLeftArm.gameObject, playerAnimLeftArm.GetComponent<Rigidbody>());
        CopyComponent(characterAnimLeftArm.gameObject, playerAnimLeftArm.GetComponent<CharacterJoint>());
        CopyComponent(characterAnimLeftArm.gameObject, playerAnimLeftArm.GetComponent<HKPlayerBodyPartHitBox>());

        playerAnimLeftArm.Find("LeftArmHitBox").SetParent(characterAnimLeftArm);

        // Right Arm
        Transform playerAnimRightArm = playerAnim.GetBoneTransform(HumanBodyBones.RightUpperArm);
        Transform characterAnimRightArm = characterAnim.GetBoneTransform(HumanBodyBones.RightUpperArm);

        CopyComponent(characterAnimRightArm.gameObject, playerAnimRightArm.GetComponent<CapsuleCollider>());
        CopyComponent(characterAnimRightArm.gameObject, playerAnimRightArm.GetComponent<Rigidbody>());
        CopyComponent(characterAnimRightArm.gameObject, playerAnimRightArm.GetComponent<CharacterJoint>());
        CopyComponent(characterAnimRightArm.gameObject, playerAnimRightArm.GetComponent<HKPlayerBodyPartHitBox>());

        playerAnimRightArm.Find("RightArmHitBox").SetParent(characterAnimRightArm);

        // Left ForeArm
        Transform playerAnimLeftForeArm = playerAnim.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        Transform characterAnimLeftForeArm = characterAnim.GetBoneTransform(HumanBodyBones.LeftLowerArm);

        CopyComponent(characterAnimLeftForeArm.gameObject, playerAnimLeftForeArm.GetComponent<CapsuleCollider>());
        CopyComponent(characterAnimLeftForeArm.gameObject, playerAnimLeftForeArm.GetComponent<Rigidbody>());
        CopyComponent(characterAnimLeftForeArm.gameObject, playerAnimLeftForeArm.GetComponent<CharacterJoint>());
        CopyComponent(characterAnimLeftForeArm.gameObject, playerAnimLeftForeArm.GetComponent<HKPlayerBodyPartHitBox>());

        playerAnimLeftForeArm.Find("LeftElbowHitBox").SetParent(characterAnimLeftForeArm);

        // Right ForeArm
        Transform playerAnimRightForeArm = playerAnim.GetBoneTransform(HumanBodyBones.RightLowerArm);
        Transform characterAnimRightForeArm = characterAnim.GetBoneTransform(HumanBodyBones.RightLowerArm);

        CopyComponent(characterAnimRightForeArm.gameObject, playerAnimRightForeArm.GetComponent<CapsuleCollider>());
        CopyComponent(characterAnimRightForeArm.gameObject, playerAnimRightForeArm.GetComponent<Rigidbody>());
        CopyComponent(characterAnimRightForeArm.gameObject, playerAnimRightForeArm.GetComponent<CharacterJoint>());
        CopyComponent(characterAnimRightForeArm.gameObject, playerAnimRightForeArm.GetComponent<HKPlayerBodyPartHitBox>());

        playerAnimRightForeArm.Find("RightElbowHitBox").SetParent(characterAnimRightForeArm);

        // Left Hand
        Transform playerAnimLeftHand = playerAnim.GetBoneTransform(HumanBodyBones.LeftHand);
        Transform characterAnimLeftHand = characterAnim.GetBoneTransform(HumanBodyBones.LeftHand);

        CopyComponent(characterAnimLeftHand.gameObject, playerAnimLeftHand.GetComponent<HKPlayerBodyPartHitBox>());

        playerAnimLeftHand.Find("LeftHandHitBox").SetParent(characterAnimLeftHand);

        // Right Hand
        Transform playerAnimRightHand = playerAnim.GetBoneTransform(HumanBodyBones.RightHand);
        Transform characterAnimRightHand = characterAnim.GetBoneTransform(HumanBodyBones.RightHand);

        CopyComponent(characterAnimRightHand.gameObject, playerAnimRightHand.GetComponent<HKPlayerBodyPartHitBox>());

        playerAnimRightHand.Find("RightHandHitBox").SetParent(characterAnimRightHand);

        #endregion

        #region Head

        // Spine1
        Transform playerAnimHead = playerAnim.GetBoneTransform(HumanBodyBones.Head);
        Transform characterAnimHead = characterAnim.GetBoneTransform(HumanBodyBones.Head);

        CopyComponent(characterAnimHead.gameObject, playerAnimHead.GetComponent<SphereCollider>());
        CopyComponent(characterAnimHead.gameObject, playerAnimHead.GetComponent<Rigidbody>());
        CopyComponent(characterAnimHead.gameObject, playerAnimHead.GetComponent<CharacterJoint>());
        CopyComponent(characterAnimHead.gameObject, playerAnimHead.GetComponent<HKPlayerBodyPartHitBox>());

        playerAnimHead.Find("HeadHitBoxBack").SetParent(characterAnimHead);
        playerAnimHead.Find("HeadHitBoxFront").SetParent(characterAnimHead);
        playerAnimHead.Find("HeadHitBoxFrontEnd").SetParent(characterAnimHead);

        #endregion
    }

    public Component CopyComponent(GameObject destination, Component original)
    {
        // Store the original name
        string originalName = destination.name;

        Type componentType = original.GetType();
        Component copy = destination.AddComponent(componentType);

        // Copy all fields (public and private) from the original component to the copy
        foreach (FieldInfo field in componentType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            field.SetValue(copy, field.GetValue(original));
        }

        // Copy all writable properties from the original to the copy
        foreach (PropertyInfo property in componentType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (property.CanWrite && property.GetIndexParameters().Length == 0)
            {
                property.SetValue(copy, property.GetValue(original));
            }
        }

        // Restore the original name
        destination.name = originalName;

        return copy;
    }

    #region Spawnerold
    //private string rigWeightSetterRigPropertyName = "rig";
    //private string rigLayersPropertyName = "m_RigLayers";
    //private string rigLayersRigPropertyName = "m_Rig";
    //private string rigLayersActivePropertyName = "m_Active";
    //private string _iKConstraintsPropertyName = "_iKConstraints";

    //private string _playerUIPropertyName = "_playerUI";
    //private string _playerAnimatorName = "_playerAnimator";
    //private string _gunHolderAnimatorName = "_gunHolderAnimator";
    //private string _centerSpinePosName = "_centerSpinePos";
    //private string _cameraHolderName = "_cameraHolder";
    //private string _headTransformName = "_headTransform";
    //private string _gunHolderName = "_gunHolder";
    //private string _iKBasedFingersWeightModifierName = "_iKBasedFingersWeightModifier";
    //private string _rotationConstraitBasedFingersWeightModifierName = "_rotationConstraitBasedFingersWeightModifier";

    //// Left HandsIKFollowers 
    //private string _handsIKFollowersLeftHandIKTransformName = "LeftHandIKTransform";
    //private string _handsIKFollowersLeftHandIndexIKTransformName = "LeftHandIndexIKTransform";
    //private string _handsIKFollowersLeftHandMiddleIKTransformName = "LeftHandMiddleIKTransform";
    //private string _handsIKFollowersLeftHandPinkyIKTransformName = "LeftHandPinkyIKTransform";
    //private string _handsIKFollowersLeftHandRingIKTransformName = "LeftHandRingIKTransform";
    //private string _handsIKFollowersLeftHandThumbIKTransformName = "LeftHandThumbIKTransform";

    //// Right HandsIKFollowers
    //private string _handsIKFollowersRightHandIKTransformName = "RightHandIKTransform";
    //private string _handsIKFollowersRightHandIndexIKTransformName = "RightHandIndexIKTransform";
    //private string _handsIKFollowersRightHandMiddleIKTransformName = "RightHandMiddleIKTransform";
    //private string _handsIKFollowersRightHandPinkyIKTransformName = "RightHandPinkyIKTransform";
    //private string _handsIKFollowersRightHandRingIKTransformName = "RightHandRingIKTransform";
    //private string _handsIKFollowersRightHandThumbIKTransformName = "RightHandThumbIKTransform";

    //// Left hand Constraint Followers
    //private string _handsRotationConstraintFollowersLeftHandIKTransformName = "LeftHandIKTransform";

    //private string _handsRotationConstraintFollowersLeftHandIndex1ConstraintTransformName = "LeftHandIndex1ConstraintTransform";
    //private string _handsRotationConstraintFollowersLeftHandIndex2ConstraintTransformName = "LeftHandIndex2ConstraintTransform";
    //private string _handsRotationConstraintFollowersLeftHandIndex3ConstraintTransformName = "LeftHandIndex3ConstraintTransform";

    //private string _handsRotationConstraintFollowersLeftHandMiddle1ConstraintTransformName = "LeftHandMiddle1ConstraintTransform";
    //private string _handsRotationConstraintFollowersLeftHandMiddle2ConstraintTransformName = "LeftHandMiddle2ConstraintTransform";
    //private string _handsRotationConstraintFollowersLeftHandMiddle3ConstraintTransformName = "LeftHandMiddle3ConstraintTransform";

    //private string _handsRotationConstraintFollowersLeftHandPinky1ConstraintTransformName = "LeftHandPinky1ConstraintTransform";
    //private string _handsRotationConstraintFollowersLeftHandPinky2ConstraintTransformName = "LeftHandPinky2ConstraintTransform";
    //private string _handsRotationConstraintFollowersLeftHandPinky3ConstraintTransformName = "LeftHandPinky3ConstraintTransform";

    //private string _handsRotationConstraintFollowersLeftHandRing1ConstraintTransformName = "LeftHandRing1ConstraintTransform";
    //private string _handsRotationConstraintFollowersLeftHandRing2ConstraintTransformName = "LeftHandRing2ConstraintTransform";
    //private string _handsRotationConstraintFollowersLeftHandRing3ConstraintTransformName = "LeftHandRing3ConstraintTransform";

    //private string _handsRotationConstraintFollowersLeftHandThumb1ConstraintTransformName = "LeftHandThumb1ConstraintTransform";
    //private string _handsRotationConstraintFollowersLeftHandThumb2ConstraintTransformName = "LeftHandThumb2ConstraintTransform";
    //private string _handsRotationConstraintFollowersLeftHandThumb3ConstraintTransformName = "LeftHandThumb3ConstraintTransform";

    //// Right hand Constraint Followers
    //private string _handsRotationConstraintFollowersRightHandIKTransformName = "RightHandIKTransform";

    //private string _handsRotationConstraintFollowersRightHandIndex1ConstraintTransformName = "RightHandIndex1ConstraintTransform";
    //private string _handsRotationConstraintFollowersRightHandIndex2ConstraintTransformName = "RightHandIndex2ConstraintTransform";
    //private string _handsRotationConstraintFollowersRightHandIndex3ConstraintTransformName = "RightHandIndex3ConstraintTransform";

    //private string _handsRotationConstraintFollowersRightHandMiddle1ConstraintTransformName = "RightHandMiddle1ConstraintTransform";
    //private string _handsRotationConstraintFollowersRightHandMiddle2ConstraintTransformName = "RightHandMiddle2ConstraintTransform";
    //private string _handsRotationConstraintFollowersRightHandMiddle3ConstraintTransformName = "RightHandMiddle3ConstraintTransform";

    //private string _handsRotationConstraintFollowersRightHandPinky1ConstraintTransformName = "RightHandPinky1ConstraintTransform";
    //private string _handsRotationConstraintFollowersRightHandPinky2ConstraintTransformName = "RightHandPinky2ConstraintTransform";
    //private string _handsRotationConstraintFollowersRightHandPinky3ConstraintTransformName = "RightHandPinky3ConstraintTransform";

    //private string _handsRotationConstraintFollowersRightHandRing1ConstraintTransformName = "RightHandRing1ConstraintTransform";
    //private string _handsRotationConstraintFollowersRightHandRing2ConstraintTransformName = "RightHandRing2ConstraintTransform";
    //private string _handsRotationConstraintFollowersRightHandRing3ConstraintTransformName = "RightHandRing3ConstraintTransform";

    //private string _handsRotationConstraintFollowersRightHandThumb1ConstraintTransformName = "RightHandThumb1ConstraintTransform";
    //private string _handsRotationConstraintFollowersRightHandThumb2ConstraintTransformName = "RightHandThumb2ConstraintTransform";
    //private string _handsRotationConstraintFollowersRightHandThumb3ConstraintTransformName = "RightHandThumb3ConstraintTransform";

    //// Contraints
    //private string _constrainedName = "_constrained";
    //private string _toMatchName = "_toMatch";

    //private string _targetName = "_target";


    //private string _followLPWAyName = "_y";
    //private string _followLPWAzName = "_z";


    //private string _clipProjectorPosName = "_clipProjectorPos";
    //private string _clipProjectorName = "_clipProjector";
    //private string _clipVisualName = "_clipVisual";

    //private string _rifleSlotOneName = "_rifleSlotOne";
    //private string _rifleSlotTwoName = "_rifleSlotTwo";
    //private string _pistolSlotName = "_pistolSlot";
    //private string _meleeSlotName = "_meleeSlot";
    //private string _gunHolderSlotName = "_gunHolderSlot";
    //private string _rightHandFollowerName = "_rightHandFollower";
    //private string _handsIKWeightModifierName = "_handsIKWeightModifier";

    //private string _interactPointerName = "_interactPointer";

    #endregion

    //private void SetupCharacter()
    //{
    //    if (playerModel == null)
    //    {
    //        Debug.LogWarning("Player Model has not been assigned, please ensure to assign the player model before you setup Character");
    //        return;
    //    }

    //    #region Player & PlayerModel

    //    GameObject playerParentGameObject = new GameObject("HKFpsPlayer");
    //    GameObject rig = new GameObject("Rig 1");

    //    playerParentGameObject.AddComponent<CharacterController>();
    //    playerParentGameObject.AddComponent<HKPlayerInput>();
    //    playerParentGameObject.AddComponent<HKPlayerManager>();
    //    playerParentGameObject.AddComponent<HKPlayerLocomotionCC>();
    //    playerParentGameObject.AddComponent<HKPlayerWeaponManager>();
    //    playerParentGameObject.AddComponent<HKPlayerHealthSystem>();
    //    playerParentGameObject.AddComponent<HKPlayerAudioEffects>();
    //    playerParentGameObject.AddComponent<HKPlayerItemSystem>();
    //    playerParentGameObject.AddComponent<HKPlayerInteraction>();
    //    playerParentGameObject.AddComponent<HKPlayerInventory>();

    //    playerModel.transform.SetParent(playerParentGameObject.transform);
    //    playerModel.AddComponent<Animator>();
    //    Animator playerModelAnimator = playerModel.GetComponent<Animator>();
    //    playerModelAnimator.applyRootMotion = false;

    //    playerModel.AddComponent<PlayerModel>();
    //    playerModel.AddComponent<RigBuilder>();

    //    rig.transform.SetParent(playerModel.transform);

    //    rig.AddComponent<Rig>();

    //    SerializedObject rigBuilderSObj = new SerializedObject(playerModel.GetComponent<RigBuilder>());
    //    SerializedProperty rigLayersProperty = rigBuilderSObj.FindProperty(rigLayersPropertyName);

    //    rigLayersProperty.arraySize++;
    //    SerializedProperty newElement = rigLayersProperty.GetArrayElementAtIndex(rigLayersProperty.arraySize - 1);

    //    newElement.FindPropertyRelative(rigLayersRigPropertyName).objectReferenceValue = rig.GetComponent<Rig>();
    //    newElement.FindPropertyRelative(rigLayersActivePropertyName).boolValue = true;

    //    rigBuilderSObj.ApplyModifiedProperties();


    //    #endregion

    //    #region GameObject's Creation
    //    GameObject body = new GameObject("Body");
    //    GameObject hands = new GameObject("Hands");

    //    GameObject spineRotRig = new GameObject("SpineRotRig");
    //    GameObject spine1RotRig = new GameObject("Spine1RotRig");
    //    GameObject spine2RotRig = new GameObject("Spine2RotRig");
    //    GameObject headAimRig = new GameObject("HeadAimRig");

    //    GameObject iKBasedFingersWeight = new GameObject("IKBasedFingersWeight");
    //    GameObject rotationConstrainedFingersWeight = new GameObject("RotationConstrainedFingersWeight");
    //    GameObject handsIKWeight = new GameObject("HandsIKWeightModifier");
    //    GameObject leftHandIK = new GameObject("LeftHandIK");
    //    GameObject rightHandIK = new GameObject("RightHandIK");


    //    GameObject directRotationMatchFingersL = new GameObject("DirectRotationMatchFingers");
    //    GameObject iKBasedFingersL = new GameObject("IKBasedFingers");

    //    GameObject directRotationMatchFingersR = new GameObject("DirectRotationMatchFingers");
    //    GameObject iKBasedFingersR = new GameObject("IKBasedFingers");



    //    GameObject leftHandIndex1Constraint = new GameObject("LeftHandIndex1Constraint");
    //    GameObject leftHandIndex2Constraint = new GameObject("LeftHandIndex2Constraint");
    //    GameObject leftHandIndex3Constraint = new GameObject("LeftHandIndex3Constraint");

    //    GameObject leftHandMiddle1Constraint = new GameObject("LeftHandMiddle1Constraint");
    //    GameObject leftHandMiddle2Constraint = new GameObject("LeftHandMiddle2Constraint");
    //    GameObject leftHandMiddle3Constraint = new GameObject("LeftHandMiddle3Constraint");

    //    GameObject leftHandPinky1Constraint = new GameObject("LeftHandPinky1Constraint");
    //    GameObject leftHandPinky2Constraint = new GameObject("LeftHandPinky2Constraint");
    //    GameObject leftHandPinky3Constraint = new GameObject("LeftHandPinky3Constraint");

    //    GameObject leftHandRing1Constraint = new GameObject("LeftHandRing1Constraint");
    //    GameObject leftHandRing2Constraint = new GameObject("LeftHandRing2Constraint");
    //    GameObject leftHandRing3Constraint = new GameObject("LeftHandRing3Constraint");

    //    GameObject leftHandThumb1Constraint = new GameObject("LeftHandThumb1Constraint");
    //    GameObject leftHandThumb2Constraint = new GameObject("LeftHandThumb2Constraint");
    //    GameObject leftHandThumb3Constraint = new GameObject("LeftHandThumb3Constraint");

    //    GameObject leftHandIndexIK = new GameObject("LeftHandIndexIK");
    //    GameObject leftHandMiddleIK = new GameObject("LeftHandMiddleIK");
    //    GameObject leftHandPinkyIK = new GameObject("LeftHandPinkyIK");
    //    GameObject leftHandRingIK = new GameObject("LeftHandRingIK");
    //    GameObject leftHandThumbIK = new GameObject("LeftHandThumbIK");

    //    GameObject rightHandIndex1Constraint = new GameObject("RightHandIndex1Constraint");
    //    GameObject rightHandIndex2Constraint = new GameObject("RightHandIndex2Constraint");
    //    GameObject rightHandIndex3Constraint = new GameObject("RightHandIndex3Constraint");

    //    GameObject rightHandMiddle1Constraint = new GameObject("RightHandMiddle1Constraint");
    //    GameObject rightHandMiddle2Constraint = new GameObject("RightHandMiddle2Constraint");
    //    GameObject rightHandMiddle3Constraint = new GameObject("RightHandMiddle3Constraint");

    //    GameObject rightHandPinky1Constraint = new GameObject("RightHandPinky1Constraint");
    //    GameObject rightHandPinky2Constraint = new GameObject("RightHandPinky2Constraint");
    //    GameObject rightHandPinky3Constraint = new GameObject("RightHandPinky3Constraint");

    //    GameObject rightHandRing1Constraint = new GameObject("RightHandRing1Constraint");
    //    GameObject rightHandRing2Constraint = new GameObject("RightHandRing2Constraint");
    //    GameObject rightHandRing3Constraint = new GameObject("RightHandRing3Constraint");

    //    GameObject rightHandThumb1Constraint = new GameObject("RightHandThumb1Constraint");
    //    GameObject rightHandThumb2Constraint = new GameObject("RightHandThumb2Constraint");
    //    GameObject rightHandThumb3Constraint = new GameObject("RightHandThumb3Constraint");

    //    GameObject rightHandIndexIK = new GameObject("RightHandIndexIK");
    //    GameObject rightHandMiddleIK = new GameObject("RightHandMiddleIK");
    //    GameObject rightHandPinkyIK = new GameObject("RightHandPinkyIK");
    //    GameObject rightHandRingIK = new GameObject("RightHandRingIK");
    //    GameObject rightHandThumbIK = new GameObject("RightHandThumbIK");

    //    GameObject leftHandConstraintIndex1Target = new GameObject("LeftHandIndex1ConstraintTarget");
    //    GameObject leftHandConstraintIndex2Target = new GameObject("LeftHandIndex2ConstraintTarget");
    //    GameObject leftHandConstraintIndex3Target = new GameObject("LeftHandIndex3ConstraintTarget");

    //    GameObject leftHandConstraintMiddle1Target = new GameObject("LeftHandMiddle1ConstraintTarget");
    //    GameObject leftHandConstraintMiddle2Target = new GameObject("LeftHandMiddle2ConstraintTarget");
    //    GameObject leftHandConstraintMiddle3Target = new GameObject("LeftHandMiddle3ConstraintTarget");

    //    GameObject leftHandConstraintPinky1Target = new GameObject("LeftHandPinky1ConstraintTarget");
    //    GameObject leftHandConstraintPinky2Target = new GameObject("LeftHandPinky2ConstraintTarget");
    //    GameObject leftHandConstraintPinky3Target = new GameObject("LeftHandPinky3ConstraintTarget");

    //    GameObject leftHandConstraintRing1Target = new GameObject("LeftHandRing1ConstraintTarget");
    //    GameObject leftHandConstraintRing2Target = new GameObject("LeftHandRing2ConstraintTarget");
    //    GameObject leftHandConstraintRing3Target = new GameObject("LeftHandRing3ConstraintTarget");

    //    GameObject leftHandConstraintThumb1Target = new GameObject("LeftHandThumb1ConstraintTarget");
    //    GameObject leftHandConstraintThumb2Target = new GameObject("LeftHandThumb2ConstraintTarget");
    //    GameObject leftHandConstraintThumb3Target = new GameObject("LeftHandThumb3ConstraintTarget");


    //    GameObject leftHandIndexIKTarget = new GameObject("LeftHandIndexIKTarget");
    //    GameObject leftHandMiddleIKTarget = new GameObject("LeftHandMiddleIKTarget");
    //    GameObject leftHandPinkyIKTarget = new GameObject("LeftHandPinkyIKTarget");
    //    GameObject leftHandRingIKTarget = new GameObject("LeftHandRingIKTarget");
    //    GameObject leftHandThumbIKTarget = new GameObject("LeftHandThumbIKTarget");

    //    GameObject rightHandConstraintIndex1Target = new GameObject("RightHandIndex1ConstraintTarget");
    //    GameObject rightHandConstraintIndex2Target = new GameObject("RightHandIndex2ConstraintTarget");
    //    GameObject rightHandConstraintIndex3Target = new GameObject("RightHandIndex3ConstraintTarget");

    //    GameObject rightHandConstraintMiddle1Target = new GameObject("RightHandMiddle1ConstraintTarget");
    //    GameObject rightHandConstraintMiddle2Target = new GameObject("RightHandMiddle2ConstraintTarget");
    //    GameObject rightHandConstraintMiddle3Target = new GameObject("RightHandMiddle3ConstraintTarget");

    //    GameObject rightHandConstraintPinky1Target = new GameObject("RightHandPinky1ConstraintTarget");
    //    GameObject rightHandConstraintPinky2Target = new GameObject("RightHandPinky2ConstraintTarget");
    //    GameObject rightHandConstraintPinky3Target = new GameObject("RightHandPinky3ConstraintTarget");

    //    GameObject rightHandConstraintRing1Target = new GameObject("RightHandRing1ConstraintTarget");
    //    GameObject rightHandConstraintRing2Target = new GameObject("RightHandRing2ConstraintTarget");
    //    GameObject rightHandConstraintRing3Target = new GameObject("RightHandRing3ConstraintTarget");

    //    GameObject rightHandConstraintThumb1Target = new GameObject("RightHandThumb1ConstraintTarget");
    //    GameObject rightHandConstraintThumb2Target = new GameObject("RightHandThumb2ConstraintTarget");
    //    GameObject rightHandConstraintThumb3Target = new GameObject("RightHandThumb3ConstraintTarget");


    //    GameObject rightHandIndexIKTarget = new GameObject("RightHandIndexIKTarget");
    //    GameObject rightHandMiddleIKTarget = new GameObject("RightHandMiddleIKTarget");
    //    GameObject rightHandPinkyIKTarget = new GameObject("RightHandPinkyIKTarget");
    //    GameObject rightHandRingIKTarget = new GameObject("RightHandRingIKTarget");
    //    GameObject rightHandThumbIKTarget = new GameObject("RightHandThumbIKTarget");

    //    GameObject iKBasedFingersTargetL = new GameObject("IKBasedFingersTarget");
    //    GameObject directRotationMatchFingersTargetL = new GameObject("DirectRotationMatchFingers");

    //    GameObject iKBasedFingersTargetR = new GameObject("IKBasedFingersTarget");
    //    GameObject directRotationMatchFingersTargetR = new GameObject("directRotationMatchFingers");


    //    GameObject leftHandIKTarget = new GameObject("LeftHandIKTarget");
    //    GameObject rightHandIKTarget = new GameObject("RightHandIKTarget");

    //    // Spine Follower
    //    GameObject spineFollower = new GameObject("SpineFollower");

    //    GameObject centerSpinePos = new GameObject("CenterSpinePos");
    //    GameObject clipProjectorPos = new GameObject("ClipProjectorPos");

    //    GameObject gunHolderAnimated = new GameObject("GunHolderAnimated");
    //    GameObject cameraHolder = new GameObject("CameraHolder");
    //    GameObject gunHolder = new GameObject("GunHolder");

    //    GameObject clipProjector = new GameObject("ClipProjector");
    //    GameObject interactPointer = new GameObject("InteractPointer");
    //    GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);

    //    // Slots/Sockets
    //    GameObject slotsSockets = new GameObject("Slots/Sockets");

    //    GameObject rightHandSlot = new GameObject("RightHandSlot");
    //    GameObject playerBackSlots = new GameObject("PlayerBackSlot");
    //    GameObject rightUpLegSlot = new GameObject("RightUpLegSlot");

    //    GameObject rifleSlotOne = new GameObject("RifleSlotOne");
    //    GameObject rifleSlotTwo = new GameObject("RifleSlotTwo");

    //    GameObject pistolSlot = new GameObject("PistolSlot");

    //    GameObject knifeSlotPosition = new GameObject("KnifeSlotPosition");
    //    GameObject knifeSlot = new GameObject("KnifeSlot");

    //    #endregion

    //    #region Parenting
    //    spineRotRig.transform.SetParent(body.transform);
    //    spine1RotRig.transform.SetParent(body.transform);
    //    spine2RotRig.transform.SetParent(body.transform);
    //    headAimRig.transform.SetParent(body.transform);

    //    iKBasedFingersWeight.transform.SetParent(hands.transform);
    //    rotationConstrainedFingersWeight.transform.SetParent(hands.transform);
    //    handsIKWeight.transform.SetParent(hands.transform);
    //    leftHandIK.transform.SetParent(hands.transform);
    //    rightHandIK.transform.SetParent(hands.transform);

    //    directRotationMatchFingersL.transform.SetParent(leftHandIK.transform);
    //    iKBasedFingersL.transform.SetParent(leftHandIK.transform);

    //    directRotationMatchFingersR.transform.SetParent(rightHandIK.transform);
    //    iKBasedFingersR.transform.SetParent(rightHandIK.transform);

    //    body.transform.SetParent(rig.transform);
    //    hands.transform.SetParent(rig.transform);

    //    leftHandIndexIK.transform.SetParent(iKBasedFingersL.transform);
    //    leftHandMiddleIK.transform.SetParent(iKBasedFingersL.transform);
    //    leftHandPinkyIK.transform.SetParent(iKBasedFingersL.transform);
    //    leftHandRingIK.transform.SetParent(iKBasedFingersL.transform);
    //    leftHandThumbIK.transform.SetParent(iKBasedFingersL.transform);

    //    leftHandIndex1Constraint.transform.SetParent(directRotationMatchFingersL.transform);
    //    leftHandIndex2Constraint.transform.SetParent(leftHandIndex1Constraint.transform);
    //    leftHandIndex3Constraint.transform.SetParent(leftHandIndex2Constraint.transform);

    //    leftHandMiddle1Constraint.transform.SetParent(directRotationMatchFingersL.transform);
    //    leftHandMiddle2Constraint.transform.SetParent(leftHandMiddle1Constraint.transform);
    //    leftHandMiddle3Constraint.transform.SetParent(leftHandMiddle2Constraint.transform);

    //    leftHandPinky1Constraint.transform.SetParent(directRotationMatchFingersL.transform);
    //    leftHandPinky2Constraint.transform.SetParent(leftHandPinky1Constraint.transform);
    //    leftHandPinky3Constraint.transform.SetParent(leftHandPinky2Constraint.transform);

    //    leftHandRing1Constraint.transform.SetParent(directRotationMatchFingersL.transform);
    //    leftHandRing2Constraint.transform.SetParent(leftHandRing1Constraint.transform);
    //    leftHandRing3Constraint.transform.SetParent(leftHandRing2Constraint.transform);

    //    leftHandThumb1Constraint.transform.SetParent(directRotationMatchFingersL.transform);
    //    leftHandThumb2Constraint.transform.SetParent(leftHandThumb1Constraint.transform);
    //    leftHandThumb3Constraint.transform.SetParent(leftHandThumb2Constraint.transform);

    //    rightHandIndexIK.transform.SetParent(iKBasedFingersR.transform);
    //    rightHandMiddleIK.transform.SetParent(iKBasedFingersR.transform);
    //    rightHandPinkyIK.transform.SetParent(iKBasedFingersR.transform);
    //    rightHandRingIK.transform.SetParent(iKBasedFingersR.transform);
    //    rightHandThumbIK.transform.SetParent(iKBasedFingersR.transform);

    //    rightHandIndex1Constraint.transform.SetParent(directRotationMatchFingersR.transform);
    //    rightHandIndex2Constraint.transform.SetParent(rightHandIndex1Constraint.transform);
    //    rightHandIndex3Constraint.transform.SetParent(rightHandIndex2Constraint.transform);

    //    rightHandMiddle1Constraint.transform.SetParent(directRotationMatchFingersR.transform);
    //    rightHandMiddle2Constraint.transform.SetParent(rightHandMiddle1Constraint.transform);
    //    rightHandMiddle3Constraint.transform.SetParent(rightHandMiddle2Constraint.transform);

    //    rightHandPinky1Constraint.transform.SetParent(directRotationMatchFingersR.transform);
    //    rightHandPinky2Constraint.transform.SetParent(rightHandPinky1Constraint.transform);
    //    rightHandPinky3Constraint.transform.SetParent(rightHandPinky2Constraint.transform);

    //    rightHandRing1Constraint.transform.SetParent(directRotationMatchFingersR.transform);
    //    rightHandRing2Constraint.transform.SetParent(rightHandRing1Constraint.transform);
    //    rightHandRing3Constraint.transform.SetParent(rightHandRing2Constraint.transform);

    //    rightHandThumb1Constraint.transform.SetParent(directRotationMatchFingersR.transform);
    //    rightHandThumb2Constraint.transform.SetParent(rightHandThumb1Constraint.transform);
    //    rightHandThumb3Constraint.transform.SetParent(rightHandThumb2Constraint.transform);

    //    // Right hand and Left hand IK parenting

    //    // Left hand
    //    iKBasedFingersTargetL.transform.SetParent(leftHandIKTarget.transform);
    //    directRotationMatchFingersTargetL.transform.SetParent(leftHandIKTarget.transform);

    //    leftHandIndexIKTarget.transform.SetParent(iKBasedFingersTargetL.transform);
    //    leftHandMiddleIKTarget.transform.SetParent(iKBasedFingersTargetL.transform);
    //    leftHandPinkyIKTarget.transform.SetParent(iKBasedFingersTargetL.transform);
    //    leftHandRingIKTarget.transform.SetParent(iKBasedFingersTargetL.transform);
    //    leftHandThumbIKTarget.transform.SetParent(iKBasedFingersTargetL.transform);

    //    leftHandConstraintIndex1Target.transform.SetParent(directRotationMatchFingersTargetL.transform);
    //    leftHandConstraintIndex2Target.transform.SetParent(leftHandConstraintIndex1Target.transform);
    //    leftHandConstraintIndex3Target.transform.SetParent(leftHandConstraintIndex2Target.transform);


    //    leftHandConstraintMiddle1Target.transform.SetParent(directRotationMatchFingersTargetL.transform);
    //    leftHandConstraintMiddle2Target.transform.SetParent(leftHandConstraintMiddle1Target.transform);
    //    leftHandConstraintMiddle3Target.transform.SetParent(leftHandConstraintMiddle2Target.transform);


    //    leftHandConstraintPinky1Target.transform.SetParent(directRotationMatchFingersTargetL.transform);
    //    leftHandConstraintPinky2Target.transform.SetParent(leftHandConstraintPinky1Target.transform);
    //    leftHandConstraintPinky3Target.transform.SetParent(leftHandConstraintPinky2Target.transform);

    //    leftHandConstraintRing1Target.transform.SetParent(directRotationMatchFingersTargetL.transform);
    //    leftHandConstraintRing2Target.transform.SetParent(leftHandConstraintRing1Target.transform);
    //    leftHandConstraintRing3Target.transform.SetParent(leftHandConstraintRing2Target.transform);

    //    leftHandConstraintThumb1Target.transform.SetParent(directRotationMatchFingersTargetL.transform);
    //    leftHandConstraintThumb2Target.transform.SetParent(leftHandConstraintThumb1Target.transform);
    //    leftHandConstraintThumb3Target.transform.SetParent(leftHandConstraintThumb2Target.transform);

    //    // Right
    //    iKBasedFingersTargetR.transform.SetParent(rightHandIKTarget.transform);
    //    directRotationMatchFingersTargetR.transform.SetParent(rightHandIKTarget.transform);

    //    rightHandIndexIKTarget.transform.SetParent(iKBasedFingersTargetR.transform);
    //    rightHandMiddleIKTarget.transform.SetParent(iKBasedFingersTargetR.transform);
    //    rightHandPinkyIKTarget.transform.SetParent(iKBasedFingersTargetR.transform);
    //    rightHandRingIKTarget.transform.SetParent(iKBasedFingersTargetR.transform);
    //    rightHandThumbIKTarget.transform.SetParent(iKBasedFingersTargetR.transform);

    //    rightHandConstraintIndex1Target.transform.SetParent(directRotationMatchFingersTargetR.transform);
    //    rightHandConstraintIndex2Target.transform.SetParent(rightHandConstraintIndex1Target.transform);
    //    rightHandConstraintIndex3Target.transform.SetParent(rightHandConstraintIndex2Target.transform);

    //    rightHandConstraintMiddle1Target.transform.SetParent(directRotationMatchFingersTargetR.transform);
    //    rightHandConstraintMiddle2Target.transform.SetParent(rightHandConstraintMiddle1Target.transform);
    //    rightHandConstraintMiddle3Target.transform.SetParent(rightHandConstraintMiddle2Target.transform);

    //    rightHandConstraintPinky1Target.transform.SetParent(directRotationMatchFingersTargetR.transform);
    //    rightHandConstraintPinky2Target.transform.SetParent(rightHandConstraintPinky1Target.transform);
    //    rightHandConstraintPinky3Target.transform.SetParent(rightHandConstraintPinky2Target.transform);

    //    rightHandConstraintRing1Target.transform.SetParent(directRotationMatchFingersTargetR.transform);
    //    rightHandConstraintRing2Target.transform.SetParent(rightHandConstraintRing1Target.transform);
    //    rightHandConstraintRing3Target.transform.SetParent(rightHandConstraintRing2Target.transform);

    //    rightHandConstraintThumb1Target.transform.SetParent(directRotationMatchFingersTargetR.transform);
    //    rightHandConstraintThumb2Target.transform.SetParent(rightHandConstraintThumb1Target.transform);
    //    rightHandConstraintThumb3Target.transform.SetParent(rightHandConstraintThumb2Target.transform);

    //    // IK targets
    //    leftHandIKTarget.transform.SetParent(spineFollower.transform);
    //    rightHandIKTarget.transform.SetParent(spineFollower.transform);

    //    // Spine Follower 
    //    spineFollower.transform.SetParent(playerParentGameObject.transform);

    //    centerSpinePos.transform.SetParent(spineFollower.transform);
    //    clipProjectorPos.transform.SetParent(spineFollower.transform);

    //    gunHolderAnimated.transform.SetParent(centerSpinePos.transform);
    //    cameraHolder.transform.SetParent(centerSpinePos.transform);

    //    gunHolder.transform.SetParent(gunHolderAnimated.transform);

    //    clipProjector.transform.SetParent(clipProjectorPos.transform);
    //    interactPointer.transform.SetParent(clipProjectorPos.transform);

    //    visual.transform.SetParent(clipProjector.transform);
    //    visual.SetActive(false);

    //    // Slots/Sockets
    //    slotsSockets.transform.SetParent(playerParentGameObject.transform);

    //    rightHandSlot.transform.SetParent(slotsSockets.transform);
    //    playerBackSlots.transform.SetParent(slotsSockets.transform);
    //    rightUpLegSlot.transform.SetParent(slotsSockets.transform);

    //    rifleSlotOne.transform.SetParent(playerBackSlots.transform);
    //    rifleSlotTwo.transform.SetParent(playerBackSlots.transform);

    //    pistolSlot.transform.SetParent(rightUpLegSlot.transform);
    //    knifeSlotPosition.transform.SetParent(rightUpLegSlot.transform);

    //    knifeSlot.transform.SetParent(knifeSlotPosition.transform);

    //    #endregion

    //    #region Add Components
    //    MultiRotationConstraint spineConstraint = spineRotRig.AddComponent<MultiRotationConstraint>();
    //    MultiRotationConstraint spine1Constraint = spine1RotRig.AddComponent<MultiRotationConstraint>();
    //    MultiRotationConstraint spine2Constraint = spine2RotRig.AddComponent<MultiRotationConstraint>();
    //    MultiRotationConstraint headConstraint = headAimRig.AddComponent<MultiRotationConstraint>();

    //    ConstraintsWeightModifier iKBasedFingersWeightModifier = iKBasedFingersWeight.AddComponent<ConstraintsWeightModifier>();
    //    ConstraintsWeightModifier rotationConstrainedFingersWeightModifier = rotationConstrainedFingersWeight.AddComponent<ConstraintsWeightModifier>();
    //    ConstraintsWeightModifier handsIKWeightModifier = handsIKWeight.AddComponent<ConstraintsWeightModifier>();


    //    MatchRotationToTarget leftHandIndex1Target = leftHandIndex1Constraint.AddComponent<MatchRotationToTarget>();
    //    MatchRotationToTarget leftHandIndex2Target = leftHandIndex2Constraint.AddComponent<MatchRotationToTarget>();
    //    MatchRotationToTarget leftHandIndex3Target = leftHandIndex3Constraint.AddComponent<MatchRotationToTarget>();

    //    MatchRotationToTarget leftHandMiddle1Target = leftHandMiddle1Constraint.AddComponent<MatchRotationToTarget>();
    //    MatchRotationToTarget leftHandMiddle2Target = leftHandMiddle2Constraint.AddComponent<MatchRotationToTarget>();
    //    MatchRotationToTarget leftHandMiddle3Target = leftHandMiddle3Constraint.AddComponent<MatchRotationToTarget>();

    //    MatchRotationToTarget leftHandPinky1Target = leftHandPinky1Constraint.AddComponent<MatchRotationToTarget>();
    //    MatchRotationToTarget leftHandPinky2Target = leftHandPinky2Constraint.AddComponent<MatchRotationToTarget>();
    //    MatchRotationToTarget leftHandPinky3Target = leftHandPinky3Constraint.AddComponent<MatchRotationToTarget>();

    //    MatchRotationToTarget leftHandRing1Target = leftHandRing1Constraint.AddComponent<MatchRotationToTarget>();
    //    MatchRotationToTarget leftHandRing2Target = leftHandRing2Constraint.AddComponent<MatchRotationToTarget>();
    //    MatchRotationToTarget leftHandRing3Target = leftHandRing3Constraint.AddComponent<MatchRotationToTarget>();

    //    MatchRotationToTarget leftHandThumb1Target = leftHandThumb1Constraint.AddComponent<MatchRotationToTarget>();
    //    MatchRotationToTarget leftHandThumb2Target = leftHandThumb2Constraint.AddComponent<MatchRotationToTarget>();
    //    MatchRotationToTarget leftHandThumb3Target = leftHandThumb3Constraint.AddComponent<MatchRotationToTarget>();

    //    MatchRotationToTarget rightHandIndex1Target = rightHandIndex1Constraint.AddComponent<MatchRotationToTarget>();
    //    MatchRotationToTarget rightHandIndex2Target = rightHandIndex2Constraint.AddComponent<MatchRotationToTarget>();
    //    MatchRotationToTarget rightHandIndex3Target = rightHandIndex3Constraint.AddComponent<MatchRotationToTarget>();

    //    MatchRotationToTarget rightHandMiddle1Target = rightHandMiddle1Constraint.AddComponent<MatchRotationToTarget>();
    //    MatchRotationToTarget rightHandMiddle2Target = rightHandMiddle2Constraint.AddComponent<MatchRotationToTarget>();
    //    MatchRotationToTarget rightHandMiddle3Target = rightHandMiddle3Constraint.AddComponent<MatchRotationToTarget>();

    //    MatchRotationToTarget rightHandPinky1Target = rightHandPinky1Constraint.AddComponent<MatchRotationToTarget>();
    //    MatchRotationToTarget rightHandPinky2Target = rightHandPinky2Constraint.AddComponent<MatchRotationToTarget>();
    //    MatchRotationToTarget rightHandPinky3Target = rightHandPinky3Constraint.AddComponent<MatchRotationToTarget>();

    //    MatchRotationToTarget rightHandRing1Target = rightHandRing1Constraint.AddComponent<MatchRotationToTarget>();
    //    MatchRotationToTarget rightHandRing2Target = rightHandRing2Constraint.AddComponent<MatchRotationToTarget>();
    //    MatchRotationToTarget rightHandRing3Target = rightHandRing3Constraint.AddComponent<MatchRotationToTarget>();

    //    MatchRotationToTarget rightHandThumb1Target = rightHandThumb1Constraint.AddComponent<MatchRotationToTarget>();
    //    MatchRotationToTarget rightHandThumb2Target = rightHandThumb2Constraint.AddComponent<MatchRotationToTarget>();
    //    MatchRotationToTarget rightHandThumb3Target = rightHandThumb3Constraint.AddComponent<MatchRotationToTarget>();

    //    TwoBoneIKConstraint leftHandBoneIK = leftHandIK.AddComponent<TwoBoneIKConstraint>();
    //    TwoBoneIKConstraint rightHandBoneIK = rightHandIK.AddComponent<TwoBoneIKConstraint>();

    //    // Left hand IK
    //    TwoBoneIKConstraint leftHandIndexIKConstraint = leftHandIndexIK.AddComponent<TwoBoneIKConstraint>();
    //    TwoBoneIKConstraint leftHandMiddleIKConstraint = leftHandMiddleIK.AddComponent<TwoBoneIKConstraint>();
    //    TwoBoneIKConstraint leftHandPinkyIKConstraint = leftHandPinkyIK.AddComponent<TwoBoneIKConstraint>();
    //    TwoBoneIKConstraint leftHandRingIKConstraint = leftHandRingIK.AddComponent<TwoBoneIKConstraint>();
    //    TwoBoneIKConstraint leftHandThumbIKConstraint = leftHandThumbIK.AddComponent<TwoBoneIKConstraint>();

    //    // Right hand IK
    //    TwoBoneIKConstraint rightHandIndexIKConstraint = rightHandIndexIK.AddComponent<TwoBoneIKConstraint>();
    //    TwoBoneIKConstraint rightHandMiddleIKConstraint = rightHandMiddleIK.AddComponent<TwoBoneIKConstraint>();
    //    TwoBoneIKConstraint rightHandPinkyIKConstraint = rightHandPinkyIK.AddComponent<TwoBoneIKConstraint>();
    //    TwoBoneIKConstraint rightHandRingIKConstraint = rightHandRingIK.AddComponent<TwoBoneIKConstraint>();
    //    TwoBoneIKConstraint rightHandThumbIKConstraint = rightHandThumbIK.AddComponent<TwoBoneIKConstraint>();

    //    // Spine Follower
    //    FollowTransformPosLateUpdate spineFollowerPosLateUpdate = spineFollower.AddComponent<FollowTransformPosLateUpdate>();

    //    GunHolderAnimated gunHolderAnim = gunHolderAnimated.AddComponent<GunHolderAnimated>();
    //    Slot gunHolderSlot = gunHolder.AddComponent<Slot>();

    //    FollowLocalPositionWithAxis interactPointerLocalPosition = interactPointer.AddComponent<FollowLocalPositionWithAxis>();

    //    // Slots/Sockets
    //    FollowTransformPosAndRot rightHandSlotFollow = rightHandSlot.AddComponent<FollowTransformPosAndRot>();
    //    FollowTransformPosAndRot playerBackSlotsFollow = playerBackSlots.AddComponent<FollowTransformPosAndRot>();
    //    FollowTransformPosAndRot rightUpLegSlotFollow = rightUpLegSlot.AddComponent<FollowTransformPosAndRot>();

    //    Slot rifleSlotOneSlot = rifleSlotOne.AddComponent<Slot>();
    //    Slot rifleSlotTwoSlot = rifleSlotTwo.AddComponent<Slot>();

    //    Slot pistolSlotSlot = pistolSlot.AddComponent<Slot>();

    //    Slot knifeSlotSlot = knifeSlot.AddComponent<Slot>();

    //    FollowTransformPosAndRot leftHandIndexIKTargetFollow = leftHandIndexIKTarget.AddComponent<FollowTransformPosAndRot>();
    //    FollowTransformPosAndRot leftHandMiddleIKTargetFollow = leftHandMiddleIKTarget.AddComponent<FollowTransformPosAndRot>();
    //    FollowTransformPosAndRot leftHandPinkyIKTargetFollow = leftHandPinkyIKTarget.AddComponent<FollowTransformPosAndRot>();
    //    FollowTransformPosAndRot leftHandRingIKTargetFollow = leftHandRingIKTarget.AddComponent<FollowTransformPosAndRot>();
    //    FollowTransformPosAndRot leftHandThumbIKTargetFollow = leftHandThumbIKTarget.AddComponent<FollowTransformPosAndRot>();


    //    FollowTransformPosAndRot rightHandIndexIKTargetFollow = rightHandIndexIKTarget.AddComponent<FollowTransformPosAndRot>();
    //    FollowTransformPosAndRot rightHandMiddleIKTargetFollow = rightHandMiddleIKTarget.AddComponent<FollowTransformPosAndRot>();
    //    FollowTransformPosAndRot rightHandPinkyIKTargetFollow = rightHandPinkyIKTarget.AddComponent<FollowTransformPosAndRot>();
    //    FollowTransformPosAndRot rightHandRingIKTargetFollow = rightHandRingIKTarget.AddComponent<FollowTransformPosAndRot>();
    //    FollowTransformPosAndRot rightHandThumbIKTargetFollow = rightHandThumbIKTarget.AddComponent<FollowTransformPosAndRot>();

    //    // Left side target constraint

    //    FollowTransformRot leftHandIndex1ConstraintTargetFollow = leftHandConstraintIndex1Target.AddComponent<FollowTransformRot>();
    //    FollowTransformRot leftHandIndex2ConstraintTargetFollow = leftHandConstraintIndex2Target.AddComponent<FollowTransformRot>();
    //    FollowTransformRot leftHandIndex3ConstraintTargetFollow = leftHandConstraintIndex3Target.AddComponent<FollowTransformRot>();

    //    FollowTransformRot leftHandMiddle1ConstraintTargetFollow = leftHandConstraintMiddle1Target.AddComponent<FollowTransformRot>();
    //    FollowTransformRot leftHandMiddle2ConstraintTargetFollow = leftHandConstraintMiddle2Target.AddComponent<FollowTransformRot>();
    //    FollowTransformRot leftHandMiddle3ConstraintTargetFollow = leftHandConstraintMiddle3Target.AddComponent<FollowTransformRot>();

    //    FollowTransformRot leftHandPinky1ConstraintTargetFollow = leftHandConstraintPinky1Target.AddComponent<FollowTransformRot>();
    //    FollowTransformRot leftHandPinky2ConstraintTargetFollow = leftHandConstraintPinky2Target.AddComponent<FollowTransformRot>();
    //    FollowTransformRot leftHandPinky3ConstraintTargetFollow = leftHandConstraintPinky3Target.AddComponent<FollowTransformRot>();

    //    FollowTransformRot leftHandRing1ConstraintTargetFollow = leftHandConstraintRing1Target.AddComponent<FollowTransformRot>();
    //    FollowTransformRot leftHandRing2ConstraintTargetFollow = leftHandConstraintRing2Target.AddComponent<FollowTransformRot>();
    //    FollowTransformRot leftHandRing3ConstraintTargetFollow = leftHandConstraintRing3Target.AddComponent<FollowTransformRot>();

    //    FollowTransformRot leftHandThumb1ConstraintTargetFollow = leftHandConstraintThumb1Target.AddComponent<FollowTransformRot>();
    //    FollowTransformRot leftHandThumb2ConstraintTargetFollow = leftHandConstraintThumb2Target.AddComponent<FollowTransformRot>();
    //    FollowTransformRot leftHandThumb3ConstraintTargetFollow = leftHandConstraintThumb3Target.AddComponent<FollowTransformRot>();

    //    // Right side target constraint

    //    FollowTransformRot rightHandIndex1ConstraintTargetFollow = rightHandConstraintIndex1Target.AddComponent<FollowTransformRot>();
    //    FollowTransformRot rightHandIndex2ConstraintTargetFollow = rightHandConstraintIndex2Target.AddComponent<FollowTransformRot>();
    //    FollowTransformRot rightHandIndex3ConstraintTargetFollow = rightHandConstraintIndex3Target.AddComponent<FollowTransformRot>();

    //    FollowTransformRot rightHandMiddle1ConstraintTargetFollow = rightHandConstraintMiddle1Target.AddComponent<FollowTransformRot>();
    //    FollowTransformRot rightHandMiddle2ConstraintTargetFollow = rightHandConstraintMiddle2Target.AddComponent<FollowTransformRot>();
    //    FollowTransformRot rightHandMiddle3ConstraintTargetFollow = rightHandConstraintMiddle3Target.AddComponent<FollowTransformRot>();

    //    FollowTransformRot rightHandPinky1ConstraintTargetFollow = rightHandConstraintPinky1Target.AddComponent<FollowTransformRot>();
    //    FollowTransformRot rightHandPinky2ConstraintTargetFollow = rightHandConstraintPinky2Target.AddComponent<FollowTransformRot>();
    //    FollowTransformRot rightHandPinky3ConstraintTargetFollow = rightHandConstraintPinky3Target.AddComponent<FollowTransformRot>();

    //    FollowTransformRot rightHandRing1ConstraintTargetFollow = rightHandConstraintRing1Target.AddComponent<FollowTransformRot>();
    //    FollowTransformRot rightHandRing2ConstraintTargetFollow = rightHandConstraintRing2Target.AddComponent<FollowTransformRot>();
    //    FollowTransformRot rightHandRing3ConstraintTargetFollow = rightHandConstraintRing3Target.AddComponent<FollowTransformRot>();

    //    FollowTransformRot rightHandThumb1ConstraintTargetFollow = rightHandConstraintThumb1Target.AddComponent<FollowTransformRot>();
    //    FollowTransformRot rightHandThumb2ConstraintTargetFollow = rightHandConstraintThumb2Target.AddComponent<FollowTransformRot>();
    //    FollowTransformRot rightHandThumb3ConstraintTargetFollow = rightHandConstraintThumb3Target.AddComponent<FollowTransformRot>();



    //    #endregion

    //    #region Body Constraints
    //    spineConstraint.weight = 0.3f;
    //    spine1Constraint.weight = 0.3f;
    //    spine2Constraint.weight = 1f;
    //    headConstraint.weight = 1f;

    //    headConstraint.data.constrainedZAxis = false;

    //    spineConstraint.data.constrainedObject = playerModelAnimator.GetBoneTransform(HumanBodyBones.Spine);
    //    spine1Constraint.data.constrainedObject = playerModelAnimator.GetBoneTransform(HumanBodyBones.Chest);
    //    spine2Constraint.data.constrainedObject = playerModelAnimator.GetBoneTransform(HumanBodyBones.UpperChest);
    //    headConstraint.data.constrainedObject = playerModelAnimator.GetBoneTransform(HumanBodyBones.Head);

    //    spineConstraint.data.maintainOffset = true;
    //    spine1Constraint.data.maintainOffset = true;
    //    spine2Constraint.data.maintainOffset = true;
    //    headConstraint.data.maintainOffset = true;

    //    spine1Constraint.data.offset = new Vector3(0, 50, 0);
    //    spine2Constraint.data.offset = new Vector3(0, 50, 0);
    //    #endregion

    //    #region Constraints

    //    // Rig
    //    leftHandBoneIK.data.root = playerModelAnimator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
    //    leftHandBoneIK.data.mid = playerModelAnimator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
    //    leftHandBoneIK.data.tip = playerModelAnimator.GetBoneTransform(HumanBodyBones.LeftHand);
    //    leftHandBoneIK.data.target = leftHandIKTarget.transform;


    //    rightHandBoneIK.data.root = playerModelAnimator.GetBoneTransform(HumanBodyBones.RightUpperArm);
    //    rightHandBoneIK.data.mid = playerModelAnimator.GetBoneTransform(HumanBodyBones.RightLowerArm);
    //    rightHandBoneIK.data.tip = playerModelAnimator.GetBoneTransform(HumanBodyBones.RightHand);
    //    rightHandBoneIK.data.target = leftHandIKTarget.transform;


    //    Transform tipLeftIndex = playerModelAnimator.GetBoneTransform(HumanBodyBones.LeftIndexDistal);
    //    leftHandIndexIKConstraint.data.tip = tipLeftIndex;
    //    leftHandIndexIKConstraint.data.mid = tipLeftIndex.parent;
    //    leftHandIndexIKConstraint.data.root = tipLeftIndex.parent.parent;
    //    leftHandIndexIKConstraint.data.target = leftHandIndexIKTarget.transform;

    //    Transform tipLeftMiddle = playerModelAnimator.GetBoneTransform(HumanBodyBones.LeftMiddleDistal);
    //    leftHandMiddleIKConstraint.data.tip = tipLeftMiddle;
    //    leftHandMiddleIKConstraint.data.mid = tipLeftMiddle.parent;
    //    leftHandMiddleIKConstraint.data.root = tipLeftMiddle.parent.parent;
    //    leftHandMiddleIKConstraint.data.target = leftHandMiddleIKTarget.transform;

    //    Transform tipLeftPinky = playerModelAnimator.GetBoneTransform(HumanBodyBones.LeftLittleDistal);
    //    leftHandPinkyIKConstraint.data.tip = tipLeftPinky;
    //    leftHandPinkyIKConstraint.data.mid = tipLeftPinky.parent;
    //    leftHandPinkyIKConstraint.data.root = tipLeftPinky.parent.parent;
    //    leftHandPinkyIKConstraint.data.target = leftHandPinkyIKTarget.transform;

    //    Transform tipLeftRing = playerModelAnimator.GetBoneTransform(HumanBodyBones.LeftRingDistal);
    //    leftHandRingIKConstraint.data.tip = tipLeftRing;
    //    leftHandRingIKConstraint.data.mid = tipLeftRing.parent;
    //    leftHandRingIKConstraint.data.root = tipLeftRing.parent.parent;
    //    leftHandRingIKConstraint.data.target = leftHandRingIKTarget.transform;

    //    Transform tipLeftThumb = playerModelAnimator.GetBoneTransform(HumanBodyBones.LeftThumbDistal);
    //    leftHandThumbIKConstraint.data.tip = tipLeftThumb;
    //    leftHandThumbIKConstraint.data.mid = tipLeftThumb.parent;
    //    leftHandThumbIKConstraint.data.root = tipLeftThumb.parent.parent;
    //    leftHandThumbIKConstraint.data.target = leftHandThumbIKTarget.transform;

    //    // 

    //    Transform tipRightIndex = playerModelAnimator.GetBoneTransform(HumanBodyBones.RightIndexDistal);
    //    rightHandIndexIKConstraint.data.tip = tipRightIndex;
    //    rightHandIndexIKConstraint.data.mid = tipRightIndex.parent;
    //    rightHandIndexIKConstraint.data.root = tipRightIndex.parent.parent;
    //    rightHandIndexIKConstraint.data.target = rightHandIndexIKTarget.transform;

    //    Transform tipRightMiddle = playerModelAnimator.GetBoneTransform(HumanBodyBones.RightMiddleDistal);
    //    rightHandMiddleIKConstraint.data.tip = tipRightMiddle;
    //    rightHandMiddleIKConstraint.data.mid = tipRightMiddle.parent;
    //    rightHandMiddleIKConstraint.data.root = tipRightMiddle.parent.parent;
    //    rightHandMiddleIKConstraint.data.target = rightHandMiddleIKTarget.transform;

    //    Transform tipRightPinky = playerModelAnimator.GetBoneTransform(HumanBodyBones.RightLittleDistal);
    //    rightHandPinkyIKConstraint.data.tip = tipRightPinky;
    //    rightHandPinkyIKConstraint.data.mid = tipRightPinky.parent;
    //    rightHandPinkyIKConstraint.data.root = tipRightPinky.parent.parent;
    //    rightHandPinkyIKConstraint.data.target = rightHandPinkyIKTarget.transform;

    //    Transform tipRightRing = playerModelAnimator.GetBoneTransform(HumanBodyBones.RightRingDistal);
    //    rightHandRingIKConstraint.data.tip = tipRightRing;
    //    rightHandRingIKConstraint.data.mid = tipRightRing.parent;
    //    rightHandRingIKConstraint.data.root = tipRightRing.parent.parent;
    //    rightHandRingIKConstraint.data.target = rightHandRingIKTarget.transform;

    //    Transform tipRightThumb = playerModelAnimator.GetBoneTransform(HumanBodyBones.RightThumbDistal);
    //    rightHandThumbIKConstraint.data.tip = tipRightThumb;
    //    rightHandThumbIKConstraint.data.mid = tipRightThumb.parent;
    //    rightHandThumbIKConstraint.data.root = tipRightThumb.parent.parent;
    //    rightHandThumbIKConstraint.data.target = rightHandThumbIKTarget.transform;

    //    // SpineFollower
    //    spineFollowerPosLateUpdate.Target = playerModelAnimator.GetBoneTransform(HumanBodyBones.UpperChest);

    //    gunHolderSlot.SmoothTime = 0.2f;

    //    // Slots/Sockets
    //    rightHandSlotFollow.Target = playerModelAnimator.GetBoneTransform(HumanBodyBones.RightHand);
    //    playerBackSlotsFollow.Target = playerModelAnimator.GetBoneTransform(HumanBodyBones.UpperChest);

    //    rightUpLegSlotFollow.Target = playerModelAnimator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
    //    rightUpLegSlotFollow.MaintainOffset = true;


    //    rifleSlotOneSlot.SmoothTime = 0.2f;
    //    rifleSlotTwoSlot.SmoothTime = 0.2f;
    //    pistolSlotSlot.SmoothTime = 0.2f;
    //    knifeSlotSlot.SmoothTime = 0.2f;



    //    #endregion

    //    #region Constraints Weight Modifier
    //    SerializedProperty constraintsWeightModifierProperty = GetPropertyFromObj(iKBasedFingersWeightModifier, _iKConstraintsPropertyName);

    //    CreateAndAssignElement(constraintsWeightModifierProperty, leftHandIndexIK);
    //    CreateAndAssignElement(constraintsWeightModifierProperty, leftHandMiddleIK);
    //    CreateAndAssignElement(constraintsWeightModifierProperty, leftHandPinkyIK);
    //    CreateAndAssignElement(constraintsWeightModifierProperty, leftHandRingIK);
    //    CreateAndAssignElement(constraintsWeightModifierProperty, leftHandThumbIK);
    //    CreateAndAssignElement(constraintsWeightModifierProperty, rightHandIndexIK);
    //    CreateAndAssignElement(constraintsWeightModifierProperty, rightHandMiddleIK);
    //    CreateAndAssignElement(constraintsWeightModifierProperty, rightHandPinkyIK);
    //    CreateAndAssignElement(constraintsWeightModifierProperty, rightHandRingIK);
    //    CreateAndAssignElement(constraintsWeightModifierProperty, rightHandThumbIK);

    //    SerializedProperty rotationConstrainedWeight = GetPropertyFromObj(rotationConstrainedFingersWeightModifier, _iKConstraintsPropertyName);



    //    CreateAndAssignElement(rotationConstrainedWeight, leftHandIndex1Constraint);
    //    CreateAndAssignElement(rotationConstrainedWeight, leftHandIndex2Constraint);
    //    CreateAndAssignElement(rotationConstrainedWeight, leftHandIndex3Constraint);

    //    CreateAndAssignElement(rotationConstrainedWeight, leftHandMiddle1Constraint);
    //    CreateAndAssignElement(rotationConstrainedWeight, leftHandMiddle2Constraint);
    //    CreateAndAssignElement(rotationConstrainedWeight, leftHandMiddle3Constraint);

    //    CreateAndAssignElement(rotationConstrainedWeight, leftHandPinky1Constraint);
    //    CreateAndAssignElement(rotationConstrainedWeight, leftHandPinky2Constraint);
    //    CreateAndAssignElement(rotationConstrainedWeight, leftHandPinky3Constraint);

    //    CreateAndAssignElement(rotationConstrainedWeight, leftHandRing1Constraint);
    //    CreateAndAssignElement(rotationConstrainedWeight, leftHandRing2Constraint);
    //    CreateAndAssignElement(rotationConstrainedWeight, leftHandRing3Constraint);

    //    CreateAndAssignElement(rotationConstrainedWeight, leftHandThumb1Constraint);
    //    CreateAndAssignElement(rotationConstrainedWeight, leftHandThumb2Constraint);
    //    CreateAndAssignElement(rotationConstrainedWeight, leftHandThumb3Constraint);

    //    CreateAndAssignElement(rotationConstrainedWeight, rightHandIndex1Constraint);
    //    CreateAndAssignElement(rotationConstrainedWeight, rightHandIndex2Constraint);
    //    CreateAndAssignElement(rotationConstrainedWeight, rightHandIndex3Constraint);

    //    CreateAndAssignElement(rotationConstrainedWeight, rightHandMiddle1Constraint);
    //    CreateAndAssignElement(rotationConstrainedWeight, rightHandMiddle2Constraint);
    //    CreateAndAssignElement(rotationConstrainedWeight, rightHandMiddle3Constraint);

    //    CreateAndAssignElement(rotationConstrainedWeight, rightHandPinky1Constraint);
    //    CreateAndAssignElement(rotationConstrainedWeight, rightHandPinky2Constraint);
    //    CreateAndAssignElement(rotationConstrainedWeight, rightHandPinky3Constraint);

    //    CreateAndAssignElement(rotationConstrainedWeight, rightHandRing1Constraint);
    //    CreateAndAssignElement(rotationConstrainedWeight, rightHandRing2Constraint);
    //    CreateAndAssignElement(rotationConstrainedWeight, rightHandRing3Constraint);

    //    CreateAndAssignElement(rotationConstrainedWeight, rightHandThumb1Constraint);
    //    CreateAndAssignElement(rotationConstrainedWeight, rightHandThumb2Constraint);
    //    CreateAndAssignElement(rotationConstrainedWeight, rightHandThumb3Constraint);

    //    SerializedProperty handsIKModifier = GetPropertyFromObj(handsIKWeightModifier, _iKConstraintsPropertyName);

    //    CreateAndAssignElement(handsIKModifier, leftHandIK);
    //    CreateAndAssignElement(handsIKModifier, rightHandIK);


    //    #endregion

    //    #region player Refs
    //    // Constraints

    //    // Left side constraints
    //    AssignPrivatePropertyObj(rightHandIndex1Target, playerModelAnimator.GetBoneTransform(HumanBodyBones.RightIndexProximal), _constrainedName);
    //    AssignPrivatePropertyObj(rightHandIndex1Target, rightHandConstraintIndex1Target, _toMatchName);

    //    AssignPrivatePropertyObj(rightHandIndex2Target, playerModelAnimator.GetBoneTransform(HumanBodyBones.RightIndexIntermediate), _constrainedName);
    //    AssignPrivatePropertyObj(rightHandIndex2Target, rightHandConstraintIndex2Target, _toMatchName);

    //    AssignPrivatePropertyObj(rightHandIndex3Target, playerModelAnimator.GetBoneTransform(HumanBodyBones.RightIndexDistal), _constrainedName);
    //    AssignPrivatePropertyObj(rightHandIndex3Target, rightHandConstraintIndex3Target, _toMatchName);


    //    AssignPrivatePropertyObj(rightHandMiddle1Target, playerModelAnimator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal), _constrainedName);
    //    AssignPrivatePropertyObj(rightHandMiddle1Target, rightHandConstraintMiddle1Target, _toMatchName);

    //    AssignPrivatePropertyObj(rightHandMiddle2Target, playerModelAnimator.GetBoneTransform(HumanBodyBones.LeftMiddleIntermediate), _constrainedName);
    //    AssignPrivatePropertyObj(rightHandMiddle2Target, rightHandConstraintMiddle2Target, _toMatchName);

    //    AssignPrivatePropertyObj(rightHandMiddle3Target, playerModelAnimator.GetBoneTransform(HumanBodyBones.LeftMiddleDistal), _constrainedName);
    //    AssignPrivatePropertyObj(rightHandMiddle3Target, rightHandConstraintMiddle3Target, _toMatchName);


    //    AssignPrivatePropertyObj(rightHandPinky1Target, playerModelAnimator.GetBoneTransform(HumanBodyBones.LeftLittleProximal), _constrainedName);
    //    AssignPrivatePropertyObj(rightHandPinky1Target, rightHandConstraintPinky1Target, _toMatchName);

    //    AssignPrivatePropertyObj(rightHandPinky2Target, playerModelAnimator.GetBoneTransform(HumanBodyBones.LeftLittleIntermediate), _constrainedName);
    //    AssignPrivatePropertyObj(rightHandPinky2Target, rightHandConstraintIndex2Target, _toMatchName);

    //    AssignPrivatePropertyObj(rightHandPinky3Target, playerModelAnimator.GetBoneTransform(HumanBodyBones.LeftLittleDistal), _constrainedName);
    //    AssignPrivatePropertyObj(rightHandPinky3Target, rightHandConstraintPinky3Target, _toMatchName);


    //    AssignPrivatePropertyObj(rightHandRing1Target, playerModelAnimator.GetBoneTransform(HumanBodyBones.LeftLittleProximal), _constrainedName);
    //    AssignPrivatePropertyObj(rightHandRing1Target, rightHandConstraintRing1Target, _toMatchName);

    //    AssignPrivatePropertyObj(rightHandRing2Target, playerModelAnimator.GetBoneTransform(HumanBodyBones.LeftLittleIntermediate), _constrainedName);
    //    AssignPrivatePropertyObj(rightHandRing2Target, rightHandConstraintRing2Target, _toMatchName);

    //    AssignPrivatePropertyObj(rightHandRing3Target, playerModelAnimator.GetBoneTransform(HumanBodyBones.LeftLittleDistal), _constrainedName);
    //    AssignPrivatePropertyObj(rightHandRing3Target, rightHandConstraintRing3Target, _toMatchName);


    //    AssignPrivatePropertyObj(rightHandThumb1Target, playerModelAnimator.GetBoneTransform(HumanBodyBones.LeftLittleProximal), _constrainedName);
    //    AssignPrivatePropertyObj(rightHandThumb1Target, rightHandConstraintThumb1Target, _toMatchName);

    //    AssignPrivatePropertyObj(rightHandThumb2Target, playerModelAnimator.GetBoneTransform(HumanBodyBones.LeftLittleIntermediate), _constrainedName);
    //    AssignPrivatePropertyObj(rightHandThumb2Target, rightHandConstraintThumb2Target, _toMatchName);

    //    AssignPrivatePropertyObj(rightHandThumb3Target, playerModelAnimator.GetBoneTransform(HumanBodyBones.LeftLittleDistal), _constrainedName);
    //    AssignPrivatePropertyObj(rightHandThumb3Target, rightHandConstraintThumb3Target, _toMatchName);

    //    // Right side Constraints
    //    AssignPrivatePropertyObj(leftHandIndex1Target, playerModelAnimator.GetBoneTransform(HumanBodyBones.RightIndexProximal), _constrainedName);
    //    AssignPrivatePropertyObj(leftHandIndex1Target, leftHandConstraintIndex1Target, _toMatchName);

    //    AssignPrivatePropertyObj(leftHandIndex2Target, playerModelAnimator.GetBoneTransform(HumanBodyBones.RightIndexIntermediate), _constrainedName);
    //    AssignPrivatePropertyObj(leftHandIndex2Target, leftHandConstraintIndex2Target, _toMatchName);

    //    AssignPrivatePropertyObj(leftHandIndex3Target, playerModelAnimator.GetBoneTransform(HumanBodyBones.RightIndexDistal), _constrainedName);
    //    AssignPrivatePropertyObj(leftHandIndex3Target, leftHandConstraintIndex3Target, _toMatchName);


    //    AssignPrivatePropertyObj(leftHandMiddle1Target, playerModelAnimator.GetBoneTransform(HumanBodyBones.RightMiddleProximal), _constrainedName);
    //    AssignPrivatePropertyObj(leftHandMiddle1Target, leftHandConstraintMiddle1Target, _toMatchName);

    //    AssignPrivatePropertyObj(leftHandMiddle2Target, playerModelAnimator.GetBoneTransform(HumanBodyBones.RightMiddleIntermediate), _constrainedName);
    //    AssignPrivatePropertyObj(leftHandMiddle2Target, leftHandConstraintMiddle2Target, _toMatchName);

    //    AssignPrivatePropertyObj(leftHandMiddle3Target, playerModelAnimator.GetBoneTransform(HumanBodyBones.RightMiddleDistal), _constrainedName);
    //    AssignPrivatePropertyObj(leftHandMiddle3Target, leftHandConstraintMiddle3Target, _toMatchName);


    //    AssignPrivatePropertyObj(leftHandPinky1Target, playerModelAnimator.GetBoneTransform(HumanBodyBones.RightLittleProximal), _constrainedName);
    //    AssignPrivatePropertyObj(leftHandPinky1Target, leftHandConstraintPinky1Target, _toMatchName);

    //    AssignPrivatePropertyObj(leftHandPinky2Target, playerModelAnimator.GetBoneTransform(HumanBodyBones.RightLittleIntermediate), _constrainedName);
    //    AssignPrivatePropertyObj(leftHandPinky2Target, leftHandConstraintIndex2Target, _toMatchName);

    //    AssignPrivatePropertyObj(leftHandPinky3Target, playerModelAnimator.GetBoneTransform(HumanBodyBones.RightLittleDistal), _constrainedName);
    //    AssignPrivatePropertyObj(leftHandPinky3Target, leftHandConstraintPinky3Target, _toMatchName);


    //    AssignPrivatePropertyObj(leftHandRing1Target, playerModelAnimator.GetBoneTransform(HumanBodyBones.RightLittleProximal), _constrainedName);
    //    AssignPrivatePropertyObj(leftHandRing1Target, leftHandConstraintRing1Target, _toMatchName);

    //    AssignPrivatePropertyObj(leftHandRing2Target, playerModelAnimator.GetBoneTransform(HumanBodyBones.RightLittleIntermediate), _constrainedName);
    //    AssignPrivatePropertyObj(leftHandRing2Target, leftHandConstraintRing2Target, _toMatchName);

    //    AssignPrivatePropertyObj(leftHandRing3Target, playerModelAnimator.GetBoneTransform(HumanBodyBones.RightLittleDistal), _constrainedName);
    //    AssignPrivatePropertyObj(leftHandRing3Target, leftHandConstraintRing3Target, _toMatchName);


    //    AssignPrivatePropertyObj(leftHandThumb1Target, playerModelAnimator.GetBoneTransform(HumanBodyBones.RightLittleProximal), _constrainedName);
    //    AssignPrivatePropertyObj(leftHandThumb1Target, leftHandConstraintThumb1Target, _toMatchName);

    //    AssignPrivatePropertyObj(leftHandThumb2Target, playerModelAnimator.GetBoneTransform(HumanBodyBones.RightLittleIntermediate), _constrainedName);
    //    AssignPrivatePropertyObj(leftHandThumb2Target, leftHandConstraintThumb2Target, _toMatchName);

    //    AssignPrivatePropertyObj(leftHandThumb3Target, playerModelAnimator.GetBoneTransform(HumanBodyBones.RightLittleDistal), _constrainedName);
    //    AssignPrivatePropertyObj(leftHandThumb3Target, leftHandConstraintThumb3Target, _toMatchName);

    //    // CenterSpinePos
    //    MainBobSettings m_BobSettings = new MainBobSettings();

    //    //m_BobSettings.Controller = controller;

    //    gunHolderAnim.MainBobSettings = m_BobSettings;

    //    // ClipProjectorPos
    //    AssignPrivatePropertyObj(interactPointerLocalPosition, cameraHolder.transform, _targetName);
    //    AssignPrivatePropertyBool(interactPointerLocalPosition, false, _followLPWAyName);
    //    AssignPrivatePropertyBool(interactPointerLocalPosition, false, _followLPWAzName);


    //    AssignPrivatePropertyObj(rig.AddComponent<RigWeightSetter>(), rig.GetComponent<Rig>(), rigWeightSetterRigPropertyName);
    //    #endregion
    //}

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
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        property.objectReferenceValue = objReference;
        serializedObject.ApplyModifiedProperties();
    }

    private void AssignPrivatePropertyObj(SerializedObject serializedObject, SerializedProperty serializedProperty, string relativePropertyName, UnityEngine.Object objReference, Component component = null)
    {
        SerializedProperty relativeProperty = serializedProperty.FindPropertyRelative(relativePropertyName);

        relativeProperty.objectReferenceValue = objReference;

        serializedObject.ApplyModifiedProperties();

        if (component != null) EditorUtility.SetDirty(component);
    }

    void AssignPrivatePropertyBool(Component component, bool boolValue, string propertyName)
    {
        SerializedObject serializedObject = new SerializedObject(component);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        property.boolValue = boolValue;
        serializedObject.ApplyModifiedProperties();
    }

    void AssignPrivatePropertyList(Component component, List<UnityEngine.Object> objectsToAssign, string propertyName)
    {
        SerializedObject serializedObject = new SerializedObject(component);
        SerializedProperty listProperty = serializedObject.FindProperty(propertyName);

        // Clear existing elements
        listProperty.ClearArray();

        // Set the size of the list to match the number of objects
        listProperty.arraySize = objectsToAssign.Count;

        // Assign each element
        for (int i = 0; i < objectsToAssign.Count; i++)
        {
            SerializedProperty elementProperty = listProperty.GetArrayElementAtIndex(i);
            elementProperty.objectReferenceValue = objectsToAssign[i];
        }

        serializedObject.ApplyModifiedProperties();
    }
}
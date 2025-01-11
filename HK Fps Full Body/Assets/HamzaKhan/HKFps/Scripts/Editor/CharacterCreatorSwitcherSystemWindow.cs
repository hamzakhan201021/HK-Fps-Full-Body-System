using UnityEngine.Animations.Rigging;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System;

namespace HKFps
{
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
}
using UnityEngine;
using UnityEditor;

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

    private void SetupCharacter()
    {
        // (MUHAMMAD NOTE) This is how to create a game object
        GameObject playerParentGameObject = new GameObject("HKFpsPlayer");

        // (MUHAMMAD NOTE) This is how we add a component/script to a game object.
        playerParentGameObject.AddComponent<CharacterController>();
        playerParentGameObject.AddComponent<PlayerController>();
        playerParentGameObject.AddComponent<PlayerUI>();

        SerializedObject playerSObject = new SerializedObject(playerParentGameObject.GetComponent<PlayerController>());

        SerializedProperty HandsIkTransformProperty = playerSObject.FindProperty("_handsIKFollowers");

        GameObject lhand = new GameObject("lhand");

        GameObject lhandIndex = new GameObject("lhandIndex");
        GameObject lhandMiddle = new GameObject("lhandMiddle");
        GameObject lhandPinky = new GameObject("lhandPinky");
        GameObject lhandRing = new GameObject("lhandRing");
        GameObject lhandThumb = new GameObject("lhandThumb");

        AssignPrivatePropertyObj(playerSObject, HandsIkTransformProperty, "LeftHandIKTransform", lhand.transform, playerParentGameObject.GetComponent<PlayerController>());

        AssignPrivatePropertyObj(playerSObject, HandsIkTransformProperty, "LeftHandIndexIKTransform", lhandIndex.transform, playerParentGameObject.GetComponent<PlayerController>());
        AssignPrivatePropertyObj(playerSObject, HandsIkTransformProperty, "LeftHandMiddleIKTransform", lhandMiddle.transform, playerParentGameObject.GetComponent<PlayerController>());
        AssignPrivatePropertyObj(playerSObject, HandsIkTransformProperty, "LeftHandPinkyIKTransform", lhandPinky.transform, playerParentGameObject.GetComponent<PlayerController>());
        AssignPrivatePropertyObj(playerSObject, HandsIkTransformProperty, "LeftHandRingIKTransform", lhandRing.transform, playerParentGameObject.GetComponent<PlayerController>());
        AssignPrivatePropertyObj(playerSObject, HandsIkTransformProperty, "LeftHandThumbIKTransform", lhandThumb.transform, playerParentGameObject.GetComponent<PlayerController>());

        if (playerModel != null)
        {
            playerModel.transform.SetParent(playerParentGameObject.transform);
            playerModel.AddComponent<Animator>();
        }
    }

    private void AssignPrivatePropertyObj(SerializedObject serializedObject, SerializedProperty serializedProperty, string relativePropertyName, Object objReference, Component component = null)
    {
        SerializedProperty relativeProperty = serializedProperty.FindPropertyRelative(relativePropertyName);

        relativeProperty.objectReferenceValue = objReference;

        serializedObject.ApplyModifiedProperties();

        if (component != null) EditorUtility.SetDirty(component);
    }
}

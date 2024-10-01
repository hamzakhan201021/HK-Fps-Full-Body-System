using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class HeatEffectsManager : MonoBehaviour
{

    #region Variables

    // General
    [Space]
    [Header("General")]
    [Tooltip("The Weapon Script of the Weapon")]
    [HideInInspector] public WeaponBase Weapon;
    [HideInInspector] [Range(0f, 1f)] public float MaxHeatValueLimit = 1f;
    [HideInInspector] [Range(0f, 1f)] public float MinHeatValueLimit = 0f;

    // Heat Distortion Effect
    [Space]
    [Header("Heat Distortion")]
    [Tooltip("Choose whether you want to use Heat Distortion Effect")]
    [HideInInspector] public bool UseHeatDistortion = true;

    [Tooltip("Heat Distortion Material, This Material will be Instantiated")]
    [HideInInspector] public Material HeatDistortionMaterial;
    [HideInInspector] public Renderer HeatDistortionRenderer;

    [Tooltip("The Name of the Heat Distortion Parameter")]
    [HideInInspector] public string HeatDistortionAmountParameterName = "_Distortion_Amount";

    private int _heatDistortionAmountParameterNameID;

    [Tooltip("heatingAmountMapped is calculated based on the weapon's Heating Settings, " +
        "Current Extra Heat & the min max limits, it is mapped to 0 - 1")]
    private float _heatingAmountMapped;

    // Object Heating Effect
    [Space]
    [Header("Object Heating")]
    [Tooltip("Choose whether you want to use Object Heating Effect")]
    [HideInInspector] public bool UseObjectHeating = true;

    [Tooltip("Heating Gradient, Controls how the Color of the Tip will change when heated")]
    [HideInInspector] public Gradient HeatingGradient;
    [Tooltip("The Glow Intensity Multiplier")]
    [HideInInspector] public float IntensityMulti;

    [Tooltip("All Your Tip Refs")]
    [HideInInspector] public List<WeaponTipHeatData> WeaponTipHeatDatas;

    private Color _heatingColor;

    private float _heatingAmountMappedLerpSpeed = 10;

    // EDITOR SPECIFIC.
    [HideInInspector] public bool[] WeaponTipHeatDatasElementsFoldouts;
    [HideInInspector] public bool WeaponTipHeatDatasDisplayList = false;

    #endregion

    #region General

    // Start is called before the first frame update
    void Start()
    {
        // Initialize
        Initialize();
    }

    /// <summary>
    /// All Initialization Goes in here.
    /// </summary>
    private void Initialize()
    {
        // Instantiate an instance For All...
        for (int i = 0; i < WeaponTipHeatDatas.Count; i++)
        {
            WeaponTipHeatDatas[i].TipRenderer.material = new Material(WeaponTipHeatDatas[i].TipMaterial);
            WeaponTipHeatDatas[i].EmissionColorParameterNameID = Shader.PropertyToID(WeaponTipHeatDatas[i].EmissionColorParameterName);
        }

        // Instantiate an instance of the original material for the distortion Renderer
        HeatDistortionRenderer.material = new Material(HeatDistortionMaterial);

        // Get ID
        _heatDistortionAmountParameterNameID = Shader.PropertyToID(HeatDistortionAmountParameterName);
    }

    // Update is called once per frame
    void Update()
    {
        // Handle Effects
        HandleEffects();
    }

    #endregion

    #region Functions
    
    private void HandleEffects()
    {
        // COMMON (

        // Get the Heat Amount Mapped
        _heatingAmountMapped = Mathf.Lerp(_heatingAmountMapped, Mathf.Lerp(MinHeatValueLimit, MaxHeatValueLimit,
            Weapon.GetCurrentExtraHeat() / Weapon.WeaponData.ExtraOverheatThreshold), _heatingAmountMappedLerpSpeed * Time.deltaTime);

        // )

        // HEAT DISTORTION

        // Check if the user wants to use Heat Distortion Effect, Set material amount..
        if (UseHeatDistortion == true) HeatDistortionRenderer.material.SetFloat(_heatDistortionAmountParameterNameID, _heatingAmountMapped);

        // )

        // OBJECT HEATING

        // Check if the user wants to use Object Heat Effect
        if (UseObjectHeating == true)
        {
            // Get The Color.
            _heatingColor = HeatingGradient.Evaluate(_heatingAmountMapped);

            // Set The Color.
            for (int i = 0; i < WeaponTipHeatDatas.Count; i++)
            {
                WeaponTipHeatDatas[i].TipRenderer.material.SetColor(WeaponTipHeatDatas[i].EmissionColorParameterNameID,
                    _heatingColor * (_heatingAmountMapped * IntensityMulti));
            }
        }
    }

    #endregion
}

#region Custom Editor

#if UNITY_EDITOR

// Custom Editor
[CustomEditor(typeof(HeatEffectsManager))]
public class HeatEffectsManagerEditor : Editor
{

    #region Properties

    // General
    SerializedProperty Weapon;
    SerializedProperty MaxHeatValueLimit;
    SerializedProperty MinHeatValueLimit;

    // Heat Distortion Effect
    SerializedProperty UseHeatDistortion;
    SerializedProperty HeatDistortionMaterial;
    SerializedProperty HeatDistortionRenderer;
    SerializedProperty HeatDistortionAmountParameterName;

    // Object Heating Effect
    SerializedProperty UseObjectHeating;
    SerializedProperty HeatingGradient;
    SerializedProperty IntensityMulti;
    SerializedProperty WeaponTipHeatDatas;

    #endregion

    #region Main

    private void OnEnable()
    {
        // General
        FindProperty(ref Weapon, "Weapon");
        FindProperty(ref MaxHeatValueLimit, "MaxHeatValueLimit");
        FindProperty(ref MinHeatValueLimit, "MinHeatValueLimit");

        // Heat Distortion Effect
        FindProperty(ref UseHeatDistortion, "UseHeatDistortion");
        FindProperty(ref HeatDistortionMaterial, "HeatDistortionMaterial");
        FindProperty(ref HeatDistortionRenderer, "HeatDistortionRenderer");
        FindProperty(ref HeatDistortionAmountParameterName, "HeatDistortionAmountParameterName");

        // Object Heating Effect
        FindProperty(ref UseObjectHeating, "UseObjectHeating");
        FindProperty(ref HeatingGradient, "HeatingGradient");
        FindProperty(ref IntensityMulti, "IntensityMulti");
        FindProperty(ref WeaponTipHeatDatas, "WeaponTipHeatDatas");

        HeatEffectsManager heatEffectsManager = (HeatEffectsManager)target;

        // Initialize the Elements fooldouts bool.
        if (heatEffectsManager.WeaponTipHeatDatasElementsFoldouts.Length == 0)
        {
            heatEffectsManager.WeaponTipHeatDatasElementsFoldouts = new bool[WeaponTipHeatDatas.arraySize];
        }
    }

    public override void OnInspectorGUI()
    {
        // Draw Base Inspector.
        base.OnInspectorGUI();

        // Update
        serializedObject.Update();

        HeatEffectsManager heatEffectsManager = (HeatEffectsManager)target;

        // Draw Weapon Property
        DrawPropertyField(Weapon);

        if (UseHeatDistortion.boolValue == true || UseObjectHeating.boolValue == true)
        {
            EditorGUILayout.Space(3);
            EditorGUI.indentLevel += 1;
            DrawPropertyField(MaxHeatValueLimit);
            DrawPropertyField(MinHeatValueLimit);
            EditorGUI.indentLevel -= 1;
        }

        // Heat Distortion Effect
        DrawPropertyField(UseHeatDistortion);

        if (UseHeatDistortion.boolValue == true)
        {
            EditorGUI.indentLevel += 1;
            DrawPropertyField(HeatDistortionMaterial);
            DrawPropertyField(HeatDistortionRenderer);
            DrawPropertyField(HeatDistortionAmountParameterName);
            EditorGUI.indentLevel -= 1;
        }

        // Object Heating Effect
        DrawPropertyField(UseObjectHeating);

        if (UseObjectHeating.boolValue == true)
        {
            EditorGUI.indentLevel += 1;
            DrawPropertyField(HeatingGradient);
            DrawPropertyField(IntensityMulti);
            EditorGUILayout.Space(2);
            DrawWeaponTipHeatDataList(WeaponTipHeatDatas, "Weapon Tip Heat Datas", ref heatEffectsManager.WeaponTipHeatDatasElementsFoldouts, ref heatEffectsManager.WeaponTipHeatDatasDisplayList);
            EditorGUI.indentLevel -= 1;
        }

        // Apply Modified Properties.
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawWeaponTipHeatDataList(SerializedProperty property, string fieldName, ref bool[] elementsFoldoutsArray, ref bool displayList)
    {
        // Begin Horizontal Drawing.
        EditorGUILayout.BeginHorizontal();

        // Display The Name of the list and the arrow to open/close.
        displayList = EditorGUILayout.Foldout(displayList, fieldName, true);

        // Add a flexible space, this means it will be a space which will resize based on the inspector window.
        GUILayout.FlexibleSpace();

        // Display the size field.
        EditorGUILayout.PropertyField(property.FindPropertyRelative("Array.size"), GUIContent.none);

        // End the horizontal Drawing.
        EditorGUILayout.EndHorizontal();

        // TEXT STYLE.
        GUIStyle textStyle = new GUIStyle();
        textStyle.normal.textColor = Color.white;
        textStyle.fontSize = 20;
        textStyle.fontStyle = FontStyle.Bold;

        // Check if the List is expanded.
        if (displayList)
        {
            // Add an indent
            EditorGUI.indentLevel++;

            // Ensure foldouts array size matches the array size.
            if (elementsFoldoutsArray.Length != property.arraySize)
            {
                elementsFoldoutsArray = new bool[property.arraySize];
            }

            // Draw the background box.
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // For loop for each of the array element.
            for (int i = 0; i < property.arraySize; i++)
            {
                // Add a small space between each element.
                if (i > 0)
                {
                    EditorGUILayout.Space(5);
                }

                // Begin a Horizontal layout for the element and the button.
                EditorGUILayout.BeginHorizontal();

                // Get the Four serialized Properties.
                SerializedProperty element = property.GetArrayElementAtIndex(i);
                SerializedProperty tipRendererProperty = element.FindPropertyRelative("TipRenderer");
                SerializedProperty tipMaterialProperty = element.FindPropertyRelative("TipMaterial");
                SerializedProperty emissionColorParameterNameProperty = element.FindPropertyRelative("EmissionColorParameterName");

                // Draw the foldout
                elementsFoldoutsArray[i] = EditorGUILayout.Foldout(elementsFoldoutsArray[i], $"Element {i}", true);

                // Add a Flexible Space before the button.
                GUILayout.FlexibleSpace();

                // Remove Button for each Element.
                if (GUILayout.Button("-", textStyle))
                {
                    // Delete array element.
                    property.DeleteArrayElementAtIndex(i);
                    break;
                }

                // Space.
                EditorGUILayout.Space(20);

                // End the horizontal layout.
                EditorGUILayout.EndHorizontal();

                // Check if the foldout is expanded.
                if (elementsFoldoutsArray[i])
                {
                    // Draw the properties of this element.
                    EditorGUI.indentLevel++;
                    DrawPropertyField(tipMaterialProperty);
                    DrawPropertyField(tipRendererProperty);
                    DrawPropertyField(emissionColorParameterNameProperty);
                    EditorGUI.indentLevel--;
                }
            }

            // Remove the indent, So we can continue from the original.
            EditorGUI.indentLevel--;

            // Add a Space.
            EditorGUILayout.Space(2);

            // Begin Horizontal Drawing.
            EditorGUILayout.BeginHorizontal();

            // Check if the list has no elements.
            if (property.arraySize == 0)
            {
                // Display a label letting the user know that there aren't any elements.
                EditorGUILayout.LabelField("List is Empty");
            }

            // Flexible Space.
            GUILayout.FlexibleSpace();

            // Add Button(For adding an element)
            if (GUILayout.Button("+", textStyle))
            {
                // Insert a new array element.
                property.InsertArrayElementAtIndex(property.arraySize);
            }

            // Space.
            EditorGUILayout.Space(20);

            // Remove Button for removing an element.
            if (GUILayout.Button("-", textStyle))
            {
                // check if there are elements in the list.
                if (property.arraySize > 0)
                {
                    property.DeleteArrayElementAtIndex(property.arraySize - 1);
                }
            }

            // Space.
            EditorGUILayout.Space(20);

            // End Horizontal.
            EditorGUILayout.EndHorizontal();

            // End the background Box.
            EditorGUILayout.EndVertical();
        }

        // Some Space At the End!.
        EditorGUILayout.Space();
    }

    #endregion

    #region Functions

    private void FindProperty(ref SerializedProperty property, string name)
    {
        // Find Property.
        property = serializedObject.FindProperty(name);
    }

    private void DrawPropertyField(SerializedProperty property)
    {
        // Draw a property Field.
        EditorGUILayout.PropertyField(property);
    }

    #endregion
}

#endif

#endregion
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HKFps
{
    public class WeaponSOEditorWindow : EditorWindow
    {
        private Vector3 positionOffset;

        [MenuItem("Tools/HK Fps/Weapon/Weapon Offset Setter")]
        public static void ShowWindow()
        {
            GetWindow<WeaponSOEditorWindow>("Weapon Offset Setter");
        }

        private void OnGUI()
        {
            GUILayout.Label("Set Weapon Position Offset", EditorStyles.boldLabel);

            // Field to enter the offset value
            positionOffset = EditorGUILayout.Vector3Field("Position Offset", positionOffset);

            if (GUILayout.Button("Set Offset for All Weapons"))
            {
                SetWeaponOffsets();
            }
        }

        private void SetWeaponOffsets()
        {
            // Find all assets of type WeaponSO
            string[] guids = AssetDatabase.FindAssets("t:WeaponSO");
            List<WeaponSO> weaponSOs = new List<WeaponSO>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                WeaponSO weaponSO = AssetDatabase.LoadAssetAtPath<WeaponSO>(path);

                if (weaponSO != null)
                {
                    weaponSOs.Add(weaponSO);
                }
            }

            // Modify the WeaponPositionOffset for each WeaponSO
            foreach (var weaponSO in weaponSOs)
            {
                Undo.RecordObject(weaponSO, "Weapon SO BULK UPDATE");
                //weaponSO.WeaponPositionOffset += positionOffset;
                weaponSO.NewPosition += positionOffset;
                EditorUtility.SetDirty(weaponSO);
            }

            // Save all modified assets
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Updated {weaponSOs.Count} WeaponSO assets with new position offset.");
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class ReverseAnimationClip : ScriptableWizard
{
    public string NewFileName = "";

    [MenuItem("Tools/HK Fps/AnimTools/Reverse Animation Clip...")]
    private static void ReverseAnimationClipWizard()
    {
        DisplayWizard<ReverseAnimationClip>("Reverse Animation Clip...", "Reverse");
    }

    private void OnWizardCreate()
    {
        string directoryPath =
            Path.GetDirectoryName(AssetDatabase.GetAssetPath(Selection.activeObject));
        string fileName =
            Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject));
        string fileExtension =
            Path.GetExtension(AssetDatabase.GetAssetPath(Selection.activeObject));
        fileName = fileName.Split('.')[0];

        string copiedFilePath;
        if (NewFileName != null && NewFileName != "")
        {
            copiedFilePath = directoryPath + Path.DirectorySeparatorChar + NewFileName + fileExtension;
        }
        else
        {
            copiedFilePath = directoryPath + Path.DirectorySeparatorChar + fileName + "_Reversed" + fileExtension;
        }

        AnimationClip originalClip = GetSelectedClip();

        AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(Selection.activeObject), copiedFilePath);

        AnimationClip reversedClip = (AnimationClip)AssetDatabase.LoadAssetAtPath(copiedFilePath, typeof(AnimationClip));

        if (originalClip == null)
        {
            return;
        }

        float clipLength = originalClip.length;
        EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(originalClip);
        Debug.Log(curveBindings.Length);
        reversedClip.ClearCurves();
        foreach(EditorCurveBinding binding in curveBindings)
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(originalClip, binding);
            Keyframe[] keys = curve.keys;
            int keyCount = keys.Length;
            WrapMode postWrapmode = curve.postWrapMode;
            curve.postWrapMode = curve.preWrapMode;
            curve.preWrapMode = postWrapmode;
            for (int i = 0; i < keyCount; i++)
            {
                Keyframe K = keys[i];
                K.time = clipLength - K.time;
                float tmp = -K.inTangent;
                K.inTangent = -K.outTangent;
                keys[i] = K;
            }
            curve.keys = keys;
            reversedClip.SetCurve(binding.path, binding.type, binding.propertyName, curve);

            AnimationEvent[] events = AnimationUtility.GetAnimationEvents(originalClip);
            if (events.Length > 0)
            {
                for (int i = 0; i < events.Length; i++)
                {
                    events[i].time = clipLength - events[i].time;
                }
                AnimationUtility.SetAnimationEvents(reversedClip, events);
            }
        }

        Debug.Log("[[ReverseAnimationClip.cs]] Succesfully reversed" +
                "animation clip " + fileName + ".");
    }

    private AnimationClip GetSelectedClip()
    {
        Object[] clips = Selection.GetFiltered(typeof(AnimationClip), SelectionMode.Assets);
        if (clips.Length > 0)
        {
            return clips[0] as AnimationClip;
        }
        return null;
    }
}

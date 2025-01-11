using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace HKFps
{
    public class BlendTreeCopyPaste : MonoBehaviour
    {
        const string _workDir = "Assets/Editor/AnimationTools/BlendTreeCopyPaste/";
        const string _filename = "btcopy_";
        static int _depth = 0;
        static string _treePath = "";
        static string _log = "";
        static BlendTree _useTree = null;
        public class Pair<T1, T2>
        {
            public T1 First;
            public T2 Second;
            public Pair(T1 first, T2 second)
            {
                First = first;
                Second = second;
            }
        }
        static void makeWorkDirIfDoesntExist()
        {
            string path = "Assets";
            var split = _workDir.Split('/');
            for (int i = 1; i < split.Length; i++)
            {
                string p = split[i];
                string newpath = path + "/" + p;
                if (!AssetDatabase.IsValidFolder(newpath)) AssetDatabase.CreateFolder(path, p);
                path = newpath;
            }
        }
        //========================================================
        static void ClearConsole()
        {
            var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
            var clearMathod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            clearMathod.Invoke(null, null);
        }
        //========================================================
        public static bool isBlendTree(ChildMotion motion)
        {
            try { var treeType = (motion.motion as BlendTree).blendType; }
            catch { return false; }
            return true;
        }
        //========================================================
        public static BlendTree getBlendTreeFromSelection()
        {
            BlendTree bt = _useTree == null ? Selection.activeObject as BlendTree : _useTree;
            if (bt == null) bt = (Selection.activeObject as AnimatorState).motion as BlendTree;
            return bt;
        }
        //========================================================
        public static string getLogPath()
        {
            return Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length) + _workDir + "log.txt";
        }
        //========================================================
        [MenuItem("Tools/HK Fps/AnimTools/Blend Tree/Copy Tree")]
        static void CopyBlendTree()
        {
            int notCopied = 0;
            // Get selected blendtree ...
            BlendTree bt = _useTree == null ? getBlendTreeFromSelection() : _useTree;
            if (bt == null)
            {
                Debug.LogError("BlendTreeCopy - Error: No Selected Blend Tree");
                return;
            }
            // Copy directory ...
            makeWorkDirIfDoesntExist();
            _log = "";
            _depth = -1;
            _treePath = "btcopy_";
            CopyTreeRecursive(bt, 0);
            // Save log
            ClearConsole();
            System.IO.File.WriteAllText(getLogPath(), _log);
            Debug.Log("BlendTree Copied!" + (notCopied > 0 ? " (" + notCopied.ToString() + " child blend trees not copied)" : ""));
        }

        public static void CopyTreeRecursive(BlendTree t, int ichild)
        {
            string oldpath = _treePath;

            _treePath += _depth.ToString() + "," + ichild.ToString() + "_";
            // Save 't's motions ...
            BlendTree tclone = Instantiate<BlendTree>(t);
            string fpath = _workDir + _filename + _depth.ToString() + "," + ichild.ToString() + ".asset";
            AssetDatabase.CreateAsset(tclone, _workDir + _filename + _depth.ToString() + "," + ichild.ToString() + ".asset");
            _log += fpath + "\n";
            // Save children (recursive) ...
            _depth++;
            for (int i = 0; i < t.children.Length; i++)
            {
                if (isBlendTree(t.children[i]))
                {
                    CopyTreeRecursive(t.children[i].motion as BlendTree, i);
                }
            }
            _depth--;
            _treePath = oldpath;
        }
        //========================================================
        [MenuItem("Tools/HK Fps/AnimTools/Blend Tree/Paste")]
        static void PasteBlendTree()
        {
            try
            {
                BlendTree bt = _useTree == null ? getBlendTreeFromSelection() : _useTree;
                var lines = System.IO.File.ReadAllLines(getLogPath());
                List<BlendTree> trees = new List<BlendTree>();
                for (int i = 0; i < lines.Length; i++)
                    trees.Add(AssetDatabase.LoadAssetAtPath<BlendTree>(lines[i]));
                for (int i = 1; i < lines.Length; i++)
                {
                    string l = lines[i].Substring((_workDir + _filename).Length);
                    l = l.Substring(0, l.Length - ".asset".Length);
                    if (l.Length == 0) continue;
                    Debug.Log(l);
                    var split = l.Split(',');
                    int a = int.Parse(split[0]);
                    int b = int.Parse(split[1]);
                    trees[a].children[b].motion = trees[i];
                }
                pasteBlendTreeSettings(bt, trees[0]);
                ClearConsole();
                Debug.Log("BlendTree pasted!");
            }
            catch
            {
                Debug.LogError("BlendTree - Error pasting!");
            }
        }
        public static void pasteBlendTreeSettings(BlendTree bt, BlendTree paste)
        {
            bt.blendType = paste.blendType;
            bt.minThreshold = paste.minThreshold;
            bt.maxThreshold = paste.maxThreshold;
            bt.useAutomaticThresholds = paste.useAutomaticThresholds;
            bt.hideFlags = paste.hideFlags;
            bt.children = paste.children.Clone() as ChildMotion[];
            bt.blendParameter = paste.blendParameter;
            bt.blendParameterY = paste.blendParameterY;
        }
        //========================================================
        [MenuItem("Tools/HK Fps/AnimTools/Blend Tree/Delete Trees")]
        public static void deleteBlendTrees()
        {
            List<BlendTree> bts = getBlendTreesFromSelection();

            foreach (BlendTree bt in bts)
            {
                DestroyImmediate(bt, true);
            }
        }
        //========================================================
        public static List<BlendTree> getBlendTreesFromSelection()
        {
            List<BlendTree> blendTrees = new List<BlendTree>();

            foreach (Object obj in Selection.objects)
            {
                BlendTree bt = obj as BlendTree;
                if (bt == null)
                {
                    AnimatorState state = obj as AnimatorState;
                    if (state != null)
                    {
                        bt = state.motion as BlendTree;
                    }
                }
                if (bt != null)
                {
                    blendTrees.Add(bt);
                }
            }

            return blendTrees;
        }
    }
}
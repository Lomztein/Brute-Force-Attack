using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

public class BuildToAll : EditorWindow {

    public List<string> scenes = new List<string>();
    public List<BuildTarget> targets = new List<BuildTarget>();
    public List<string> targetExtensions = new List<string>();

    public string buildName;

    [MenuItem ("Project Virus/Build Multiple")]
    public static void ShowWindow () {
        EditorWindow.GetWindow (typeof (BuildToAll));
    }

    void OnGUI () {
        buildName = EditorGUILayout.TextField ("Name ", buildName);
        for (int s = 0; s < scenes.Count; s++) {
            scenes[s] = EditorGUILayout.TextField (scenes[s]);
        }

        if (GUILayout.Button ("Add Scene"))
            scenes.Add ("");
        if (GUILayout.Button ("Remove Scene"))
            scenes.RemoveAt (scenes.Count - 1);
        for (int t = 0; t < targets.Count; t++) {
            targets[t] = (BuildTarget)EditorGUILayout.EnumPopup (targets[t]);
            if (targetExtensions.Count - 1 >= t) targetExtensions[t] = EditorGUILayout.TextField (targetExtensions[t]);
        }

        if (GUILayout.Button ("Add Target")) {
            targetExtensions.Add (".exe");
            targets.Add (BuildTarget.StandaloneWindows);
        }
        if (GUILayout.Button ("Remove Target")) {
            targets.RemoveAt (targets.Count - 1);
            if (targetExtensions.Count > targets.Count)
                targetExtensions.RemoveAt (targets.Count - 1);
        }
        if (GUILayout.Button ("Build All"))
            Build ();
    }

    string[] GetScenes () {
        string[] s = new string[scenes.Count];
        for (int i = 0; i < scenes.Count; i++) {
            s[i] = "Assets\\Scenes\\" + scenes[i] + ".unity";
        }
        return s;
    }

    void Build () {
        for (int i = 0; i < targets.Count; i++) {
            BuildPipeline.BuildPlayer (GetScenes (), "Compiled\\" + targets[i].ToString () + "\\" + buildName + targetExtensions[i],
                           targets[i], BuildOptions.None);
        }
    }
}

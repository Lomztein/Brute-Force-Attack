﻿using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;

public class BuildToAll : EditorWindow {

    public List<string> scenes = new List<string>();
    public List<BuildTarget> targets = new List<BuildTarget>();
    public List<string> targetExtensions = new List<string>();

    public string buildName;
    public string[] debugExtensions = new string[] { ".pdb" };
    private string sevenZipPath = "D:/Program Files/7-Zip/7zG.exe";

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
            s[i] = "Assets/Scenes/" + scenes[i] + ".unity";
        }
        return s;
    }

    void Build () {
        for (int i = 0; i < targets.Count; i++) {
            string path = Application.dataPath.Substring (0, Application.dataPath.LastIndexOf ('/'));
            string buildPath = path + "/Compiled/" + targets[i].ToString ();
            Directory.CreateDirectory (buildPath);
            BuildPipeline.BuildPlayer (GetScenes (), buildPath + "/" + buildName + targetExtensions[i],
                           targets[i], BuildOptions.None);

            string[] debugFiles = Directory.GetFiles (buildPath);
            foreach (string file in debugFiles) {
                if (debugExtensions.Contains (file.Substring (file.LastIndexOf ('.')))) {
                    File.Delete (file);
                }
            }

            CreateZip (buildPath, buildPath + ".zip");
            Directory.Delete (buildPath, true);
        }
    }

    public void CreateZip (string sourceName, string targetName) {
        ProcessStartInfo p = new ProcessStartInfo ();
        p.FileName = @sevenZipPath;
        p.Arguments = "a -tzip \"" + targetName + "\" \"" + sourceName + "\" -mx=9";
        p.WindowStyle = ProcessWindowStyle.Normal;
        Process x = Process.Start (p);
        x.WaitForExit ();
    }
}
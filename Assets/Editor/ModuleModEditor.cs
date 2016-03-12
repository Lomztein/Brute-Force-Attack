using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

public class ModuleModEditor : EditorWindow {

	private enum WindowStatus { Clone, Edit }
	private WindowStatus windowStatus;

    public Module module;
    public List<Module> toClone = new List<Module> ();

	[MenuItem ("Project Virus/Module Mod Editor")]
	public static void ShowWindow () {
		EditorWindow.GetWindow (typeof(ModuleModEditor));
	}

	void OnGUI () {
        module = (Module)EditorGUILayout.ObjectField (module, typeof (Module), false);
        for (int i = 0; i < toClone.Count; i++) {
            toClone[i] = (Module)EditorGUILayout.ObjectField (toClone[i], typeof (Module), false);
        }

        EditorGUILayout.Separator ();

        if (GUILayout.Button ("Add module"))
            toClone.Add (null);
        if (GUILayout.Button ("Remove module"))
            toClone.RemoveAt (toClone.Count - 1);

        EditorGUILayout.Separator ();

        if (GUILayout.Button ("Clone mods")) {
            for (int i = 0; i < toClone.Count; i++) {
                toClone[i].moduleMods = module.moduleMods;
            }
        }
    }
}

using UnityEngine;
using System.Collections;
using UnityEditor;

public class ResearchEditor : EditorWindow {

	private ResearchMenu research;

	public float zoomLevel;
	public Vector2 offset;

	private enum Action { Default, ChoosingPrerequisite };
	private enum WindowState { All, Upgrade };

	private Action action = Action.Default;
	private WindowState state = WindowState.All;

	private Upgrade focusUpgrade;

	[MenuItem ("Project Virus/Research")]
	public static void ShowWindow () {
		EditorWindow.GetWindow (typeof (ResearchEditor));
	}

	void OnGUI () {
		if (!research)
			research = GameObject.Find ("ResearchMenu").GetComponent<ResearchMenu>();

		if (state == WindowState.All) {
			offset.y = GUI.VerticalSlider (new Rect (position.width - 15, 5, 10, position.height - 10), offset.y, position.height, 0);
			if (GUI.Button (new Rect (position.width / 3, position.height - 25, position.width / 3, 20), "Add upgrade"))
				research.upgrades.Add (new Upgrade());
			
			for (int i = 0; i < research.upgrades.Count; i++) {
				
				Upgrade u = research.upgrades[i];
				Vector3 pos = research.GetPos (u);

				Rect rect = new Rect (pos.x * 30 - offset.x + position.width/2, pos.y * 30 - offset.y + 10f, 20, 20);

				if (u.upgradeSprite) {
					if (GUI.Button (rect, u.upgradeSprite.texture)) {
						SelectUpgrade (u, i);
					}
				}else{
					if (GUI.Button (rect, "")) {
						SelectUpgrade (u, i);
					}
				}
			}

			for (int i = (int)(offset.y / 30); i < (int)(position.height / 30) + (int)(offset.y / 30); i++) {
				GUI.Label (new Rect (10, i * 30 + 10 - offset.y, 20, 20), i.ToString ()); 
			}

			research.ResetDictionary ();
		}

		if (state == WindowState.Upgrade) {
			focusUpgrade.upgradeName = EditorGUILayout.TextField ("Name: ",focusUpgrade.upgradeName);
			focusUpgrade.upgradeFunction = EditorGUILayout.TextField ("Function: ",focusUpgrade.upgradeFunction);
			focusUpgrade.upgradeCost = EditorGUILayout.IntField ("Cost: ",focusUpgrade.upgradeCost);
			focusUpgrade.upgradeSprite = (Sprite)EditorGUILayout.ObjectField ("Sprite: ",focusUpgrade.upgradeSprite, typeof (Sprite), false);
			if (focusUpgrade.prerequisiteID >= 0) {
				if (GUILayout.Button ("Remove Prerequisite: " + research.upgrades[focusUpgrade.prerequisiteID].upgradeName)) {
					Debug.Log ("Removing prerequisitite");
					focusUpgrade.prerequisiteID = -1;
				}
			}else{
				if (GUILayout.Button ("Add Prerequisite")) {
					state = WindowState.All;
					action = Action.ChoosingPrerequisite;
				}
			}

			if (GUILayout.Button ("Delete")) {
				research.upgrades.Remove (focusUpgrade);
				state = WindowState.All;
				focusUpgrade = null;
			}

			if (GUILayout.Button ("Return")) {
				state = WindowState.All;
				focusUpgrade = null;
			}
		}
	}

	void SelectUpgrade (Upgrade u, int index) {
		if (action == Action.Default)
			focusUpgrade = u;

		if (action == Action.ChoosingPrerequisite) {
			focusUpgrade.prerequisiteID = index;
			action = Action.Default;
		}

		state = WindowState.Upgrade;
	}
}

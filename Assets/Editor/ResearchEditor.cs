using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

public class ResearchEditor : EditorWindow {

	private ResearchMenu research;
	public Vector2 offset;

	private enum Action { Default, ChoosingPrerequisite, MovingResearch, CloningResearch };
	private enum WindowState { All, Research };

	private Action action = Action.Default;
	private WindowState state = WindowState.All;

	private Research focusResearch;

	[MenuItem ("Project Virus/Research")]
	public static void ShowWindow () {
		EditorWindow.GetWindow (typeof (ResearchEditor));
	}

	void DrawResearchButton (Vector3 pos, Research u) {
		Rect rect = new Rect (pos.x * 30 - offset.x + position.width/2, pos.y * 30 - offset.y + 10f, 20, 20);

		if (u != null) {
			if (u.sprite) {
				if (GUI.Button (rect, u.sprite.texture)) {
					SelectResearch (u);
				}
			}else{
				if (GUI.Button (rect, "")) {
					SelectResearch (u);
				}
			}

			if (u.prerequisite != -1) {
				Vector2 o = new Vector2 (position.width/2 + 10 + (u.x - u.GetPrerequisite ().x) * 15, 20);
				Drawing.DrawLine (
					new Vector2 (u.x, u.y) * 30 - offset + o
				  , new Vector2 (u.GetPrerequisite ().x, u.GetPrerequisite().y) * 30 - offset + o
				  , Color.black, 2, false);
			}
		}else{
			if (GUI.Button (rect, "")) {
				SelectEmptyButton (Mathf.RoundToInt (pos.x), Mathf.RoundToInt (pos.y));
			}
		}
	}

	void SelectEmptyButton (int x, int y) {
		if (action == Action.MovingResearch) {
			focusResearch.x = x;
			focusResearch.y = y;
		}

		if (action == Action.CloningResearch) {
            Research r = new Research ();
			r.x = x;
			r.y = y;
			r.colour = focusResearch.colour;
			r.desc = focusResearch.desc;
			r.name = focusResearch.name;
			r.func = focusResearch.func;
			r.meta = focusResearch.meta;
			r.sprite = focusResearch.sprite;
            r.index = research.research.Count;
            r.prerequisite = -1;
			research.research.Add (r);
		}

		action = Action.Default;
	}
	
	void OnGUI () {
        GUI.SetNextControlName("DUMMY");
        GUI.Label(new Rect(-100, -100, 1, 1), "DUMMY");

        if (!research) {
			GameObject r = GameObject.Find ("ResearchMenu");
			if (r) research = r.GetComponent<ResearchMenu>();
		}else{

			if (state == WindowState.All) {
				if (action != Action.MovingResearch) {

					offset.y = GUI.VerticalSlider (new Rect (position.width - 15, 5, 10, position.height - 10), offset.y, position.height, 0);
                    if (GUI.Button (new Rect (position.width / 3, position.height - 50, position.width / 3, 20), "Suggest All Names")) {
                        for (int i = 0; i < research.research.Count; i++) {

                            string n;
                            string d;

                            SuggestTitle (research.research[i], out n, out d);
                            research.research[i].name = n;
                            research.research[i].desc = d;

                        }
                    }

                    if (GUI.Button (new Rect (position.width / 3, position.height - 25, position.width / 3, 20), "Add Research")) {
						research.research.Add (new Research ());
                        SelectResearch (research.research[research.research.Count-1]);
                        research.research[research.research.Count - 1].index = research.research.Count - 1;
                        research.research[research.research.Count - 1].prerequisite = -1;
                        state = WindowState.All;
                        action = Action.MovingResearch;
					}
					
					for (int i = 0; i < research.research.Count; i++) {
						
						Research u = research.research[i];
						Vector3 pos = research.GetPos (u);

						DrawResearchButton (pos, u);
					}

					for (int i = (int)(offset.y / 30); i < (int)(position.height / 30) + (int)(offset.y / 30); i++) {
						GUI.Label (new Rect (10, i * 30 + 10 - offset.y, 20, 20), i.ToString ()); 
					}
				}

				if (action == Action.MovingResearch || action == Action.CloningResearch) {

					// Dis part gunna be heavy and hacky.
					// I'm truely sorry you have to see this.

					// Create and populate unfound research.
					List<Research> unfound = new List<Research>();
					foreach (Research r in research.research) {
						unfound.Add (r);
					}

					// Foreach loops are major simple to work with, yay! :D

					for (int y = (int)(offset.y / 30); y < (int)(position.height / 30) + (int)(offset.y / 30); y++) {
						for (int x = (int)((offset.x - position.width/2) / 30); x < (int)((position.width/2) / 30); x++) {

							// Figure out if there is research at current position, search in unfound.
							Research atPos = null;
							foreach (Research r in unfound) {
								if (r.x == x && r.y == y) {
									atPos = r;
								}
							}

							if (atPos != null) {
								DrawResearchButton (new Vector3 (x,y), atPos);
								unfound.Remove (atPos);
							}else{
								DrawResearchButton (new Vector3 (x,y), null);
							}
						}
					}
				}
					
			}

			if (state == WindowState.Research) {
				if (focusResearch == null) {
					state = WindowState.All;
					action = Action.Default;
				}
				focusResearch.name = EditorGUILayout.TextField ("Name: ",focusResearch.name);
				focusResearch.desc = EditorGUILayout.TextArea (focusResearch.desc);
				focusResearch.func = EditorGUILayout.TextField ("Function: ",focusResearch.func);
				focusResearch.meta = EditorGUILayout.TextField ("Meta: ",focusResearch.meta);
				focusResearch.sprite = (Sprite)EditorGUILayout.ObjectField ("Sprite: ",focusResearch.sprite, typeof (Sprite), false);
				focusResearch.colour = (Colour)EditorGUILayout.EnumPopup ("Colour: ", focusResearch.colour);
				if (focusResearch.prerequisite != -1) {
					if (GUILayout.Button ("Remove Prerequisite: " + focusResearch.GetPrerequisite ().name)) {
						Debug.Log ("Removing Prerequisitite");
						focusResearch.prerequisite = -1;
					}
				}else{
					if (GUILayout.Button ("Add Prerequisite")) {
						state = WindowState.All;
						action = Action.ChoosingPrerequisite;
					}
				}

				if (GUILayout.Button ("Move")) {
					state = WindowState.All;
					action = Action.MovingResearch;
				}

				if (GUILayout.Button ("Clone")) {
					state = WindowState.All;
					action = Action.CloningResearch;
				}

				if (GUILayout.Button ("Delete")) {
					research.research.Remove (focusResearch);
					state = WindowState.All;
					action = Action.Default;
					focusResearch = null;
				}

                if (GUILayout.Button ("Suggest Name")) {
                    string n;
                    string d;

                    SuggestTitle (focusResearch, out n, out d);
                    focusResearch.name = n;
                    focusResearch.desc = d;
                }

				if (GUILayout.Button ("Return")) {
					state = WindowState.All;
					action = Action.Default;
					focusResearch = null;
				}
			}
		}
	}

    private void SuggestTitle (Research r, out string title, out string desc) {
        switch (r.func) {
            case "UnlockModule":
                title = "Research the " + research.unlockableModules[int.Parse(r.meta)].GetComponent<Module> ().moduleName + " module";
                desc = research.unlockableModules[int.Parse (r.meta)].GetComponent<Module> ().moduleDesc;
                break;

            case "UnlockSpecialModule":
                title = "Research the specialized " + research.unlockableModules[int.Parse(r.meta)].GetComponent<Module> ().moduleName + " module";
                desc = research.unlockableModules[int.Parse (r.meta)].GetComponent<Module> ().moduleDesc;
                break;

            case "IncreaseFirerate":
                title = "Increase " + r.colour.ToString ().ToLower () + " firerate by " + r.meta + "%";
                desc = "Increase the firerate of " + r.colour.ToString ().ToLower () + " weapons by " + r.meta + "%";
                break;
            case "IncreaseDamage":
                title = "Increase " + r.colour.ToString ().ToLower () + " damage by " + r.meta + "%";
                desc = "Increase the damage of " + r.colour.ToString ().ToLower () + " weapons by " + r.meta + "%";
                break;
            case "DecreaseCost":
                title = "Decrease " + r.colour.ToString ().ToLower () + " cost by " + r.meta + "%";
                desc = "Decrease the cost of " + r.colour.ToString ().ToLower () + " modules by " + r.meta + "%";
                break;
            case "IncreaseRange":
                title = "Increase base range by " + r.meta + "%";
                desc = "Increase the range of base modules by " + r.meta + "%";
                break;
            case "IncreaseTurnrate":
                title = "Increase rotator turnrate by " + r.meta + "%";
                desc = "Increase the turnrate of rotator modules by " + r.meta + "%";
                break;
            default:
                title = r.name;
                desc = r.desc;
                break;
        }
    }

	void SelectResearch (Research u) {
        GUI.FocusControl("DUMMY");
		if (action == Action.Default)
            focusResearch = u;

		if (action == Action.ChoosingPrerequisite) {
			focusResearch.prerequisite = u.index;
			action = Action.Default;
		}

		state = WindowState.Research;
	}
}

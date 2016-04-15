using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditorInternal;
using System.Collections.Generic;

namespace Gameotiv.ToDoList
{
    public class ToDoList : EditorWindow
    {
        //The window for further use.
        static ToDoList window;

        //A reference to the to-do list object that should be in the resources folder.
        ToDoListObject list;

		//Scroll position for the to-do list.
        Vector2 todoListScroll;
		//Scroll position for group editing.
        Vector2 groupEditingScroll;
		//Scroll position for the preferences.
        Vector2 preferencesScroll;
		//A simple call to easily get the window size.
        Vector2 size { get { if (window) { return window.position.size; } return Vector2.zero; } }

		//Anim bool to show the to-do list view.
        AnimBool showGroups = new AnimBool(true);
		//Anim bool to show the group editing view.
        AnimBool showEditingGroups = new AnimBool(false);
		//Anim bool to show the preferences view.
        AnimBool showPreferences = new AnimBool(false);

		//A reoderable list for the group editing.
        ReorderableList editingList;

		//Create the window.
        [MenuItem("Tools/To-Do List")]
        static void Init()
        {
			//Create an instance of the window.
            window = CreateInstance<ToDoList>();
            //Name the window.
            window.titleContent = new GUIContent("To-Do List");
			//Show the window.
            window.Show();
        }

        void OnEnable()
        {
			//Set the window to this. This is so the code can recompile and the window doesn't become null.
            window = this;

			//Find the to-do list in the resources folder.
            FindList();

			//Add the repaint listeners to the anim bools.
            showGroups.valueChanged.AddListener(Repaint);
            showEditingGroups.valueChanged.AddListener(Repaint);
            showPreferences.valueChanged.AddListener(Repaint);

			//Update anim bools for groups and tasks.
            UpdateAnimBools();
			//Create the group editing list.
            MakeEditingList();
        }

        void MakeEditingList()
        {
			//Make sure the list is set to avoid any errors.
            if (list)
            {
				//Create a new reoderable list.
                editingList = new ReorderableList(list.priorityGroups, typeof(PriorityGroup), true, true, true, true);
                editingList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
					//Make the preview label. This is used so the user can see what the text color will look like on the groups.
                    GUIStyle previewLabel = new GUIStyle(GUI.skin.button);
                    previewLabel.normal.textColor = list.priorityGroups[index].groupColor;
                    previewLabel.hover.textColor = list.priorityGroups[index].groupColor;
                    previewLabel.active.textColor = list.priorityGroups[index].groupColor;
                    list.priorityGroups[index].groupName = EditorGUI.TextField(new Rect(rect.x, rect.y, rect.width / 2 - 40, EditorGUIUtility.singleLineHeight), list.priorityGroups[index].groupName);
                    list.priorityGroups[index].groupColor = EditorGUI.ColorField(new Rect(rect.x + rect.width / 2 - 35, rect.y, rect.width / 2 - 120, EditorGUIUtility.singleLineHeight), list.priorityGroups[index].groupColor);
					GUI.Button(new Rect(rect.x + rect.width - 150, rect.y, 70, EditorGUIUtility.singleLineHeight), "Preview", previewLabel);
                    list.priorityGroups[index].labelStyle = (FontStyle)EditorGUI.EnumPopup(new Rect(rect.x + rect.width - 70, rect.y, 70, EditorGUIUtility.singleLineHeight), list.priorityGroups[index].labelStyle);
                };
                editingList.drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, "Priority Groups");
                };
                editingList.onAddCallback = (ReorderableList l) =>
                {
                    PriorityGroup newGroup = new PriorityGroup();
                    newGroup.groupName = "New Group";
                    newGroup.groupColor = GUI.skin.label.normal.textColor;
                    newGroup.labelStyle = FontStyle.Normal;
                    list.priorityGroups.Add(newGroup);
                };
                editingList.onRemoveCallback = (ReorderableList l) =>
                {
                    if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete this group? ALL the tasks will also be deleted. This action CAN NOT BE UNDONE!", "Yes", "No"))
                    {
                        ReorderableList.defaultBehaviours.DoRemoveButton(l);
                    }
                };
            }
        }

        void UpdateAnimBools()
        {
            //Find the list to make sure we got it.
            FindList();
            if (list)
            {
                foreach (PriorityGroup group in list.priorityGroups)
                {
                    group.expanded.valueChanged.AddListener(Repaint);

                    foreach(Task task in group.tasks)
                    {
                        task.showDetails.valueChanged.AddListener(Repaint);
                    }
                }
            }
        }

        void FindList()
        {
            list = Resources.Load("Editor/ToDoList") as ToDoListObject;
        }

        void PriorityGroupButton(PriorityGroup group)
        {
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.normal.textColor = group.groupColor;
            buttonStyle.hover.textColor = group.groupColor;
            buttonStyle.active.textColor = group.groupColor;
            buttonStyle.alignment = TextAnchor.MiddleLeft;
            buttonStyle.fontStyle = group.labelStyle;

            string buttonText = group.groupName;

            if (list.showButtonPrefix)
            {
                buttonText = group.expanded.target ? "Collapse " + group.groupName : "Expand " + group.groupName;
            }

            if (GUILayout.Button(buttonText, buttonStyle, GUILayout.Width(size.x - 10), GUILayout.Height(30)))
            {
                group.expanded.target = !group.expanded.target;
            }
        }

        void ShowGroup(PriorityGroup group)
        {
            GUIStyle detailsStyle = new GUIStyle(GUI.skin.textArea);
            detailsStyle.wordWrap = true;

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("Task Name");
            group.newTaskField = EditorGUILayout.TextField(group.newTaskField);
            EditorGUILayout.LabelField("Task Details - Optional");
            group.newTaskDetails = EditorGUILayout.TextArea(group.newTaskDetails, detailsStyle);
            GUI.enabled = group.newTaskField != "";

            GUI.SetNextControlName("AddTaskButton");

            if (GUILayout.Button("Add new task to " + group.groupName))
            {
                Task task = new Task();
                task.taskName = group.newTaskField;
                task.taskDetails = group.newTaskDetails;
                group.tasks.Add(task);
                group.newTaskField = "";
                group.newTaskDetails = "";
                GUI.FocusControl("AddTaskButton");
                UpdateAnimBools();
            }

            GUI.enabled = true;

            if(group.tasks.Count >= 1)
            {
                EditorGUILayout.Space();
            }

            for (int i = 0; i < group.tasks.Count; i++)
            {
                if (list.showFinishedTasks)
                {
                    ShowTask(group.tasks[i], group, i);
                }
                else
                {
                    if (!group.tasks[i].done)
                    {
                        ShowTask(group.tasks[i], group, i);
                    }
                }
            }

            EditorGUILayout.EndVertical();
        }

        void ShowTask(Task task, PriorityGroup group, int index)
        {
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.wordWrap = true;

            if (task.done) { GUI.color = Color.green; } else { GUI.color = Color.white; }
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginHorizontal();
            GUI.color = Color.white;
            if (task.editing)
            {
                task.taskName = EditorGUILayout.TextField(task.taskName);
            }
            else
            {
                EditorGUILayout.LabelField(task.taskName);
            }

            if (task.editing) { GUI.color = Color.green; } else { GUI.color = Color.white; }

            if (GUILayout.Button(task.editing ? "Finish Editing" : "Edit", task.editing ? GUILayout.Width(120) : GUILayout.Width(40)))
            {
                task.editing = !task.editing;
            }

            GUI.color = Color.white;

            GUI.enabled = !task.editing;

            if (GUILayout.Button(task.done ? "Not done" : "Done", GUILayout.Width(80)))
            {
                task.done = !task.done;
            }

            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                if (list.displayWarningBeforeTaskRemoval)
                {
                    if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete this task?", "Yes", "No"))
                    {
                        group.tasks.RemoveAt(index);
                    }
                }
                else
                {
                    group.tasks.RemoveAt(index);
                }
            }

            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            if(task.taskDetails != "")
            {
                task.showDetails.target = GUILayout.Toggle(task.showDetails.target, "More Details", "Foldout");

                if (EditorGUILayout.BeginFadeGroup(task.showDetails.faded))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField(task.taskDetails, labelStyle);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndFadeGroup();
            }

            EditorGUILayout.EndVertical();
        }

        void ShowEditingGroup(PriorityGroup group, int index)
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Group Name", GUILayout.Width(150));
            group.groupName = EditorGUILayout.TextField(group.groupName);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Group Color", GUILayout.Width(150));
            group.groupColor = EditorGUILayout.ColorField(group.groupColor);
            EditorGUILayout.EndHorizontal();
            GUI.color = Color.red;
            if (GUILayout.Button("Delete " + group.groupName))
            {
                if (EditorUtility.DisplayDialog("Delete Confirmation", "Are you sure you want to delete " + group.groupName + " with all it's tasks?", "Yes", "No"))
                {
                    list.priorityGroups.RemoveAt(index);
                    FindList();
                }
            }
            GUI.color = Color.white;
            EditorGUILayout.EndVertical();
        }

        void ShowToDoList()
        {
            EditorGUILayout.LabelField("To-Do List", EditorStyles.boldLabel);

            GUIStyle noScroll = new GUIStyle(GUI.skin.horizontalScrollbar);
            noScroll = GUIStyle.none;

            todoListScroll = EditorGUILayout.BeginScrollView(todoListScroll, false, false, noScroll, GUI.skin.verticalScrollbar, GUIStyle.none, GUILayout.Height(size.y - 45));

            foreach (PriorityGroup group in list.priorityGroups)
            {
                PriorityGroupButton(group);

                if (EditorGUILayout.BeginFadeGroup(group.expanded.faded))
                {
                    ShowGroup(group);
                }
                EditorGUILayout.EndFadeGroup();
            }

            EditorGUILayout.EndScrollView();

            GUILayout.BeginArea(new Rect(0, size.y - 25, size.x, 25), "", GUI.skin.box);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Edit groups"))
            {
                showGroups.target = false;
                showEditingGroups.target = true;
                showPreferences.target = false;
            }

            if (GUILayout.Button("Preferences"))
            {
                showGroups.target = false;
                showEditingGroups.target = false;
                showPreferences.target = true;
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        void ShowGroupEditing()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("Editing Groups", EditorStyles.boldLabel);

            GUIStyle noScroll = new GUIStyle(GUI.skin.horizontalScrollbar);
            noScroll = GUIStyle.none;

            groupEditingScroll = EditorGUILayout.BeginScrollView(groupEditingScroll, false, false, noScroll, GUI.skin.verticalScrollbar, GUIStyle.none, GUILayout.Height(size.y - 45));

            editingList.DoLayoutList();

            EditorGUILayout.EndScrollView();

            GUILayout.BeginArea(new Rect(0, size.y - 25, size.x, 25), "", GUI.skin.box);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("To-Do List"))
            {
                showGroups.target = true;
                showEditingGroups.target = false;
                showPreferences.target = false;
                UpdateAnimBools();
            }

            if (GUILayout.Button("Preferences"))
            {
                showGroups.target = false;
                showEditingGroups.target = false;
                showPreferences.target = true;
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(list, "To-Do List Update");

                list.priorityGroups = editingList.list as List<PriorityGroup>;
            }
        }

        void ShowPreferences()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("To-Do List Preferences", EditorStyles.boldLabel);

            GUIStyle noScroll = new GUIStyle(GUI.skin.horizontalScrollbar);
            noScroll = GUIStyle.none;

            preferencesScroll = EditorGUILayout.BeginScrollView(preferencesScroll, false, false, noScroll, GUI.skin.verticalScrollbar, GUIStyle.none, GUILayout.Height(size.y - 45));

            bool displayWarningBeforeTaskRemoval = PreferenceBool(list.displayWarningBeforeTaskRemoval, "Show Warning Before Task Removal");
            bool showFinishedTasks = PreferenceBool(list.showFinishedTasks, "Show Finished Tasks");
            bool showButtonPrefix = PreferenceBool(list.showButtonPrefix, "Show Button Prefix");

            EditorGUILayout.EndScrollView();

            GUILayout.BeginArea(new Rect(0, size.y - 25, size.x, 25), "", GUI.skin.box);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("To-Do List"))
            {
                showGroups.target = true;
                showEditingGroups.target = false;
                showPreferences.target = false;
                UpdateAnimBools();
            }

            if (GUILayout.Button("Edit Groups"))
            {
                showGroups.target = false;
                showEditingGroups.target = true;
                showPreferences.target = false;
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(list, "To-Do List Update");

                list.displayWarningBeforeTaskRemoval = displayWarningBeforeTaskRemoval;
                list.showFinishedTasks = showFinishedTasks;
                list.showButtonPrefix = showButtonPrefix;
            }
        }

        bool PreferenceBool(bool value, string label)
        {
            bool newValue = value;
            EditorGUILayout.BeginHorizontal("Box");
            EditorGUILayout.LabelField(label);
            newValue = EditorGUILayout.Toggle(newValue);
            EditorGUILayout.EndHorizontal();
            return newValue;
        }

        void OnGUI()
        {
            if(Event.current.type == EventType.ValidateCommand)
            {
                if(Event.current.commandName == "UndoRedoPerformed")
                {
                    Repaint();
                }
            }

            if (list != null)
            {
                if (EditorGUILayout.BeginFadeGroup(showGroups.faded))
                {
                    ShowToDoList();
                }
                EditorGUILayout.EndFadeGroup();

                if (EditorGUILayout.BeginFadeGroup(showEditingGroups.faded))
                {
                    ShowGroupEditing();
                }
                EditorGUILayout.EndFadeGroup();

                if (EditorGUILayout.BeginFadeGroup(showPreferences.faded))
                {
                    ShowPreferences();
                }
                EditorGUILayout.EndFadeGroup();
            }
            else
            {
                GUIStyle warningLabel = new GUIStyle(GUI.skin.label);
                warningLabel.alignment = TextAnchor.MiddleCenter;

                GUI.Label(new Rect(0, 0, size.x, size.y), "The To-Do List can't be found in 'Resources/Editor'.\nPressing 'Fix Now' will create a Resources folder in the 'Assets' folder.", warningLabel);

                if (GUI.Button(new Rect(size.x / 2 - 100, size.y / 2 + 20, 200, 20), "Fix Now"))
                {
                    if (!Directory.Exists(Application.dataPath + "/Resources/Editor"))
                    {
                        Directory.CreateDirectory(Application.dataPath + "/Resources/Editor");
                    }

                    ToDoListObject asset = CreateInstance<ToDoListObject>();
                    PriorityGroup criticalGroup = new PriorityGroup();
                    PriorityGroup majorGroup = new PriorityGroup();
                    PriorityGroup minorGroup = new PriorityGroup();
                    PriorityGroup bugsGroup = new PriorityGroup();
                    PriorityGroup featuresGroup = new PriorityGroup();

                    criticalGroup.groupColor = new Color(1f, 0f, 0f);
                    criticalGroup.groupName = "Critical Issues";
                    criticalGroup.labelStyle = FontStyle.Bold;
                    majorGroup.groupColor = new Color(0f, 0f, 1f);
                    majorGroup.groupName = "Major Issues";
                    minorGroup.groupColor = new Color(1f, 0f, 1f);
                    minorGroup.groupName = "Minor Issues";
                    bugsGroup.groupColor = new Color(1f, 0.5f, 0f);
                    bugsGroup.groupName = "Bugs";
                    featuresGroup.groupColor = new Color(0f, 0.5f, 0f);
                    featuresGroup.groupName = "Features";

                    asset.priorityGroups.Add(criticalGroup);
                    asset.priorityGroups.Add(majorGroup);
                    asset.priorityGroups.Add(minorGroup);
                    asset.priorityGroups.Add(bugsGroup);
                    asset.priorityGroups.Add(featuresGroup);

                    string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Resources/Editor/ToDoList.asset");
                    AssetDatabase.CreateAsset(asset, path);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    UpdateAnimBools();
                    MakeEditingList();
                }
            }
        }
    }

}
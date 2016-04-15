using UnityEngine;
using System.Collections.Generic;
using UnityEditor.AnimatedValues;

namespace Gameotiv.ToDoList
{
    public class ToDoListObject : ScriptableObject
    {
        [HideInInspector]
        public List<PriorityGroup> priorityGroups = new List<PriorityGroup>();
        [HideInInspector]
        public bool displayWarningBeforeTaskRemoval = true;
        [HideInInspector]
        public bool showFinishedTasks = true;
        [HideInInspector]
        public bool showButtonPrefix = true;
    }

    [System.Serializable]
    public class PriorityGroup
    {
        //The name of the group.
        public string groupName;
        //The style of the label.
        public FontStyle labelStyle;
        public Color groupColor = Color.white;
        public AnimBool expanded = new AnimBool();
        public string newTaskField = "";
        public string newTaskDetails = "";

        public List<Task> tasks = new List<Task>();
    }

    [System.Serializable]
    public class Task
    {
        public string taskName;
        [Multiline(3)]
        public string taskDetails;
        public bool done = false;
        public bool editing = false;
        public AnimBool showDetails = new AnimBool();
    }
}
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

[System.Serializable]
[CreateAssetMenu (fileName = "New Module Mod", menuName = "Create New Module Mod", order = 0)]
public class ModuleMod : ScriptableObject {

    public enum Type { Menu, Toggle, Slider }
    public Type type;
    public string partName;
    public ModuleMod[] subparts;
    public int forcedCount = -1;

    public Vector3 position;
    public Text text;
    public Module module;

    private int meta;
    private int depth;

    public static GameObject[] currentMenu;
    public const int MAX_DEPTH = 5;

    public void OnClicked (int modmod) {
        switch (type) {
            case Type.Menu:
                OpenMods (position + new Vector3 (150, 20), subparts, depth + 1, module);
                break;
            case Type.Toggle:
                meta += modmod;
                WrapMeta ();

                text.text = partName + ": " + subparts[meta].partName;
                module.SendMessage ("OnToggleMod" + partName, meta, SendMessageOptions.DontRequireReceiver);
                break;
            case Type.Slider:
                meta = modmod;
                module.SendMessage ("OnSliderMod" + partName, meta, SendMessageOptions.DontRequireReceiver);
                text.text = partName + ": " + meta.ToString ();
                break;
        }
    }

    public void WrapMeta () {
        int length = forcedCount == -1 ? subparts.Length : forcedCount;
        if (meta < 0)
            meta = length - 1;
        meta %= (length);
    }

    public void Initialize () {
        switch (type) {
            case Type.Menu:
                AddButtonListener (text.GetComponentInParent<Button> (), 0);
                text.text = partName;
                break;

            case Type.Toggle:
                module.SendMessage ("OnInitializeToggleMod" + partName, this, SendMessageOptions.DontRequireReceiver);
                Button[] buttons = text.transform.parent.GetComponentsInChildren<Button> ();
                OnClicked (0);
                AddButtonListener (buttons[0], -1);
                AddButtonListener (buttons[1], 1);
                break;

            case Type.Slider:
                module.SendMessage ("OnInitializeSliderMod" + partName, this, SendMessageOptions.DontRequireReceiver);
                Slider slider = text.GetComponentInParent<Slider> ();
                slider.value = meta;
                AddSliderListener (slider);
                break;

            default:
                break;
        }
    }

    public void UpdateGUIElement () {
        if (subparts.Length == 0)
            return;

        switch (type) {
            case Type.Toggle:
                text.text = partName + ": " + subparts[meta].partName;
                break;
            case Type.Slider:
                text.text = partName + ": " + meta.ToString ();
                break;
        }
    }

    public void GetReturnedMeta (int _meta) {
        meta = _meta;
    }

    void AddButtonListener (Button button, int _meta) {
        button.onClick.AddListener (() => {
            OnClicked (_meta);
        });
    }
    void AddSliderListener (Slider slider) {
        slider.onValueChanged.AddListener (
            delegate { OnClicked ((int)slider.value); }
            );
    }

    public static void OpenMods (Vector3 position, ModuleMod[] mod, int depth, Module module) {

        if (depth > MAX_DEPTH) {
            Debug.LogError ("Shit went deeper than allowed.");
            return;
        }

        if (currentMenu[depth]) {
            Object.Destroy (currentMenu[depth]);
        }
        currentMenu[depth] = new GameObject ("ModuleMods");
        if (depth == 0)
            currentMenu[depth].transform.SetParent (Game.game.GUICanvas.transform, true);
        else
            currentMenu[depth].transform.SetParent (currentMenu[depth - 1].transform, true);

        for (int i = 0; i < mod.Length; i++) {
            GameObject newButton = Object.Instantiate (ModuleTreeButton.buttonTypes[(int)mod[i].type]);
            RectTransform rect = newButton.GetComponent<RectTransform> ();
            Vector2 size = rect.sizeDelta;
            newButton.transform.position = position + (Vector3)size / 2f - Vector3.up * size.y * (i+1);
            rect.SetParent (currentMenu[depth].transform, true);

            mod[i].position = position;
            mod[i].text = newButton.GetComponentInChildren<Text> ();
            mod[i].module = module;
            mod[i].depth = depth;
            mod[i].Initialize ();
        }
    }
}
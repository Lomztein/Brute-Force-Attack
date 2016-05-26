using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class FileBrowser : MonoBehaviour {

    public string[] files;

    public static FileBrowser currentBrowser;
    public static GameObject prefab;
    public static GameObject buttonPrefab;
    
    private GameObject returnObject;
    private string returnFuntion;

    public static void OpenFileBrowser (string directory, GameObject returnO, string returnS) {
        if (!prefab) {
            prefab = Resources.Load<GameObject> ("GUI/FileBrowser");
        }
        if (!buttonPrefab) {
            buttonPrefab = Resources.Load<GameObject> ("GUI/FileBrowserButton");
        }

        GameObject newBrowser = (GameObject)Instantiate (prefab, new Vector3 (Screen.width / 2f, Screen.height / 2f), Quaternion.identity);
        newBrowser.transform.SetParent (GameObject.Find ("Canvas").transform, true); // I know this is kind of slow, but it'll do.
        currentBrowser = newBrowser.GetComponent<FileBrowser> ();
        currentBrowser.Initialize (directory, returnO, returnS);
    }

    public void CloseFileBrowser () {
        returnObject.SendMessage ("OnFileBrowserClosed", SendMessageOptions.DontRequireReceiver);
        currentBrowser = null;
        Destroy (gameObject);
    }

    void Initialize (string directory, GameObject returnO, string returnS) {
        files = Directory.GetFiles (directory, "*.dat");

        for (int i = 0; i < files.Length; i++) {
            string name = Utility.ExtractSaveName (files[i]);
            string date = File.GetCreationTime (files[i]).ToString ();
            GameObject button = Instantiate (buttonPrefab);
            button.transform.SetParent (transform);
            button.transform.FindChild ("Name").GetComponent<Text> ().text = "  " + name;
            button.transform.FindChild ("Date").GetComponent<Text> ().text = date + "  ";
            AddLoadGameButtonListener (button.GetComponent<Button> (), i);
        }

        returnObject = returnO;
        returnFuntion = returnS;
    }

    void AddLoadGameButtonListener (Button button, int index) {
        button.onClick.AddListener (() => {
            returnObject.SendMessage (returnFuntion, files[index]);
            currentBrowser.CloseFileBrowser ();
        });
    }
}

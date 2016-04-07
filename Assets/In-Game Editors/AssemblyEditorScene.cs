using UnityEngine;
using System.Collections;
using IngameEditors;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace IngameEditors {

	public class AssemblyEditorScene : MonoBehaviour {

		public PurchaseMenu purchaseMenu;
		public float zoomSpeed;

        public Module rootModule;
        public bool continueSave;

        public GameObject saveMenu;
        public Text assemblyName;
        public Text assemblyDesc;

        public static AssemblyEditorScene cur;

        // Use this for initialization
        void Awake () {
            cur = this;
			ResearchMenu.InitializeAllStatics ();
			Game.currentScene = Scene.AssemblyBuilder;
			Game.credits = int.MaxValue;
			purchaseMenu.standard = Resources.LoadAll<GameObject> ("Modules").ToList ();
			purchaseMenu.Initialize ();
		}

        void Start () {
            Game.ForceDarkOverlay(false);
        }

        public void QuitScene () {
			Game.currentScene = Scene.Play;
            SceneManager.LoadScene ("pv_menu");
		}

		void Update () {
			Camera.main.orthographicSize = Mathf.Lerp (Camera.main.orthographicSize, 
			                                           Camera.main.orthographicSize + zoomSpeed * Input.GetAxis ("Scroll Wheel") * Time.deltaTime,
			                                           -zoomSpeed * Time.deltaTime);
		}

        public void SaveAssembly () {
            Game.ForceDarkOverlay(true);
            saveMenu.SetActive(true);
            continueSave = false;
            StartCoroutine(SAVE());
        }

        public void ContinueSave () {
            Game.ForceDarkOverlay(false);
            saveMenu.SetActive(false);
            continueSave = true;
        }

        IEnumerator SAVE () {
            while (!continueSave) {
                yield return new WaitForEndOfFrame();
            }

            rootModule.assemblyName = assemblyName.text;
            rootModule.assemblyDesc = assemblyDesc.text;
            rootModule.SaveModuleAssembly(rootModule.assemblyName);
        }
    }
}
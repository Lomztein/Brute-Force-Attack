using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour {

    public Tutorial tutorial;
    public GameObject[] disableOnAsk;

    public GameObject askBox;
    public RectTransform tutorialBox;
    public Text tutorialHeader;
    public Text tutorialText;
    public Button continueButton;

    public GameObject pointingMouse;
    public AssembliesSelectionMenu selectionMenu;

    public TutorialStep[] steps;
    public int stepIndex = -1;

    // Use this for initialization
    void Start () {
        Game.UpdateDarkOverlay ();
        if (PlayerPrefs.HasKey ("bHasPlayedBefore")) {
            Destroy (gameObject);
        } else {
            AskForTutorial ();
        }
    }

    public void EndTutorial () {
        PlayerPrefs.SetInt ("bHasPlayedBefore", 1);
        Game.game.RestartMap ();
    }

    void Update () {
        if (Game.credits != 5000000)
            Game.credits = 5000000;

        if (Game.research != 1337)
            Game.research = 1337;
    }

    public void StartTutorial () {
        askBox.SetActive (false);
        stepIndex = -1;

        StartCoroutine (PlayTutorial ());
    }

    void AskForTutorial () {
        foreach (GameObject obj in disableOnAsk) {
            obj.SetActive (false);
        }

        askBox.SetActive (true);
    }

    private bool proceedTutorial = false;
    private IEnumerator PlayTutorial () {
        tutorialBox.gameObject.SetActive (true);
        ContinueTutorial ();
        proceedTutorial = false;

        // Step 1 - Short introduction
        while (!proceedTutorial) {
            yield return null;
        }

        // Step 2 - Explain intro menu
        continueButton.interactable = false;
        foreach (GameObject obj in disableOnAsk) {
            obj.SetActive (true);
        }

        while (selectionMenu.gameObject.activeInHierarchy)
            yield return null;
        ContinueTutorial ();

        proceedTutorial = false;
        continueButton.interactable = true;
        // Step 3 - Battlefield introduction, disable all GUI.
        while (!proceedTutorial)
            yield return null;

        continueButton.interactable = false;
        Game.credits += 0;
        // Step 4 - Explain walling and pathing.
        while (!Game.game.AnyWallOfType (Game.WallType.Player))
            yield return null;
        ContinueTutorial ();

        Game.credits += 0;
        // Step 5 - Explain purchase menu.
        while (Game.currentModules.Count == 0)
            yield return null;
        ContinueTutorial ();

        // Step 6 - Explain research menu.
        while (!ResearchMenu.isOpen)
            yield return null;

        yield return new WaitForFixedUpdate ();

        tutorialBox.gameObject.SetActive (false);
        while (ResearchMenu.isOpen)
            yield return null;
        tutorialBox.gameObject.SetActive (true);
        ContinueTutorial ();

        continueButton.interactable = true;
        // Step 7 - Explain assembly editor access.
        proceedTutorial = false;
        while (!proceedTutorial)
            yield return null;

        proceedTutorial = false;
        // Step 8 - Explain waves.
        while (!proceedTutorial)
            yield return null;

        proceedTutorial = false;
        // Step 9 - Explain starting wave and fast mode.
        while (!proceedTutorial)
            yield return null;

        proceedTutorial = false;
        // Step 10 - Explain circle menu and upgrades.
        while (!proceedTutorial)
            yield return null;


        // Step 11 - Explain flush battlefield button.
        while (PlayerInput.cur.flushTime != 0)
            yield return null;
        ContinueTutorial ();

        proceedTutorial = false;
        // Step 12 - Start the game.
        while (!proceedTutorial)
            yield return null;

        EndTutorial ();
    }

    public void ContinueTutorial () {
        proceedTutorial = true;
        stepIndex++;

        if (stepIndex >= steps.Length) {
            EndTutorial ();
            return;
        }

        tutorialBox.position = GetPositionRelativefromMiddle (steps[stepIndex].position);
        tutorialHeader.text = steps[stepIndex].headerText;
        tutorialText.text = steps[stepIndex].infoText;

        if (stepIndex > 1) {
            ChangeAllSelectables (steps[stepIndex - 1].toDisable, true);
        }
        ChangeAllSelectables (steps[stepIndex].toDisable, false);

        if (steps[stepIndex].focusTransform) {
            Transform trans = steps[stepIndex].focusTransform;
            Vector3 middle = new Vector3 (Screen.width / 2f, Screen.height / 2f);

            pointingMouse.SetActive (true);
            float angle = Angle.CalculateAngle (middle, trans.position);
            float distance = Vector3.Distance (middle, trans.position);
            Vector3 pos = transform.position + (trans.position - middle).normalized * (distance - 100);

            pointingMouse.transform.position = pos;
            pointingMouse.transform.eulerAngles = new Vector3 (0f, 0f, angle);
        } else {
            pointingMouse.SetActive (false);
        }
    }

    void ChangeAllSelectables (GameObject[] list, bool setting) {
        foreach (GameObject obj in list) {
            Selectable[] sel = obj.GetComponentsInChildren<Selectable> ();
            foreach (Selectable s in sel) {
                s.interactable = setting;
            }
        }
    }

    Vector3 GetPositionRelativefromMiddle (Vector3 input) {
        return new Vector3 (
            Screen.width / 2 - input.x,
            Screen.height / 2 - input.y
            );
    }

    void OnDrawGizmos () {
        if (steps.Length > 0 && stepIndex < steps.Length) {
            Gizmos.DrawSphere (GetPositionRelativefromMiddle (steps[stepIndex].position), 5);
        }
    }

    [System.Serializable]
    public class TutorialStep {
        public GameObject[] toDisable;
        public Vector3 position;
        public Transform focusTransform;
        public string headerText;
        [TextArea]
        public string infoText;
    }
}

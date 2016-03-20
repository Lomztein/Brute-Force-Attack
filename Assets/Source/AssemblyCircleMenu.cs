using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AssemblyCircleMenu : MonoBehaviour {

    public RectTransform[] buttons;
    public Module module;

    public float offsetAngle;
    public float distance;

    public static AssemblyCircleMenu cur;

    public HoverContextElement sellElement;
    public HoverContextElement upgradeElement;

    private RangeIndicator rangeIndicator;

    private bool sureSell;

    void Awake () {
        cur = this;
        Close();
    }

    public static void Open (Vector3 position, Module _module) {
        Close();
        cur.module = _module;
        cur.gameObject.SetActive(true);
        cur.transform.position = (Vector2)position;
        for (int i = 0; i < cur.buttons.Length; i++) {
            cur.buttons[i].transform.position = cur.transform.position;
            cur.buttons[i].transform.localScale = Vector3.zero;
        }
        cur.StartCoroutine(cur.Animate());
        cur.UpdateHoverContextElements();

        cur.rangeIndicator = RangeIndicator.CreateRangeIndicator(cur.module.parentBase.gameObject, cur.module.transform.position, true, false).GetComponent<RangeIndicator>();
        cur.rangeIndicator.GetComponentInChildren<SpriteRenderer>().color = Color.green;
        cur.rangeIndicator.GetRange(cur.module.parentBase.GetRange());
        UpdateButtons();
    }

    void Update () {
        if (module) {
            transform.position = (Vector2)Camera.main.WorldToScreenPoint(module.transform.position);
            if (Input.GetMouseButtonDown(1))
                Close();
        }
    }

    private IEnumerator Animate () {
        bool targetReached = false;
        while (!targetReached) {
            for (int i = 0; i < buttons.Length; i++) {

                Vector3 targetPos = transform.position + Quaternion.Euler(0f, 0f, i * (360 / buttons.Length) + offsetAngle) * Vector3.one * distance;
                if (Vector3.Distance (buttons[i].position, targetPos) > 0.1f) {
                    buttons[i].position = Vector3.Lerp(buttons[i].position, targetPos, 10 * Time.fixedDeltaTime);
                    buttons[i].localScale = Vector3.Lerp(buttons[i].localScale, Vector3.one, 10 * Time.fixedDeltaTime);
                } else {
                    targetReached = true;
                }
            }

            yield return new WaitForFixedUpdate();
        }

        for (int i = 0; i < buttons.Length; i++) {
            buttons[i].position = transform.position + Quaternion.Euler(0f, 0f, i * (360 / buttons.Length) + offsetAngle) * Vector3.one * distance;
            buttons[i].localScale = Vector3.one;
        }
    }

    public void OpenContextMenu () {
        PlayerInput.cur.contextMenu.OpenAssembly(module);
    }

    public void SellModule () {
        if (sureSell) {
            module.SellModule();
        } else {
            sureSell = true;
            Invoke("ResetSureSell", 2f);
            UpdateHoverContextElements();
        }
    }

    public static void UpdateButtons () {
        if (cur.gameObject.activeSelf && cur.module) {
            if (Game.credits >= cur.module.GetFullUpgradeCost()) {
                cur.upgradeElement.GetComponent<Button>().interactable = true;
            } else {
                cur.upgradeElement.GetComponent<Button>().interactable = false;
            }
        }
    }

    public void UpgradeModule () {
        if (Game.credits >= module.GetFullUpgradeCost()) {
            module.UpgradeAssembly(0);
            module.UpgradeAssembly(1);
            module.UpgradeAssembly(2);
            cur.rangeIndicator.GetRange(cur.module.parentBase.GetRange());
            UpdateHoverContextElements();
            module.UpdateHoverContextElement();
        }
    }

    void UpdateHoverContextElements () {
        if (sureSell) {
            sellElement.text = "Are you sure?";
        } else 
            sellElement.text = "Sell assembly - " + module.GetSellAmount() + " LoC";

        if (module.IsAssemblyUpgradeable(-1)) {
            upgradeElement.text = "Upgrade Cost: " + module.GetFullUpgradeCost() + " LoC" +
                    "\n\tLevels - R/D/T: " + module.GetAssemblyUpgradeLevel(Module.Type.Base) + "/" + module.GetAssemblyUpgradeLevel(Module.Type.Weapon) + "/" + module.GetAssemblyUpgradeLevel(Module.Type.Rotator);
        } else {
            upgradeElement.text = "Maxed out";
            upgradeElement.GetComponent<Button>().interactable = false;
        }

        HoverContextElement.activeElement = null;
    }

    void ResetSureSell () {
        sureSell = false;
        UpdateHoverContextElements();
    }

    public static void Close () {
        cur.gameObject.SetActive(false);
        if (cur.rangeIndicator)
            Destroy(cur.rangeIndicator.gameObject);
    }
}

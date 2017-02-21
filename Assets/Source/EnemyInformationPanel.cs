using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EnemyInformationPanel : MonoBehaviour {

    public static EnemyInformationPanel panel;

    public Image enemySprite;
    public Text enemyName;
    public Text enemyStats;
    public Text enemyDesc;
    public Text enemyWeakness;
    public Text amountKilled;

    public Enemy currentEnemy;

    public void Initialize () {
        panel = this;
    }

    public static void Open (Enemy enemyType, int wave) {
        Game.UpdateDarkOverlay ();
        panel.gameObject.SetActive (true);
        panel.enemySprite.sprite = enemyType.transform.FindChild ("Sprite").GetComponent<SpriteRenderer> ().sprite;
        panel.enemyName.text = enemyType.enemyName;
        panel.enemyStats.text = Enemy.GetHealth (enemyType.health, EnemyManager.GetProgressForWave (wave)).ToString () + " hitpoints in wave " + wave.ToString () + "\n" +
            Enemy.GetValue (enemyType.value, wave) + " value in wave " + wave.ToString () + "\n" +
            enemyType.damage + " damage";
        panel.enemyDesc.text = enemyType.description;
        panel.enemyWeakness.text = "Weak against " + enemyType.type.ToString () + " weapons";
        panel.amountKilled.text = EnemyManager.GetKills (enemyType.enemyName) + " eradicated";
        panel.currentEnemy = enemyType;
    }

    // Not really effecient, but I doubt it'll have an real impact.
    void FixedUpdate () {
        if (currentEnemy) {
            panel.amountKilled.text = EnemyManager.GetKills (currentEnemy.enemyName) + " eradicated";
        }
    }

    public static void Close () {
        panel.gameObject.SetActive (false);
        Game.UpdateDarkOverlay ();
    }

    public void CloseInstance () {
        Close ();
    }
}

using UnityEngine;
using System.Collections;

public class ConstructionFlashShrink : MonoBehaviour {

    public float shrinkTime;

	void Animate (Vector3 startSize) {
        transform.localScale = startSize;
        StartCoroutine(Shrink());
	}

    IEnumerator Shrink () {
        Vector3 shrinkSpeed = transform.localScale / shrinkTime;
        while (transform.localScale.x > 0.1f) {
            transform.localScale -= shrinkSpeed * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        Destroy(gameObject);
    }

    public static void Create (Vector3 size, Vector3 position) {
        GameObject flash = (GameObject)Instantiate(PlayerInput.cur.constructionFlash, position, Quaternion.identity);
        flash.SendMessage("Animate", new Vector3(size.x, size.y, 1f));
    }

    // I figured I was a bit sick of fixing bugs right now :b
    // Yeah, fun little bugs like that often happen, though they are mostly patched instantly.

    // About 10 months later when I finially decided to finish this game, I have no idea what this^ is about.
}

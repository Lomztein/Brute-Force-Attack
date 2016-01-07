using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UpcomingElement : ScriptableObject {
	
	public Text upcomingText;
	public int remaining;
	
	public void Decrease () {
		remaining--;
		upcomingText.text = "x " + remaining;
	}
	
	public UpcomingElement (Text up, int rem) {
		upcomingText = up;
		remaining = rem;
	}
}
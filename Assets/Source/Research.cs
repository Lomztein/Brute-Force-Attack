using UnityEngine;
using System.Collections;

public class Research : ScriptableObject {

	public string desc;
	public string func;
	public Sprite sprite;
	public int cost;
	
	public int x;
	public int y;

	public GameObject button;
	public Research prerequisite;

}

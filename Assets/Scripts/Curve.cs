using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Curve : MonoBehaviour {
	public UnityEvent<Curve> OnChange { get; private set; }
	public List<Vector3> points;
	public bool AddOnMouse;
}


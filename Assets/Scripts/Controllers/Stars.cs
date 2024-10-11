using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stars : MonoBehaviour
{
	public List<Transform> starTransforms;
	public float drawingTime;

	int currentStar = 0;
	float lineLength = 0;

	LineRenderer line;

	private void Start() {
		line = GetComponent<LineRenderer>();
		line.gameObject.SetActive(true);
	}

	private void Update() {
		DrawConstellation();
	}

	public void DrawConstellation() {
		Transform star = starTransforms[currentStar];

		if (currentStar == starTransforms.Count-1) {
			// Last star
			currentStar = 0;
			lineLength = 0;
			return;
		}

		Transform nextStar = starTransforms[currentStar + 1];

		Vector3 diff = nextStar.position - star.position;
		Vector3 dir = diff.normalized;
		float dist = diff.magnitude;

		lineLength += (dist / drawingTime) * Time.deltaTime;

		//Debug.DrawLine(star.position, star.position + (dir * lineLength), Color.white);
		line.SetPosition(0, star.position);
		line.SetPosition(1, star.position + (dir * lineLength));

		if (lineLength >= dist) {
			lineLength = dist;
			currentStar++;
			lineLength = 0;
		}
    }
}

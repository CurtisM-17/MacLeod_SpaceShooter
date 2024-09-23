using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
	public float moveSpeed;
	public float arrivalDistance;
	public float maxFloatDistance;

	Vector3 randomPoint;

	private void Start() {
		GeneratePoint();
	}

	private void Update() {
		AsteroidMovement();
	}

	public void AsteroidMovement() {
		Vector3 diff = randomPoint - transform.position;

		transform.position += moveSpeed * Time.deltaTime * diff.normalized;

		if ((randomPoint - transform.position).magnitude <= arrivalDistance) GeneratePoint();
	}

	void GeneratePoint() {
		randomPoint = transform.position + new Vector3(Random.Range(-maxFloatDistance, maxFloatDistance), Random.Range(-maxFloatDistance, maxFloatDistance), 0);
	}
}

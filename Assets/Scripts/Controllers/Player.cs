using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
	public List<Transform> asteroidTransforms;
	public Transform enemyTransform;
	public GameObject bombPrefab;
	public Transform bombsTransform;

	public float moveSpeed, accelerationTime, decelerationTime;
	Vector3 moveVelocity;

	float timer = 0;

	private void Start() {
		//Application.targetFrameRate = 20;
	}

	void Update() {
		timer += Time.deltaTime;
		PlayerMovement(); // Invoke every frame
	}

	float Clamp(float change) {
		return Mathf.Clamp(change, -1, 1);
	}

	float Decelerate(float currentValue) {
		// If the value is below 0, just be instant. Otherwise divide the distance by the time (1 unit, 3000 milliseconds for example)
		float decelerationSpeed = (decelerationTime > 0) ? (1.0f / decelerationTime) : 1.0f;

		// Add up to 0 if negative, subtract down to 0 if positive
		return (currentValue < 0) ?
			Mathf.Clamp(currentValue + decelerationSpeed * Time.deltaTime, -1, 0)
			:
			Mathf.Clamp(currentValue - decelerationSpeed * Time.deltaTime, 0, 1);
	}

	float accelerationStart = 0;
	float accelerationTimeElapsed = 0;

	public void PlayerMovement() {
		// Re-calculate the speed in case the accelerationTime value changes during runtime
		float accelerationSpeed = (accelerationTime > 0) ? (1.0f / accelerationTime) : 1.0f; // Make sure the value is above 

		// Up and down arrows cannot be held at the same time; prioritize one or the other
		if (Input.GetKey(KeyCode.UpArrow)) {
			if (moveVelocity.y == 0) accelerationStart = timer;
			else if (accelerationTimeElapsed == 0 && moveVelocity.y >= 1) {
				accelerationTimeElapsed = timer - accelerationStart;
				print("Acceleration time: " + accelerationTimeElapsed);
			}

			moveVelocity.y = Clamp(moveVelocity.y + accelerationSpeed * Time.deltaTime); // (?, 1)
		} else if (Input.GetKey(KeyCode.DownArrow)) moveVelocity.y = Clamp(moveVelocity.y - accelerationSpeed * Time.deltaTime); // (?, -1)
		else moveVelocity.y = Decelerate(moveVelocity.y); // (?, 0) (neither being held down)

		if (Input.GetKey(KeyCode.LeftArrow)) moveVelocity.x = Clamp(moveVelocity.x - accelerationSpeed * Time.deltaTime); // (-1, ?)
		else if (Input.GetKey(KeyCode.RightArrow)) moveVelocity.x = Clamp(moveVelocity.x + accelerationSpeed * Time.deltaTime); // (1, ?)
		else moveVelocity.x = Decelerate(moveVelocity.x); // (0, ?)

		// Keep on the screen
		Vector3 moveVector = (moveSpeed * Time.deltaTime * moveVelocity); // Where we want the position to be
		Vector3 intendedPosOnScreen = Camera.main.WorldToViewportPoint(transform.position + moveVector); // Check if this position is within the viewport

		if (intendedPosOnScreen.x < 0.0 || intendedPosOnScreen.x > 1.0) moveVector.x = 0; // This position is to the left or right of the viewport; freeze X
		if (intendedPosOnScreen.y < 0.0 || intendedPosOnScreen.y > 1.0) moveVector.y = 0; // Same but for Y. Overwrites moveVector

		// Move
		transform.position += moveVector; // Add the new filtered move vector to the position
	}
}
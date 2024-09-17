using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	public List<Transform> asteroidTransforms;
	public Transform enemyTransform;
	public GameObject bombPrefab;
	public Transform bombsTransform;

	public float moveSpeed;
	Vector3 moveVelocity;

	void Update()
	{
		PlayerMovement();
	}

	void PlayerMovement()
	{
		if (Input.GetKey(KeyCode.UpArrow)) moveVelocity.y = 1;
		else if (Input.GetKey(KeyCode.DownArrow)) moveVelocity.y = -1;
		else moveVelocity.y = 0;

		if (Input.GetKey(KeyCode.LeftArrow)) moveVelocity.x = -1;
		else if (Input.GetKey(KeyCode.RightArrow)) moveVelocity.x = 1;
		else moveVelocity.x = 0;

		// Keep in screen
		Vector3 moveVector = (moveSpeed * Time.deltaTime * moveVelocity);
		Vector3 intendedPosOnScreen = Camera.main.WorldToViewportPoint(transform.position + moveVector);

		if (intendedPosOnScreen.x < 0.0 || 1.0 < intendedPosOnScreen.x) moveVector.x = 0;
		if (intendedPosOnScreen.y < 0.0 || 1.0 < intendedPosOnScreen.y) moveVector.y = 0;

		// Move
		transform.position += moveVector;

	}
}
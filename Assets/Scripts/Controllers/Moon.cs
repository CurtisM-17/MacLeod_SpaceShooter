using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moon : MonoBehaviour
{
	public Transform planetTransform;
	public float orbitRadius, orbitalSpeed;

	float currentAngle = 0f; // Angle in radians around the planet

	private void Update() {
		OrbitalMotion(orbitRadius, orbitalSpeed, planetTransform); // Invoke every frame with the current radius and speed setting
	}

	public void OrbitalMotion(float radius, float speed, Transform target) {
		transform.position = target.position + new Vector3(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle)) * radius; // P = ( cos(Θ), sin(Θ) ) * radius
		currentAngle += speed * Time.deltaTime; // Increment the current angle by the speed
	}
}



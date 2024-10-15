using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmProjectile : MonoBehaviour
{
	GameObject player;
	Vector3 direction;
	Rigidbody2D rb;
	public float speed = 8f;
	public float rotationSpeed = 15f;
	public float damage = 15f;
	bool isReturning = false;
	float timer;
	public float recallTime = 5f;
	GameObject mech;
	GameObject armAssetContainer;

	private void Start() {
		player = GameObject.FindGameObjectWithTag("Player");
		mech = GameObject.FindGameObjectWithTag("Mech");
		rb = GetComponent<Rigidbody2D>();
		armAssetContainer = transform.Find("Arm asset").gameObject;

		direction = (player.transform.position - transform.position).normalized;
	}

	private void Update() {
		timer += Time.deltaTime;

		if (mech == null) {
			Destroy(gameObject);
			return;
		}

		if (!isReturning && timer >= recallTime) isReturning = true;

		if (isReturning) {
			Vector3 armPos = mech.transform.Find("Arm").position - transform.position;
			direction = armPos.normalized;

			// Stop when arrived
			if (armPos.magnitude <= 0.5f) {
				mech.GetComponent<Mech>().ArmReturned();
				Destroy(gameObject);
			}
		}
	}

	private void FixedUpdate() {
		rb.position += speed * Time.deltaTime * (Vector2)direction;

		armAssetContainer.transform.eulerAngles -= new Vector3(0, 0, rotationSpeed) * Time.deltaTime;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!collision.CompareTag("Player")) return;

		Player.IncrementHealth(-damage);
		isReturning = true;
	}
}

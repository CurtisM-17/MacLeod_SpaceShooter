using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullets : MonoBehaviour
{
	Rigidbody2D rb;
	public float speed = 1.0f;
	public bool damagesPlayer = false;

	private void Start() {
		rb = GetComponent<Rigidbody2D>();

		Destroy(gameObject, 4);
	}

	private void FixedUpdate() {
		rb.MovePosition(rb.transform.position + rb.transform.up * speed * Time.fixedDeltaTime);
	}

	private void OnTriggerEnter2D(Collider2D collision) {
		if (damagesPlayer && collision.gameObject.CompareTag("Player")) Damage(true);
		else if (!damagesPlayer && collision.gameObject.CompareTag("Enemy")) Damage(false);
	}

	void Damage(bool isPlayer) {
		print(isPlayer);
		Destroy(gameObject);
	}
}

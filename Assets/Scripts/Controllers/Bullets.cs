using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullets : MonoBehaviour
{
	Rigidbody2D rb;
	public float speed = 1.0f;
	public float damage = 10f;
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
		else if (!damagesPlayer && collision.gameObject.CompareTag("Enemy")) DamageEnemy(collision.gameObject);
		else if (!damagesPlayer && collision.gameObject.CompareTag("Mech")) Damage(false);
	}

	void Damage(bool isPlayer) {
		if (isPlayer) Player.IncrementHealth(-damage);
		else if (!isPlayer) Mech.IncrementHealth(-damage);

		Destroy(gameObject);
	}

	void DamageEnemy(GameObject enemy) {
		enemy.GetComponent<Enemy>().Damage(damage);
		Destroy(gameObject);
	}
}

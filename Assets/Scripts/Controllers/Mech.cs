using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Mech : MonoBehaviour
{
	public static float maxHealth = 100;
	public static float health = maxHealth;
	static Slider healthBar;
	GameObject arm;
	Rigidbody2D armRb;
	Transform playerTransform;
	public GameObject enemyPrefab;

	public float moveArmSpeed = 2f;

	bool currentlyPointing = false;
	Vector3 currentArmPointAt = Vector3.zero;
	string pointContext; // Current reason why the arm is pointing

	private void Start() {
		healthBar = GameObject.FindGameObjectWithTag("EnemyHealthMeter").GetComponent<Slider>();
		healthBar.maxValue = maxHealth;
		healthBar.value = health;

		arm = transform.Find("Arm").gameObject;
		armRb = arm.GetComponent<Rigidbody2D>();
		enemySpawnpoint = arm.transform.Find("EnemySpawnpoint");

		playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

		//NewAttackTime();
		currentAttackTime = 3;
		StartCoroutine(Attack());
	}

	private void FixedUpdate() {
		if (health <= 0) return;

		if (currentlyPointing) PointAt(currentArmPointAt);
	}

	private void OnTriggerStay2D(Collider2D collision) {
		if (!collision.gameObject.CompareTag("Player")) return;

		collision.gameObject.SendMessage("IncrementHealth", -15f * Time.deltaTime);
	}

	public static void IncrementHealth(float increment) {
		health = Mathf.Clamp(health + increment, 0, maxHealth);
		healthBar.value = health;
		if (health == 0) Die();
	}

	static void Die() {
		Destroy(GameObject.FindGameObjectWithTag("Mech"));
	}

	/// Move arm (point at)
	void PointAt(Vector3 position) {
		currentlyPointing = true;
		currentArmPointAt = position;

		Vector3 diff = (position - armRb.transform.position);

		float rotationAngle = -(Mathf.Atan2(diff.x, diff.y) * Mathf.Rad2Deg) - 180;
		//armRb.rotation = rotationAngle;

		float progress = rotationAngle - armRb.rotation;
		if (Mathf.Abs(progress) % 360 <= 5f) {
			currentlyPointing = false;
			FinishedPointing();
			return;
		}

		armRb.rotation += ((progress < 0) ? moveArmSpeed : -moveArmSpeed) * Time.deltaTime;
	}

	/// Attacks
	public float minAttackTime, maxAttackTime;
	float currentAttackTime;
	Transform enemySpawnpoint;

	void NewAttackTime() {
		currentAttackTime = Random.Range(minAttackTime, maxAttackTime);
	}

	IEnumerator Attack() {
		yield return new WaitForSeconds(currentAttackTime);

		//if (Random.Range(1, 3) == 1) DeployShipAttack(); else ThrowArmAttack(); // 50/50 chance for either attack
		DeployShipAttack_Start();

		NewAttackTime();
		//StartCoroutine(Attack());
	}

	void DeployShipAttack_Start() {
		pointContext = "Deploy Ship";
		PointAt(playerTransform.position);
	}

	void DeployShipAttack_Deploy() {
		Instantiate(enemyPrefab, enemySpawnpoint.position, enemyPrefab.transform.rotation);
	}

	void ThrowArmAttack() {
		print("Throw arm");
	}

	void FinishedPointing() {
		if (pointContext == "Deploy Ship") {
			DeployShipAttack_Deploy();
		}
	}
}

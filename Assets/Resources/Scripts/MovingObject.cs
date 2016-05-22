using UnityEngine;
using System.Collections;

public class MovingObject : MonoBehaviour {

	private SpriteRenderer sprite;
	private Rigidbody2D rb;

	private float speed;

	// Use this for initialization
	void Start () {
	}

	public void SetSpeed(float newSpeed) {
		speed = newSpeed;
		rb = GetComponent<Rigidbody2D>();
		rb.velocity = new Vector2(0,-speed);
	}
	
	// Update is called once per frame
	void Update () {
		if (transform.position.y < -400) {
			Destroy(gameObject);
		}
	}
}

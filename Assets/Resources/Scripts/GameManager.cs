using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour {
	
	public static GameManager instance = null;

	public GameObject playerPrefab;
	public GameObject block;
	public GameObject jewel;
	public GameObject shieldPowerUp;
	public GameObject shield;
	public GameObject deathParticles;

	public AudioClip shipDestroyedSound;

	[HideInInspector]
	public enum GameState {TITLE, GAMEPLAY, GAMEOVER};
	[HideInInspector]
	public GameState state;
	[HideInInspector]
	public bool disableInput;

	private Camera mainCam;
	private Player player;
	private Transform movingObjectsHolder;
	private GameObject logo;
	private GameObject touchToStart;
	private GameObject credits;
	private GameObject gameOverPanel;
	private Text scoreResultsText;
	private Text scoreText;
	private Text bestScoreText;

	private int score;
	private int bestScore;

	private string hexColor;
	private float movingObjectSpeed;
	private float waveDelay = 375f;
	private bool canAddVerBlock;
	private int nVerBlocks;
	private int waveCount;

	private GameObject lastMovingObject;
	private GameObject shieldInstance;

	private const int nColors = 6;
	private string[] colors = new string[nColors] {"51D427", "DD3824", "6024DD", "00CEAC", "EEC30C", "EE1CE8"};

	void Awake () {
		
		if (instance == null) {
			instance = this;
		}
		else if (instance != this) {
			Destroy(gameObject);
		}
		
		DontDestroyOnLoad(gameObject);

		mainCam = GameObject.Find("Main Camera").GetComponent<Camera>();
		movingObjectsHolder = GameObject.Find("MovingObjectsHolder").transform;

		scoreText = GameObject.Find("ScoreText").GetComponent<Text>();
		bestScoreText = GameObject.Find("BestScoreText").GetComponent<Text>();
		
		gameOverPanel = GameObject.Find("GameOverPanel");
		scoreResultsText = gameOverPanel.transform.GetChild(0).GetComponent<Text>();

		logo = GameObject.Find("Logo");
		touchToStart = GameObject.Find("TouchToStartText");
		credits = GameObject.Find("Credits");

		state = GameState.TITLE;

		Setup();
	}

	private void Setup() {

		// Get a different color each time
		do {
			hexColor = colors[Random.Range(0,nColors)];
		} while (SameColor(hexColor));

		mainCam.backgroundColor = HexToColor(hexColor);
		
		gameOverPanel.SetActive(false);

		score = 0;
		scoreText.text = "SCORE\n" + score.ToString();

		bestScore = PlayerPrefs.GetInt("BestScore");
		bestScoreText.text = "BEST\n" + bestScore.ToString();
	}
	
	private void Init() {

		disableInput = true;

		movingObjectSpeed = 275;
		nVerBlocks = 0;
		waveCount = 0;

		Instantiate(playerPrefab, new Vector2(RandomSign() * 60,-400), Quaternion.identity);
		player = GameObject.FindWithTag("Player").GetComponent<Player>();
	}

	public void UpdateScore() {
		score++;
		if (score > 99999) {
			score = 99999;
		}
		scoreText.text = "SCORE\n" + score.ToString();
	}

	public void GameOver() {
		StartCoroutine(GameOverCoroutine());
	}

	private IEnumerator GameOverCoroutine() {
		
		SoundManager.instance.PlaySingle(shipDestroyedSound);

		state = GameState.GAMEOVER;
		disableInput = true;

		GameObject particleSystem = Instantiate(deathParticles, player.gameObject.transform.localPosition, Quaternion.identity) as GameObject;

		Destroy(player.gameObject);

		yield return new WaitForSeconds(2);

		Destroy(particleSystem);
		gameOverPanel.SetActive(true);

		string scoreResultsMsg = "Score: " + score + "\n";

		if (score > bestScore) {
			scoreResultsMsg += "New Best Score !!";
			PlayerPrefs.SetInt("BestScore", score);
			PlayerPrefs.Save();
		} else {
			scoreResultsMsg += "Best Score: " + bestScore;
		}

		scoreResultsText.text = scoreResultsMsg;

		yield return new WaitForSeconds(1);

		disableInput = false;
	}

	public void AddShield() {
		shieldInstance = Instantiate(shield, Vector3.zero, Quaternion.identity) as GameObject;
	}

	public void DestroyShield() {
		Destroy(shieldInstance);
	}
	
	// Player becomes invincible for 3 seconds
	public IEnumerator AddInvincibility() {
		player.SetInvincible(true);
		yield return new WaitForSeconds(3f);
		player.SetInvincible(false);
	}

	private void ChangeSpeed(float newSpeed) {
		// Change speed for future moving objects
		movingObjectSpeed = newSpeed;
		// Change speed for existing moving objects
		foreach (Transform child in movingObjectsHolder) {
			child.gameObject.GetComponent<MovingObject>().SetSpeed(movingObjectSpeed);
		}
	}

	private int RandomSign() {
    	return Random.value < 0.5f ? 1 : -1;
	}

	private bool SameColor(string hex) {
		return mainCam.backgroundColor == HexToColor(hex);
	}

	private Color HexToColor(string hex) {
		byte r = byte.Parse(hex.Substring(0,2), System.Globalization.NumberStyles.HexNumber);
		byte g = byte.Parse(hex.Substring(2,2), System.Globalization.NumberStyles.HexNumber);
		byte b = byte.Parse(hex.Substring(4,2), System.Globalization.NumberStyles.HexNumber);
		return new Color32(r,g,b,255);
	}

	private GameObject InstantiateMovingObject(GameObject prefab, Vector2 pos, Vector3 scale, int rotation, string hexColor) {
		GameObject movingObject;
		movingObject = Instantiate(prefab, pos, Quaternion.identity) as GameObject;
		movingObject.GetComponent<MovingObject>().SetSpeed(movingObjectSpeed);
		movingObject.transform.localScale = scale;
		movingObject.transform.Rotate(Vector3.forward * rotation);
		movingObject.GetComponent<SpriteRenderer>().color = HexToColor(hexColor);
		movingObject.transform.SetParent(movingObjectsHolder);
		return movingObject;
	}

	void Update() {

		if (disableInput) return;
	
		if (state == GameState.TITLE) {
			if ((Input.GetMouseButtonDown(0)) || (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)) {
				logo.transform.position = new Vector3(0,-1000,0);
				touchToStart.transform.position = new Vector3(0,-1000,0);
				credits.transform.position = new Vector3(0,-1000,0);
				Init();
			}
		}

		if (state == GameState.GAMEOVER) {
			if ((Input.GetMouseButtonDown(0)) || (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)) {
				foreach (Transform child in movingObjectsHolder) {
					Destroy(child.gameObject);
				}
				Setup();
				Init();
			}
		}

		// Regular Gameplay
		if (state == GameState.GAMEPLAY) {

			if (lastMovingObject == null || lastMovingObject.transform.position.y < waveDelay) {

				// Increase speed every 10 waves
				if ((movingObjectSpeed < 500)  && (waveCount++ % 10 == 0)) {
					ChangeSpeed(movingObjectSpeed + 15);
				}

				// Block
				lastMovingObject = InstantiateMovingObject(block, new Vector2(0,500), new Vector3(0.4f,0.8f,1), 0, hexColor);

				// Jewel, Clock & Shield Left Side
				if (Random.value < 0.5f) {
					float rand = Random.value;
					if (rand < 0.02f) {
						InstantiateMovingObject(shieldPowerUp, new Vector2(-60,569f), new Vector3(0.55f,0.55f,1), 0, "FFFFFF");
					}
					else {
						InstantiateMovingObject(jewel, new Vector2(-60,569f), new Vector3(1,1,1), 0, "FFFFFF");
					}
				}

				// Jewel, Clock & Shield Right Side
				if (Random.value < 0.5f) {
					float rand = Random.value;
					if (rand < 0.02f) {
						InstantiateMovingObject(shieldPowerUp, new Vector2(60,569f), new Vector3(0.55f,0.55f,1), 0, "FFFFFF");
					}
					else {
						InstantiateMovingObject(jewel, new Vector2(60,569f), new Vector3(1,1,1), 0, "FFFFFF");
					}
				}

				// Horizontal Block
				if (nVerBlocks == 1 || Random.value < 0.6f) {
					InstantiateMovingObject(block, new Vector2(RandomSign() * 60,500), new Vector3(1.9f,0.8f,1), 0, hexColor);
					canAddVerBlock = false;
					nVerBlocks = 0;
				} else {
					canAddVerBlock = true;
				}

				// Vertical Block
				if (canAddVerBlock && Random.value < 0.5f) {
					InstantiateMovingObject(block, new Vector3(0,565,1), new Vector3(2.1f,0.8f,1), 90, hexColor);
					nVerBlocks++;
				}

			}

		}
	}

}

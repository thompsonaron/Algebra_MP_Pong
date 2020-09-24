using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Ball Settings")]
    public Transform ball;
    public Vector3 ballDir;
    public float ballSpeed = 10f;
    public float defaultBallSpeed;


    [Header("Object Positions")]
    public Transform player1;
    public Transform player2;
    public Transform player1LaunchPos;
    public Transform player2LaunchPos;

    private bool player1LastHit = false;
    private bool player2LastHit = false;

    public Transform topWall;
    public Transform bottomWall;

    private Vector3 player1StartingPos;
    private Vector3 player2StartingPos;

    [Header("Movement Settings")]
    public float movementSpeedPlayer1 = 10f;
    public float movementSpeedPlayer2 = 10f;
    public float movementSpeedBase = 10f;

    private float maxTopPos;
    private float maxBottomPos;

    // powerups
    [Header("Movement Settings")]
    private float powerUp_SlowDownDuration = 5f;
    private float powerUp_OverspeedDuration = 5f;

    private bool ballIsShot = false;

    [Header("Sinusoid settings")]
    float startTime;
    float offset;
    bool sinusoidPowerupOn = false;
    public float peroid = 20f;
    public float amplitude = 0.2f;
    public float sinusoidDuration = 3f;

    [Header("Spawner prefabs")]
    public GameObject[] spawnItems;
    public float spawnItemsY = 0.25f;
    public float spawnItemsMinMaxZ = 4.1f;
    public float spawnItemsMinMaxX = 6.5f;
    public float itemSpawnTime = 5f;

    // player points
    int player1Points;
    int player2Points;

    // UI
    public Text player1ScoreText;
    public Text player2ScoreText;
    public Text time;


    // Start is called before the first frame update
    void Start()
    {
        player1StartingPos = player1.position;
        player2StartingPos = player2.position;

        maxTopPos = topWall.position.z - topWall.localScale.y;
        maxBottomPos = bottomWall.position.z + topWall.localScale.y;

        ballDir = Vector3.left;
        defaultBallSpeed = ballSpeed;

        startTime = Time.time;

        player1Points = 0;
        player2Points = 0;

        UpdateScore();
        time.text = string.Empty;

        StartCoroutine(SpawnItemsRoutine());
    }

    private void UpdateScore()
    {
        player1ScoreText.text = player1Points.ToString();
        player2ScoreText.text = player2Points.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        // player1 input
        if (Input.GetKey(KeyCode.W) && player1.position.z < maxTopPos)
        {
            player1.position = new Vector3(player1.position.x, player1.position.y, player1.position.z + movementSpeedPlayer1 * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S) && player1.position.z > maxBottomPos)
        {
            player1.position = new Vector3(player1.position.x, player1.position.y, player1.position.z - movementSpeedPlayer1 * Time.deltaTime);
        }

        // player2 input
        if (Input.GetKey(KeyCode.UpArrow) && player2.position.z < maxTopPos)
        {
            player2.position = new Vector3(player2.position.x, player2.position.y, player2.position.z + movementSpeedPlayer2 * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.DownArrow) && player2.position.z > maxBottomPos)
        {
            player2.position = new Vector3(player2.position.x, player2.position.y, player2.position.z - movementSpeedPlayer2 * Time.deltaTime);
        }
        if (Input.GetKeyDown(KeyCode.Space) && ballSpeed == 0f)
        {
            ballIsShot = true;
        }

        // ball movement
        ball.position = ball.position + (ballDir * ballSpeed * Time.deltaTime);

        // sinusoid movement
        if (sinusoidPowerupOn)
        {
            var offset = amplitude * (Mathf.Sin(peroid * (Time.time - startTime)));
            ball.position = new Vector3(ball.position.x, ball.position.y, ball.position.z + offset);
        }

        // Debug.Log(ballDir);
        // Debug.Log(Time.time - startTime);

        // TODO increase ball speed if it is above 0
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject collidedObj = collision.gameObject;

        // disabling sinusoid movement
        //sinusoidDuration = 0f;
        //sinusoidPowerupOn = false;

        if (collidedObj.CompareTag("Player1") || collidedObj.CompareTag("Player2"))
        {
            if (collidedObj.CompareTag("Player1"))
            {
                player1LastHit = true;
                player2LastHit = false;
            }
            else
            {
                player1LastHit = false;
                player2LastHit = true;
            }

            ballDir = Vector3.Reflect(ballDir, Vector3.right);

            // randomize a bit
            ballDir = new Vector3(ballDir.x, ballDir.y, ballDir.z + UnityEngine.Random.Range(-1f, 1f));

            ballSpeed += 0.5f;
        }
        if (collidedObj.CompareTag("Wall"))
        {
            ballDir = Vector3.Reflect(ballDir, Vector3.forward);
            ballSpeed += 0.5f;
        }
        if (collidedObj.CompareTag("Obstacle"))
        {
            // checking if ball most front part is less then obstacle back part or if ball most back part is in front of obstacles most front part
            if ((ball.position.x + (ball.localScale.x / 2)) - 0.05f <= (collidedObj.transform.position.x - (collidedObj.transform.localScale.x / 2)))
            {
                ballDir = Vector3.Reflect(ballDir, Vector3.right);
            }
            else if ((ball.position.x - (ball.localScale.x / 2) + 0.05f) >= (collidedObj.transform.position.x + (collidedObj.transform.localScale.x / 2)))
            {
                // = Vector3.Reflect(ballDir, Vector3.left);
                ballDir = Vector3.Reflect(ballDir, Vector3.right);
            }
            else
            {
                ballDir = Vector3.Reflect(ballDir, Vector3.forward);
            }
            Destroy(collidedObj.gameObject);
            ballSpeed += 0.5f;
        }
        if (collidedObj.CompareTag("PowerUp"))
        {
            #region OldCode
            //if(collidedObj.name.Contains("PowerUp_SlowDown"))
            //{
            //    StartCoroutine(PowerUp_SlowDown());
            //}
            //else if (collidedObj.name.Contains("PowerUp_Overspeed"))
            //{
            //    StartCoroutine(PowerUp_Overspeed());
            //}
            //else if (collidedObj.name.Contains("PowerUp_Magnet"))
            //{
            //    StartCoroutine(PowerUp_Magnet());
            //}
            //else if (collidedObj.name.Contains("PowerUp_Sinusoid"))
            //{
            //    StartCoroutine(PowerUp_Sinusoid());
            //} 
            #endregion
            if (collidedObj.name.Contains("("))// or PowerUp
            {
                string coroutineName = collidedObj.name.Substring(0, collidedObj.name.IndexOf('('));
                StartCoroutine(coroutineName);
            }

            Destroy(collidedObj.gameObject);
        }
        
        ballDir.Normalize();
    }

    private void OnTriggerEnter(Collider other)
    {
        bool player1Scored = true;

        if (other.gameObject.name == "RedGoal")
        {
            // score player 2
            player2Points++;
            player1Scored = false;
        }
        else
        {
            // score player 1
            player1Points++;
        }

        UpdateScore();
        StopAllCoroutines();
        StartCoroutine(RestartPlay(player1Scored));
    }

    private IEnumerator RestartPlay(bool player1Scored)
    {
        // restart ball, players,remove all powerups and obstacles and start and change ball direction towards losing point player
        player1.position = player1StartingPos;
        player2.position = player2StartingPos;
        ball.position = new Vector3(0f, ball.position.y, 0f);
        
        GameObject[] spawnedPowerUps = GameObject.FindGameObjectsWithTag("PowerUp");
        GameObject[] spawnedObstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        foreach (var item in spawnedPowerUps) { Destroy(item); }
        foreach (var item in spawnedObstacles) { Destroy(item); }

        ballSpeed = 0f;
        sinusoidPowerupOn = false;
        // 2 time
        time.text = "2";
        yield return new WaitForSeconds(1f);
        ballDir = Vector3.left;
        if (player1Scored)
        {
            ballDir = Vector3.right;
        }
        // 1 time
        time.text = "1";
        yield return new WaitForSeconds(1f);
        time.text = "";
        // go
        ballSpeed = defaultBallSpeed;
        StartCoroutine(SpawnItemsRoutine());
    }

    private IEnumerator PowerUp_SlowDown()
    {
        bool player1HitLast = player1LastHit;
        if (player1HitLast) { movementSpeedPlayer1 /= 2; }
        else { movementSpeedPlayer2 /= 2; }

        yield return new WaitForSeconds(powerUp_SlowDownDuration);

        if (player1HitLast) { movementSpeedPlayer1 = movementSpeedBase; }
        else { movementSpeedPlayer2 = movementSpeedBase; }
    }

    private IEnumerator PowerUp_Overspeed()
    {
        bool player1HitLast = player1LastHit;
        if (player1HitLast) { movementSpeedPlayer1 *= 2; }
        else { movementSpeedPlayer2 *= 2; }

        yield return new WaitForSeconds(powerUp_OverspeedDuration);

        if (player1HitLast) { movementSpeedPlayer1 = movementSpeedBase; }
        else { movementSpeedPlayer2 = movementSpeedBase; }
    }

    private IEnumerator PowerUp_Magnet()
    {
        sinusoidPowerupOn = false;

        var tempBallSpeed = ballSpeed;
        ballSpeed = 0f;
        ballIsShot = false;
        if (player1LastHit)
        {
            this.transform.position = player1LaunchPos.position;
            this.transform.parent = player1.transform;
        }
        else
        {
            this.transform.position = player2LaunchPos.position;
            this.transform.parent = player2.transform;
        }

        yield return new WaitUntil(() => ballIsShot);

        this.transform.parent = null;

        if (player1LastHit) { ballDir = Vector3.right; }
        else { ballDir = Vector3.left; }
        ballSpeed = tempBallSpeed;
    }

    private IEnumerator PowerUp_Sinusoid()
    {
        startTime = Time.time;
        offset = 0f;
        sinusoidPowerupOn = true;

        yield return new WaitUntil(() => (Time.time - startTime) > sinusoidDuration);

        sinusoidPowerupOn = false;
    }

    private IEnumerator SpawnItemsRoutine()
    {
        yield return new WaitForSeconds(itemSpawnTime);
        Instantiate(spawnItems[Mathf.RoundToInt(UnityEngine.Random.Range(0, spawnItems.Length - 1))],
            new Vector3(UnityEngine.Random.Range(-spawnItemsMinMaxX, spawnItemsMinMaxX), spawnItemsY, UnityEngine.Random.Range(-spawnItemsMinMaxZ, spawnItemsMinMaxZ)),
            Quaternion.identity);

        StartCoroutine(SpawnItemsRoutine());
    }
}

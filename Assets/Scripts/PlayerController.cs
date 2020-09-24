using System.Collections;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
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
    }

    private void UpdateScore()
    {
        player1ScoreText.text = player1Points.ToString();
        player2ScoreText.text = player2Points.ToString();
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject collidedObj = collision.gameObject;

        if (collidedObj.CompareTag("Player1") || collidedObj.CompareTag("Player2"))
        {
            if (collidedObj.CompareTag("Player1"))
            {
                player1LastHit = true;
              //  player2LastHit = false;
            }
            else
            {
                player1LastHit = false;
                //player2LastHit = true;
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
        // disabling
        ballSpeed = 0f;
        sinusoidPowerupOn = false;
        // repositioning
        player1.position = player1StartingPos;
        player2.position = player2StartingPos;
        ball.position = new Vector3(0f, ball.position.y, 0f);
        
        // clearing
        GameObject[] spawnedPowerUps = GameObject.FindGameObjectsWithTag("PowerUp");
        GameObject[] spawnedObstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        foreach (var item in spawnedPowerUps) { Destroy(item); }
        foreach (var item in spawnedObstacles) { Destroy(item); }
        
        // 2 time
        time.text = "2";
        yield return new WaitForSeconds(1f);
        ballDir = Vector3.left;
        if (player1Scored) { ballDir = Vector3.right; }
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
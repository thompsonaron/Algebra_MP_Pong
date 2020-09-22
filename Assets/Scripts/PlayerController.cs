using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private float powerUp_SlowDownDuration = 5f;
    private float powerUp_OverspeedDuration = 5f;

    private bool ballIsShot = false;

    // Start is called before the first frame update
    void Start()
    {
        player1StartingPos = player1.position;
        player2StartingPos = player2.position;

        maxTopPos = topWall.position.z - topWall.localScale.y;
        maxBottomPos = bottomWall.position.z + topWall.localScale.y;

        ballDir = Vector3.left;
        defaultBallSpeed = ballSpeed;
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


        Debug.Log(ballDir);
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject collidedObj = collision.gameObject;
        if (collidedObj.CompareTag("Player1") || collidedObj.CompareTag("Player2"))
        {
            Debug.Log("Collided");

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
            ballDir = new Vector3(ballDir.x, ballDir.y, ballDir.z + Random.Range(-1f, 1f));
        }
        if (collidedObj.CompareTag("Wall"))
        {
            ballDir = Vector3.Reflect(ballDir, Vector3.forward);
        }
        if (collidedObj.CompareTag("Obstacle"))
        {
            // checking if ball most front part is less then obstacle back part or if ball most back part is in front of obstacles most front part
            if ((ball.position.x + ball.localScale.x / 2) <= (collidedObj.transform.position.x - collidedObj.transform.localScale.x / 2) 
                || (ball.position.x - ball.localScale.x / 2) >= (collidedObj.transform.position.x + collidedObj.transform.localScale.x / 2))
            {
                ballDir = Vector3.Reflect(ballDir, Vector3.right);
            }
            else
            {
                ballDir = Vector3.Reflect(ballDir, Vector3.forward);
            }
            Destroy(collidedObj.gameObject);
        }
        if (collidedObj.CompareTag("PowerUp"))
        {
            if(collidedObj.name.Contains("PowerUp_SlowDown"))
            {
                StartCoroutine(PowerUp_SlowDown());
                Destroy(collidedObj.gameObject);
            }
            else if (collidedObj.name.Contains("PowerUp_Overspeed"))
            {
                StartCoroutine(PowerUp_Overspeed());
                Destroy(collidedObj.gameObject);
            }
            else if (collidedObj.name.Contains("PowerUp_Magnet"))
            {
                StartCoroutine(PowerUp_Magnet());
                Destroy(collidedObj.gameObject);
            }
        }


        ballDir.Normalize();
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
        ballSpeed = defaultBallSpeed;

    }
}

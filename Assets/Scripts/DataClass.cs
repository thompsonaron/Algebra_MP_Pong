using UnityEngine;
using UnityEngine.UI;

public partial class  PlayerController
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
    // private bool player2LastHit = false;

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
}
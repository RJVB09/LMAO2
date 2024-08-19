using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Maze;

public class BackroomEntity : MonoBehaviour
{
    bool spawned = false;
    public Maze maze;
    Player player;
    Transform playerTransform;
    PlayerController playerController;
    Rigidbody playerRigidbody;
    MazeSolver solver;
    AudioSource warningAudioSource;
    BackroomsManager manager;
    public AudioSource monsterAudioSource;

    public AudioSource ambientAudioSource;
    public AudioSource screamAudioSource;

    public AudioClip[] ambientNoises;
    public AudioClip[] screams;

    public AudioClip warningTryHunt;
    public AudioClip sonar;

    float ambientTimer = 0;
    float screamTimer = 0;

    Animator animator;

    float currentSpeed = 3f;
    float currentTurnSpeed = 2f;

    public float walkSpeed = 3f;
    public float runSpeed = 5f;
    public float walkTurnSpeed = 3f;
    public float runTurnSpeed = 5f;
    public float huntSpeedMultiplier = 1.3f;
    bool stepping = false;

    public MonsterState currentState = MonsterState.Hunt;
    MonsterState previousState = MonsterState.Wander;

    public AudioClip huntSound;

    public Camera jumpscareCamera;

    public enum MonsterState
    { 
        Wander,
        Chase,
        Hunt,
        Kill
    }

    public void Spawn()
    {
        if (maze != null)
        {
            Vector3 initPos = maze.MazePosToWorldPos(maze.cornersNormalized[Random.Range(0, maze.cornersNormalized.Length)] * (maze.mazeSize - 1));
            transform.position = new Vector3(initPos.x, transform.position.y, initPos.z);
            solver = new MazeSolver(maze);
            wanderPos = new Vector2Int(Random.Range(0, maze.mazeSize), Random.Range(0, maze.mazeSize));
            previousDirection = Direction.PositiveY;
            huntBeginTime = chaseTime - Random.Range(minTimeTillHunt, maxTimeTillHunt);
            spawned = true;
        }
    }

    private void Start()
    {
        warningAudioSource = gameObject.GetComponent<AudioSource>();
        animator = gameObject.GetComponentInChildren<Animator>();
        player = FindObjectOfType<Player>();
        playerTransform = player.transform;
        playerRigidbody = player.GetComponentInChildren<Rigidbody>();
        playerController = player.GetComponent<PlayerController>();
        manager = FindObjectOfType<BackroomsManager>();
        jumpscareCamera.gameObject.SetActive(false);
    }

    Vector2Int a; //the monster goes from a to
    Vector2Int b; //b
    List<Direction> path;
    Direction previousDirection;

    public Vector2Int wanderPos;

    Vector2Int lastSeenPosition;

    public int huntTimer = 50;
    public float chaseTime = 0.4f; //How many seconds should the player stay out of sight for the monster to give up

    public bool playerInSight = false;
    Vector3 rayDirection;
    bool inFOV;
    bool seen;

    float seenTimer = 0;
    public float maxTimeTillHunt = 60;
    public float minTimeTillHunt = 20;
    float huntBeginTime = -10; //(negative) how long has it been since the player was seen to start a hunt

    //BUGGY WHEN YOU WALK INTO THE MOSTER, NEEDS FIXING.
    private void FixedUpdate()
    {
        if (!(currentState == MonsterState.Kill || player.Dead))
        {
            RaycastHit hit;
            rayDirection = (playerRigidbody.position - (transform.position + Vector3.up)).normalized;
            Physics.Raycast(transform.position + Vector3.up, rayDirection, out hit, Mathf.Infinity);
            Debug.DrawLine(transform.position + Vector3.up, hit.point, Color.cyan);
            inFOV = (Vector3.Dot(rayDirection, transform.rotation * Vector3.forward) >= 0.1);
            seen = hit.transform.gameObject.layer == LayerMask.NameToLayer("Player");

            if (seen && inFOV)
            {
                lastSeenPosition = maze.WorldPosToMazePos(playerTransform.position);
                seenTimer = chaseTime;
            }
            else if (seenTimer > 0)
            {
                lastSeenPosition = maze.WorldPosToMazePos(playerTransform.position);
            }

            seenTimer -= Time.fixedDeltaTime;

            //Debug.Log(seenTimer);

            switch (currentState)
            {
                case MonsterState.Wander:
                    WanderFixedUpdate();
                    break;
                case MonsterState.Chase:
                    ChaseFixedUpdate();
                    break;
                case MonsterState.Hunt:
                    HuntFixedUpdate();
                    break;
            }

            if (previousState != currentState)
            {
                Debug.Log(System.Enum.ToObject(typeof(MonsterState), previousState) + " to " + System.Enum.ToObject(typeof(MonsterState), currentState));

                switch (previousState)
                {
                    case MonsterState.Wander:
                        WanderEnd();
                        break;
                    case MonsterState.Chase:
                        ChaseEnd();
                        break;
                    case MonsterState.Hunt:
                        HuntEnd();
                        break;
                }

                switch (currentState)
                {
                    case MonsterState.Wander:
                        WanderStart();
                        break;
                    case MonsterState.Chase:
                        ChaseStart();
                        break;
                    case MonsterState.Hunt:
                        HuntStart();
                        break;
                }
            }

            previousState = currentState;

            if (!stepping)
            {
                a = maze.WorldPosToMazePos(transform.position);

                switch (currentState)
                {
                    case MonsterState.Wander:
                        WanderStep();
                        break;
                    case MonsterState.Chase:
                        ChaseStep();
                        break;
                    case MonsterState.Hunt:
                        HuntStep();
                        break;
                }

                path = solver.BFS(a, b, 100);

                if (path.Count != 0)
                {
                    StartCoroutine(ChangeCell(a, path[0]));
                    StartCoroutine(TurnEntity(previousDirection, path[0]));
                    previousDirection = path[0];
                }
            }

            animator.SetFloat("Speed", currentSpeed);
            animator.SetInteger("State", (int)currentState);

            if (screamTimer <= 0 && screamAudioSource.enabled)
            {
                screamAudioSource.Stop();
                screamAudioSource.clip = screams[Random.Range(0, screams.Length)];
                screamAudioSource.Play();
                screamTimer = Random.Range(7, 12f);
            }

            if (ambientTimer <= 0 && ambientAudioSource.enabled)
            {
                ambientAudioSource.Stop();
                ambientAudioSource.clip = ambientNoises[Random.Range(0, ambientNoises.Length)];
                ambientAudioSource.Play();
                ambientTimer = Random.Range(15, 40f);
            }

            screamTimer -= Time.fixedDeltaTime;
            ambientTimer -= Time.fixedDeltaTime;

            //Debug.Log(seen + ", " + inFOV);
            //Debug.Log(Vector3.Dot(rayDirection, transform.rotation * Vector3.forward) >= 0);

            solver.DisplayPath(path, a);
        }
    }
    public AnimationClip jumpScare;

    private void Update()
    {
        if (a == maze.WorldPosToMazePos(playerTransform.position))
            currentState = MonsterState.Kill;

        if (currentState == MonsterState.Kill && !player.Dead)
        {
            StartCoroutine(Jumpscare());

            player.Kill();
        }
    }

    public void WanderStep()
    {
        if (a == wanderPos)
        {
            wanderPos = new Vector2Int(Random.Range(0, maze.mazeSize), Random.Range(0, maze.mazeSize));
        }
        b = wanderPos;
    }
    public void ChaseStep()
    {
        b = lastSeenPosition;
        if (a == lastSeenPosition && seenTimer <= 0)
        {
            Debug.Log("Done Chase");
            currentState = MonsterState.Wander;
        }
    }
    public void HuntStep()
    {
        b = maze.WorldPosToMazePos(playerTransform.position);
        huntTimer -= 1;
        if (huntTimer == 0)
        {
            currentState = MonsterState.Wander;
            huntTimer = 50;
        }
    }

    bool huntOppertunityFound = false; //Set to true by tryhunt if it may start a hunt
    public void WanderFixedUpdate()
    {
        if (huntOppertunityFound && !manager.playerEscaped)
        {
            currentState = MonsterState.Hunt;
            huntOppertunityFound = false;
        }
        if (seen && inFOV)
        {
            currentState = MonsterState.Chase;
            StopCoroutine(TryHunt());
        }
        if (seenTimer <= huntBeginTime && !manager.playerEscaped)
        {
            StartCoroutine(TryHunt());
            seenTimer = 0;
            huntBeginTime = chaseTime - Random.Range(minTimeTillHunt, maxTimeTillHunt);
        }
    }
    public void ChaseFixedUpdate()
    {
        if (manager.playerEscaped)
            currentState = MonsterState.Wander;
    }
    public void HuntFixedUpdate()
    {
        if (manager.playerEscaped)
            currentState = MonsterState.Wander;
    }

    public void WanderStart()
    {
        ambientAudioSource.enabled = true;
        screamAudioSource.enabled = false;
        currentSpeed = walkSpeed;
        currentTurnSpeed = walkTurnSpeed;
        //animator.SetBool("Run", false);
    }
    public void ChaseStart()
    {
        Debug.Log("Start Chase");
        ambientAudioSource.enabled = false;
        screamAudioSource.enabled = true;
        currentSpeed = runSpeed;
        currentTurnSpeed = runTurnSpeed;
    }
    public void HuntStart()
    {
        ambientAudioSource.enabled = false;
        screamAudioSource.enabled = true;
        monsterAudioSource.clip = huntSound;
        monsterAudioSource.Play();
        currentSpeed = runSpeed * huntSpeedMultiplier;
        currentTurnSpeed = runTurnSpeed * huntSpeedMultiplier;
        //animator.SetBool("Run", true);
    }

    public void WanderEnd()
    {
        
    }
    public void ChaseEnd()
    {

    }
    public void HuntEnd()
    {
        monsterAudioSource.Stop();
    }

    IEnumerator ChangeCell(Vector2Int pos, Direction dir)
    {
        float time = 0;
        stepping = true;
        Vector3 newPos;

        while (time < 1)
        {
            newPos = maze.MazePosToWorldPos(pos + (Vector2)DirectionToVector(dir) * time);
            transform.position = new Vector3(newPos.x, transform.position.y, newPos.z);
            time += Time.fixedDeltaTime / maze.mazeSpacing * currentSpeed;

            yield return new WaitForFixedUpdate();
        }

        newPos = maze.MazePosToWorldPos(pos + (Vector2)DirectionToVector(dir));
        transform.position = new Vector3(newPos.x, transform.position.y, newPos.z);
        stepping = false;
    }

    IEnumerator TurnEntity(Direction oldDir, Direction dir)
    {
        float lerpValue;
        float time = 0;

        Turn turn = GetTurnDirection(oldDir, dir);
        float degreesToRotate = 0;
        switch (turn)
        {
            case Turn.Left:
                degreesToRotate = -90f;
                break;
            case Turn.Right:
                degreesToRotate = 90f;
                break;
            case Turn.Around:
                Debug.Log("DO A 180");
                degreesToRotate = Random.Range(0, 2) == 0 ? 180f : -180f;
                break;
        }
        Quaternion originalRotation = transform.rotation;
        Quaternion turnRotation = originalRotation * Quaternion.Euler(0, degreesToRotate, 0);

        if (turn != Turn.None)
        {
            while (time < 1)
            {
                lerpValue = 3 * time * time - 2 * time * time * time;
                transform.rotation = Quaternion.Slerp(originalRotation, turnRotation, lerpValue);
                time += Time.fixedDeltaTime * currentTurnSpeed;

                yield return new WaitForFixedUpdate();
            }
            transform.rotation = turnRotation;
        }
    }

    float toleranceTime = 0.2f;
    IEnumerator TryHunt()
    {
        warningAudioSource.clip = warningTryHunt;
        warningAudioSource.Play();
        yield return new WaitForSecondsRealtime(warningTryHunt.length + toleranceTime);
        float time = 0;
        warningAudioSource.clip = sonar;
        warningAudioSource.Play();
        while (time <= sonar.length)
        {
            if (Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0 || playerController.justTripped || playerController.standingUp)
                huntOppertunityFound = true;

            time += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }

    //JUMPSCARE STUFFF WIP
    IEnumerator Jumpscare()
    {
        monsterAudioSource.Stop();
        jumpscareCamera.gameObject.SetActive(true);
        animator.SetBool("Jumpscare", true);

        yield return new WaitForSeconds(2.0835f);

        StartCoroutine(player.ShowDeathScreen());
        jumpscareCamera.gameObject.SetActive(false);
    }
}
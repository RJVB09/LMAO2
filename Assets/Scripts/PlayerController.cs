using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Player))]
public class PlayerController : MonoBehaviour
{
    Player player;
    public Camera playerCamera;
    public Transform head;
    public float walkspeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 7f;
    public float gravity = 10f;


    public float sensitivity = 2f;
    public float lookXLimit = 45f;


    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    public bool canMove = true;

    CharacterController characterController;
    AudioSource audioSource;
    public new Rigidbody rigidbody;
    public AudioClip[] trippingSounds;

    float verticalWobblePos = 0;
    float horizontalWobblePos = 0;

    [HideInInspector]
    public bool tripped = false;
    [HideInInspector]
    public bool justTripped = false;
    [HideInInspector]
    public bool standingUp = false;
    [HideInInspector]
    public bool allowedToStandUp = true;

    public bool trip;

    public float standUpDuration = 1f;
    public float tripTime = 2f;

    private void Start()
    {
        player = GetComponent<Player>();
        characterController = GetComponent<CharacterController>();
        audioSource = rigidbody.gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        interactionCircle = interactionUI.GetComponent<Image>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    Vector3 oldPos = Vector3.zero;
    Vector3 oldVel = Vector3.zero;
    Vector3 oldAcc = Vector3.zero;
    [HideInInspector]
    public Vector3 velocity = Vector3.zero;
    [HideInInspector]
    public Vector3 acceleration = Vector3.zero;
    [HideInInspector]
    public Vector3 jerk = Vector3.zero;

    public float reachDistance = 4f;

    public GameObject interactionUI;
    Image interactionCircle;

    InteractableObject lastInteractableObject;
    private void Update()
    {
        if (Physics.Raycast(head.position, head.rotation * Vector3.forward, out RaycastHit hit, reachDistance, ~LayerMask.GetMask("Player")))
        {
            Debug.DrawLine(head.position, head.position + head.rotation * Vector3.forward * hit.distance, Color.yellow);

            if (hit.transform != null)
            {
                bool interactableFound = hit.transform.TryGetComponent(out InteractableObject interactableObject);
                if (interactableFound && !interactableObject.interactable)
                {
                    interactableFound = false;
                }

                // Try to get the InteractableObject component from the hit object
                if (interactableFound)
                {
                    interactionUI.SetActive(true);
                    interactionCircle.fillAmount = 0;

                    // Check if this is a new interactable object
                    if (lastInteractableObject != interactableObject)
                    {
                        // If the player was interacting with another object, trigger OnInteractUp() for it
                        if (lastInteractableObject != null && Controls.GetButton("Interact"))
                        {
                            lastInteractableObject.OnInteractUp();
                        }

                        // Update the last interactable object
                        lastInteractableObject = interactableObject;

                        // If E is being held, trigger OnInteractDown() immediately
                        if (Controls.GetButton("Interact"))
                        {
                            interactableObject.OnInteractDown();
                        }
                    }

                    // Handle interaction input
                    if (Controls.GetButtonDown("Interact"))
                    {
                        interactableObject.OnInteractDown();
                    }

                    if (Controls.GetButton("Interact"))
                    {
                        interactableObject.OnInteractHold();
                        interactionCircle.fillAmount = interactableObject.timeHeldFor / interactableObject.interactionTime;
                    }

                    // Do not call OnInteractUp() here anymore, it will be handled when the player looks away
                }
                else
                {
                    interactionUI.SetActive(false);
                    interactionCircle.fillAmount = 0;
                    // If no interactable object is hit and the player is holding E, trigger OnInteractUp()
                    if (lastInteractableObject != null && Controls.GetButton("Interact"))
                    {
                        lastInteractableObject.OnInteractUp();
                        lastInteractableObject = null; // Reset the last interactable object
                    }
                }
            }
        }
        else
        {
            interactionUI.SetActive(false);
            interactionCircle.fillAmount = 0;
            // If nothing is hit and the player is holding E, trigger OnInteractUp()
            if (lastInteractableObject != null && Controls.GetButton("Interact"))
            {
                lastInteractableObject.OnInteractUp();
                lastInteractableObject = null; // Reset the last interactable object
            }
        }

        // Reset the last interactable object if E is released
        if (Controls.GetButtonUp("Interact") && lastInteractableObject != null)
        {
            lastInteractableObject.OnInteractUp();
            lastInteractableObject = null;
        }

    }

    private void FixedUpdate()
    {


        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? Controls.GetMovement("Vertical") * (isRunning ? runSpeed : walkspeed) : 0;
        float curSpeedY = canMove ? Controls.GetMovement("Horizontal") * (isRunning ? runSpeed : walkspeed) : 0;
        Vector2 normalization = curSpeedX == 0 && curSpeedY == 0 ? Vector2.zero : new Vector2(Mathf.Abs(Controls.GetMovement("Vertical")), Mathf.Abs(Controls.GetMovement("Horizontal"))).normalized;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX * normalization.x) + (right * curSpeedY * normalization.y);

        float speed = moveDirection.magnitude;

        jerk = (acceleration - oldAcc) / Time.fixedDeltaTime;
        oldAcc = acceleration;

        acceleration = (velocity - oldVel) / Time.fixedDeltaTime;
        oldVel = velocity;

        velocity = (head.position - oldPos) / Time.fixedDeltaTime;
        oldPos = head.position;


        if ((Controls.GetButton("Jump") || /* Input.GetKeyDown(KeyCode.Q) ||*/ trip) && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.fixedDeltaTime;
        }

        if (characterController.enabled)
            characterController.Move(moveDirection * Time.fixedDeltaTime);

        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * sensitivity;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            head.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * sensitivity, 0);
        }

        verticalWobblePos += Time.fixedDeltaTime * 20 * Mathf.Pow(speed / runSpeed, 1.5f);
        horizontalWobblePos += Time.fixedDeltaTime * 6 * Mathf.Pow(speed / runSpeed, 4f);

        playerCamera.transform.localPosition = Vector3.zero 
            + Vector3.up * (Mathf.Sin(verticalWobblePos) * (1-Mathf.Exp(-6 * speed / runSpeed))) * 0.05f
            + Vector3.right * (Mathf.Lerp(-1f, 1f, Mathf.PerlinNoise(horizontalWobblePos, 0)) * (1 - Mathf.Exp(-6 * speed / runSpeed))) * 0.03f
            + Vector3.up * (Mathf.Sin(Time.time * 2)) * 0.025f;
        playerCamera.transform.localRotation = Quaternion.Euler(0, 0, -3 * curSpeedY / runSpeed);

        if ((/*Input.GetKeyDown(KeyCode.Q) ||*/ trip) && !tripped && !justTripped)
            StartCoroutine(Trip(true));

        if (Controls.GetButton("Jump") && tripped && !justTripped && !standingUp && allowedToStandUp)
            StartCoroutine(Recover());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("TrippingHazard") && !tripped && !justTripped && velocity.magnitude > walkspeed * 0.9f)
        {
            trip = true;
        }
    }

    public IEnumerator Trip(bool tripSound)
    {
        yield return new WaitForFixedUpdate();
        characterController.enabled = false;
        justTripped = true;
        canMove = false;
        rigidbody.constraints = RigidbodyConstraints.None;
        rigidbody.AddForce(moveDirection * 2f, ForceMode.VelocityChange);
        Vector3 torque = Random.onUnitSphere;
        torque = new Vector3(torque.x, 0, torque.z).normalized;
        rigidbody.AddTorque(torque * 30f, ForceMode.VelocityChange);

        trip = false;

        if (trippingSounds.Length != 0 && tripSound)
        {
            audioSource.clip = trippingSounds[Random.Range(0, trippingSounds.Length)];
            audioSource.Play();
        }

        yield return new WaitForSecondsRealtime(tripTime);

        tripped = true;
        justTripped = false;
    }

    public IEnumerator Recover()
    {
        float lerpValue = 0;
        float time = 0;
        standingUp = true;

        RaycastHit hit;
        Physics.Raycast(rigidbody.position, Vector3.down, out hit, Mathf.Infinity, ~LayerMask.GetMask("Player"));

        float differenceToStanding = 0.5f * characterController.height - hit.distance;

        rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        Vector3 oldPos = rigidbody.transform.localPosition;
        Quaternion oldRot = rigidbody.transform.localRotation;
        Vector3 destPos = new Vector3(rigidbody.transform.localPosition.x, rigidbody.transform.localPosition.y + differenceToStanding, rigidbody.transform.localPosition.z);
        Quaternion destRot = Quaternion.Euler(0, 0, 0);
        //transform.position = new Vector3(rigidbody.transform.position.x, transform.position.y, rigidbody.transform.position.z);
        //rigidbody.transform.localPosition = -(oldPos - transform.position) / tran;
        //rigidbody.transform.localRotation = Quaternion.Euler(0,0,0);
        //canMove = true;
        while (time < 1)
        {
            lerpValue = 3 * time * time - 2 * time * time * time;
            rigidbody.transform.localPosition = new Vector3(Mathf.Lerp(oldPos.x, destPos.x, lerpValue), Mathf.Lerp(oldPos.y, destPos.y, lerpValue), Mathf.Lerp(oldPos.z, destPos.z, lerpValue));
            rigidbody.transform.localRotation = new Quaternion(Mathf.Lerp(oldRot.x, destRot.x, lerpValue), Mathf.Lerp(oldRot.y, destRot.y, lerpValue), Mathf.Lerp(oldRot.z, destRot.z, lerpValue), Mathf.Lerp(oldRot.w, destRot.w, lerpValue));
            time += Time.fixedDeltaTime / standUpDuration;
            yield return new WaitForFixedUpdate();
        }

        //transform.position = new Vector3(rigidbody.transform.position.x, transform.position.y, rigidbody.transform.position.z);
        Debug.Log(rigidbody.transform.position);
        
        transform.position = rigidbody.transform.position;
        characterController.enabled = true;

        rigidbody.transform.localPosition = Vector3.zero;
        rigidbody.transform.localRotation = destRot;

        tripped = false;
        standingUp = false;
        canMove = true;
        player.playerUI.SetActive(true);
    }
}

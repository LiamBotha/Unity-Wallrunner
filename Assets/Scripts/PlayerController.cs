using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private Animator anim;
    private GameObject cameraM;

    private WallRunManager wallRunManager;
    private CameraLook cameraLook;

    private bool jumpRequest = false;
    private bool grounded = true;

    private float playerHeight;
    private Vector3 boxSize;
    private Transform currentCheckpoint;

    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private float playerSpeed = 1;
    [SerializeField] private float jumpHeight = 5;

    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;

    public bool canMove = true;

    public bool Grounded { get => grounded; }
    public float PlayerHeight { get => playerHeight;}
    public Transform CurrentCheckpoint { get => currentCheckpoint; set => currentCheckpoint = value; }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        cameraM = Camera.main.gameObject;

        wallRunManager = GetComponent<WallRunManager>();
        cameraLook = FindObjectOfType<CameraLook>();

        playerHeight = (GetComponent<CapsuleCollider>().height / 2.02f) * transform.localScale.y; // TODO - Change to whatever collider used in the final version
        boxSize = new Vector3(0.3f, 0.045f, 0.3f); // TODO - replace number with variable
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove && Input.GetKeyDown(KeyCode.Space) && Grounded)
        {
            jumpRequest = true;
        }
    }

    private void FixedUpdate()
    {
        if (!grounded)
            PlayerFalling();

        if (jumpRequest && canMove)
        {
            wallRunManager.StartedRegJump = true;
            StartCoroutine(wallRunManager.JumpCooldown(0.5f));

            rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);

            jumpRequest = false;
            grounded = false;
        }
        else
        {
            Vector3 boxCenter = transform.position + (Vector3.down * playerHeight);

            grounded = (Physics.OverlapBox(boxCenter, boxSize, transform.rotation, groundLayer,QueryTriggerInteraction.Ignore).Length > 0); // I have disabled triggers affecting raycasts in unity

            if (grounded)
                wallRunManager.StartedRegJump = false;

            ExtDebug.DrawBox(boxCenter, boxSize, transform.rotation, Color.red);
        }


        PlayerMovement();
    }

    private void PlayerMovement()
    {
        if(canMove)
        {
            Vector3 inputDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            var movement = inputDir * playerSpeed * Time.fixedDeltaTime;
            //transform.Translate(movement, Space.Self);

            rb.MovePosition(rb.position + transform.TransformDirection(movement)); // maybe replace with add force for momentum

            if (inputDir != Vector3.zero)
            {
                //anim.SetBool("isWalking", true);
            }
        }
    }

    private void PlayerFalling()
    {
        if (rb.velocity.y < 0 && !wallRunManager.IsOnWall)
        {
            //rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
            rb.AddForce(Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime, ForceMode.Impulse);
        }
        else if (rb.velocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            //rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
            rb.AddForce(Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime, ForceMode.Impulse);
        }
    } // Handles player falling and small jumps

    private void OnDrawGizmos()
    {
        Vector3 boxCenter = transform.position + (Vector3.down * playerHeight);

        Gizmos.DrawCube(boxCenter, boxSize);
    }

    public void Death()
    {
        transform.position = currentCheckpoint.position;
        //transform.LookAt(currentCheckpoint); TODO - add function to cameralook to look at a spesific location
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunManager : MonoBehaviour
{
    Rigidbody rb;

    public float wallRunSpeed = 1;
    public float wallDist = 1.1f;
    
    public int minSideAngle = 60;
    public int maxSideAngle = 80;

    private bool wallJumpRequest = false;
    private bool wallRunRequest = false;
    private bool startedWallRun = false;

    private GameObject CurrentWall;
    private Vector3 wallNormal;
    private PlayerController playerController;

    private CameraLook cameraLook;

    private ParkourState parkourState = ParkourState.NONE;

    public LayerMask wallLayer;

    private Coroutine countdown;

    public bool IsOnWall { get; private set; } = false;
    public bool StartedRegJump { get; set; } = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerController = GetComponent<PlayerController>();
        cameraLook = FindObjectOfType<CameraLook>();
    }

    private void Update()
    {
        if(Input.GetKey(KeyCode.Space) && IsOnWall)
        {
            wallRunRequest = true;
        }
        else
        {
            wallRunRequest = false;
        }

        if (Input.GetKeyUp(KeyCode.Space) && IsOnWall)
        {
            wallJumpRequest = true;
        }
        else if (Input.GetKeyUp(KeyCode.Space) && StartedRegJump)
        {
            StartedRegJump = false;
        }
    }

    private void FixedUpdate()
    {
        if(playerController.Grounded)
        {
            wallNormal = Vector3.zero;
            parkourState = ParkourState.NONE;
        }

        if (wallJumpRequest)
        {
            wallJumpRequest = false;

            WallJump();
        }

        if (wallRunRequest)
        {
            HandleWallRun();
        }
        else
        {
            parkourState = ParkourState.NONE;
            EndWallRun();
        }
    }

    private void HandleParkour(Collision collision, ContactPoint hit)
    {
        if (hit.normal != wallNormal && (hit.normal.x != 0 || hit.normal.z != 0)) // wallclimb
        {
            var wallAngle = Vector3.Angle(Vector3.up, hit.normal);
            var playerAngle = Vector3.Angle(transform.forward, -hit.normal);

            if (playerAngle > -25 && playerAngle < 25)
            {
                if (Physics.Raycast(transform.position + (Vector3.up * 0.01f), transform.forward, 1, wallLayer, QueryTriggerInteraction.Ignore))
                {
                    StartParkour(collision, hit, ParkourState.CLIMB);

                    cameraLook.startRot = (int)Quaternion.LookRotation(-wallNormal.normalized).eulerAngles.y;

                    Debug.Log(cameraLook.startRot + " StartRot");
                    cameraLook.wallRunningMode = true;
                }
                else if (!Physics.Raycast(transform.position + (Vector3.up * 2), transform.forward, 2, wallLayer, QueryTriggerInteraction.Ignore))
                {
                    Debug.DrawRay(transform.position + (Vector3.up * 2) + (transform.forward), Vector3.down, Color.red);
                    if (Physics.Raycast(transform.position + (transform.forward), Vector3.down, 2.5f, wallLayer, QueryTriggerInteraction.Ignore))
                    {
                        transform.position = new Vector3(hit.point.x, hit.point.y + 1, hit.point.z);

                        Debug.Log("Climbed");

                        parkourState = ParkourState.NONE;
                    }
                }
            }
            else if (wallAngle > 80 && wallAngle < 110) //wallRun
            {
                if(playerAngle > minSideAngle && playerAngle < maxSideAngle)
                {
                    StartParkour(collision, hit, ParkourState.RUN);

                    bool right = RaycastSides();

                    if (right)
                        cameraLook.startRot = Quaternion.LookRotation(Vector3.Cross(Vector3.up, wallNormal)).eulerAngles.y;
                    else
                        cameraLook.startRot = Quaternion.LookRotation(-Vector3.Cross(Vector3.up, wallNormal)).eulerAngles.y;

                    cameraLook.wallRunningMode = true;
                }
            }
        }
    }

    private void HandleParkour(GameObject nearestWall, RaycastHit hit)
    {
        if (hit.normal != wallNormal && (hit.normal.x != 0 || hit.normal.z != 0)) // wallclimb
        {
            var wallAngle = Vector3.Angle(Vector3.up, hit.normal);
            var playerAngle = Vector3.Angle(transform.forward, -hit.normal);

            if (playerAngle > -25 && playerAngle < 25)
            {
                if (Physics.Raycast(transform.position + (Vector3.up * 0.01f), transform.forward, 1, wallLayer, QueryTriggerInteraction.Ignore)) // climbing
                {
                    StartParkour(nearestWall, hit.normal, ParkourState.CLIMB);

                    cameraLook.startRot = (int)Quaternion.LookRotation(-wallNormal.normalized).eulerAngles.y;

                    Debug.Log(cameraLook.startRot + " StartRot");
                    cameraLook.wallRunningMode = true;
                }
                else if (!Physics.Raycast(transform.position + (Vector3.up * 2), transform.forward, 2, wallLayer, QueryTriggerInteraction.Ignore)) // top of ledge
                {
                    Debug.DrawRay(transform.position + (Vector3.up * 2) + (transform.forward), Vector3.down, Color.red);
                    if (Physics.Raycast(transform.position + (transform.forward), Vector3.down, 2.5f, wallLayer, QueryTriggerInteraction.Ignore)) // floor on top of ledge
                    {
                        transform.position = new Vector3(hit.point.x, hit.point.y + 1, hit.point.z);

                        Debug.Log("Climbed");

                        parkourState = ParkourState.NONE;
                    }
                }
            }
            else if (wallAngle > 80 && wallAngle < 110) //wallRun
            {
                if (playerAngle > minSideAngle && playerAngle < maxSideAngle)
                {
                    StartParkour(nearestWall, hit.normal, ParkourState.RUN);

                    bool right = RaycastSides();

                    if (right)
                        cameraLook.startRot = Quaternion.LookRotation(Vector3.Cross(Vector3.up, wallNormal)).eulerAngles.y;
                    else
                        cameraLook.startRot = Quaternion.LookRotation(-Vector3.Cross(Vector3.up, wallNormal)).eulerAngles.y;

                    cameraLook.wallRunningMode = true;
                }
            }
        }
    }

    private void StartParkour(Collision collision, ContactPoint hit, ParkourState newState)
    {
        CurrentWall = collision.gameObject;
        wallNormal = hit.normal;

        startedWallRun = true;
        IsOnWall = true;
        parkourState = newState;

        rb.useGravity = false;
        playerController.canMove = false;

        rb.velocity = Vector3.zero;
    }

    private void StartParkour(GameObject nearestWall, Vector3 hitNormal, ParkourState newState)
    {
        CurrentWall = nearestWall;
        wallNormal = hitNormal;

        startedWallRun = true;
        IsOnWall = true;
        parkourState = newState;

        rb.useGravity = false;
        playerController.canMove = false;

        rb.velocity = Vector3.zero;

        Debug.Log("Started Running");
    }

    private void HandleWallRun()
    {
        Debug.Log("In Handle Wall Run");

        bool isRight = true;

        if (CurrentWall != null)
        {
            isRight = RaycastSides();
        }

        if (IsOnWall && parkourState == ParkourState.RUN)
        {
            if (startedWallRun)
            {
                if (countdown != null)
                    StopCoroutine(countdown);
                countdown = StartCoroutine(Countdown(1.25f));
            }

            WallRun(isRight);
        }
        else if (IsOnWall && parkourState == ParkourState.CLIMB)
        {
            if (startedWallRun)
            {
                if (countdown != null)
                    StopCoroutine(countdown);
                countdown = StartCoroutine(Countdown(1.25f)); //TODO - Transfer into climb upon hitting wall
            }

            WallClimb();
        }

        if (parkourState == ParkourState.NONE)
        {
            EndWallRun();
        }
    }

    private void WallClimb()
    {
        rb.MovePosition(transform.position + (Vector3.up * (wallRunSpeed / 1.9f) * Time.deltaTime) + (transform.up * 0.4f * Time.deltaTime));
    }

    private void WallJump()
    {
        var wallDir = wallNormal;

        EndWallRun();

        Vector3 jumpDir = Vector3.zero;
        if (parkourState == ParkourState.RUN)
            jumpDir = (transform.forward * 5) + (Vector3.up * 5) + (wallDir * 5);
        else if (parkourState == ParkourState.CLIMB)
            jumpDir = (wallDir * 5) + (Vector3.up * 10);

        rb.AddForce(jumpDir, ForceMode.Impulse);

        parkourState = ParkourState.NONE;
    }

    private void WallRun(bool isRight)
    {
        Vector3 runDir;
        if (isRight)
        {
            runDir = Vector3.Cross(Vector3.up, wallNormal);
        }
        else
        {
            runDir = -Vector3.Cross(Vector3.up, wallNormal);
        }

        rb.MovePosition(transform.position + (runDir * wallRunSpeed * Time.deltaTime) + (transform.up * 0.4f * Time.deltaTime));
    }

    private bool RaycastSides()
    {
        RaycastHit hit;
        bool isRight = true;

        Vector3 wallDirection = Vector3.RotateTowards(-wallNormal, CurrentWall.transform.position, 0, 0);

        Debug.DrawLine(transform.position, transform.position + (wallDirection * wallDist));

        if (Physics.Raycast(transform.position, wallDirection, out hit, wallDist, wallLayer, QueryTriggerInteraction.Ignore))
        {

        }
        else if(Physics.Raycast(transform.position, transform.forward, out hit, wallDist, wallLayer, QueryTriggerInteraction.Ignore))
        {

        }
        else
        {
            if(parkourState == ParkourState.CLIMB)
            {

                if(!Physics.Raycast(transform.position + (Vector3.up * 2), transform.forward, out hit, 2, wallLayer, QueryTriggerInteraction.Ignore))
                {
                    Debug.DrawRay(transform.position + (Vector3.up * 2) + (transform.forward), Vector3.down, Color.red);
                    if (Physics.Raycast(transform.position + (transform.forward * 2f), Vector3.down, out hit, playerController.PlayerHeight / 2.5f, wallLayer, QueryTriggerInteraction.Ignore))
                    {
                        transform.position = new Vector3(hit.point.x, hit.point.y + (playerController.PlayerHeight / 2.5f), hit.point.z);

                        Debug.Log("Climbed");

                        parkourState = ParkourState.NONE;
                    }
                }
            }

            parkourState = ParkourState.NONE;
        }

        float crossed = Vector3.Cross(transform.forward, wallNormal).y;

        if ( crossed > 0)
        {
            isRight = false;
        }

        return isRight;
    }

    private void EndWallRun()
    {
        StopCoroutine("Countdown");

        IsOnWall = false;
        CurrentWall = null;
        rb.useGravity = true;
        playerController.canMove = true;
        cameraLook.wallRunningMode = false;
        wallRunRequest = false;
    }

    private IEnumerator Countdown(float time)
    {
        startedWallRun = false;

        rb.useGravity = false;

        yield return new WaitForSeconds(time);

        rb.useGravity = true;
    } // TODO - falling too fast after multiple runs

    public IEnumerator JumpCooldown(float time)
    {
        yield return new WaitForSeconds(time);

        StartedRegJump = false;

        Debug.Log("Cooldown");
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    ContactPoint[] contacts = new ContactPoint[10];
    //    collision.GetContacts(contacts);

    //    foreach(var hit in contacts)
    //    {
    //        HandleParkour(collision, hit);
    //    }
    //}

    private void OnTriggerStay(Collider other)
    {
        if(!StartedRegJump)
        {
            var point = other.ClosestPoint(transform.position);

            ExtDebug.DrawBox(point, Vector3.one / 2, Quaternion.identity, Color.blue);

            var dir = point - transform.position;
            RaycastHit hit;

            Debug.DrawLine(transform.position, point, Color.white);

            if (Physics.Raycast(transform.position, dir, out hit, wallDist, wallLayer, QueryTriggerInteraction.Ignore) && !playerController.Grounded)
            {
                Debug.DrawRay(point, hit.normal, Color.green);

                HandleParkour(other.gameObject, hit);
            }
        }
    }

    private void OnDrawGizmos()
    {
        float rayDist = 2f;
        float forwardAngle = 25f;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * rayDist);

        //Gizmos.color = Color.yellow;
        //Vector3 dir = Quaternion.AngleAxis(forwardAngle, Vector3.up) * transform.forward;
        //Vector3 dir2 = Quaternion.AngleAxis(-forwardAngle, Vector3.up) * transform.forward;
        //Gizmos.DrawRay(transform.position, dir * rayDist / 2);
        //Gizmos.DrawRay(transform.position, dir2 * rayDist / 2);

        //Gizmos.color = Color.magenta;
        //Vector3 dir3 = Quaternion.AngleAxis(minSideAngle, Vector3.up) * transform.forward;
        //Vector3 dir4 = Quaternion.AngleAxis(maxSideAngle, Vector3.up) * transform.forward;
        //Gizmos.DrawRay(transform.position, dir3 * rayDist);
        //Gizmos.DrawRay(transform.position, dir4 * rayDist);

        //Gizmos.color = Color.cyan;
        //Vector3 dir5 = Quaternion.AngleAxis(-minSideAngle, Vector3.up) * transform.forward;
        //Vector3 dir6 = Quaternion.AngleAxis(-maxSideAngle, Vector3.up) * transform.forward;
        //Gizmos.DrawRay(transform.position, dir5 * rayDist);
        //Gizmos.DrawRay(transform.position, dir6 * rayDist);

        Gizmos.color = Color.blue;
        if(wallNormal != null)
            Gizmos.DrawRay(transform.position, wallNormal * rayDist);
    }
}

public enum ParkourState
{
    NONE,RUN,CLIMB
}
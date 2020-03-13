using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CameraLook : MonoBehaviour
{
    [SerializeField]
    public float sensitivity = 5.0f;
    //[SerializeField]
    //public float smoothing = 2.0f;
    // the chacter is the capsule
    public GameObject character;
    // get the incremental value of mouse moving
    //private Vector2 mouseLook;
    // smooth the mouse moving
    //private Vector2 smoothV;

    [NonSerialized]
    public bool wallRunningMode = false;

    private float minimumX = -60;
    private float maximumX = 60;
    private float minimumY = -80;
    private float maximumY = 80;

    private float rotX = 0;
    private float rotY = 0;
    
    public float startRot = 0;

    Rigidbody rb;

    Quaternion originalRotation;

    // Use this for initialization
    void Start()
    {
        character = transform.parent.parent.gameObject;
        rb = GetComponent<Rigidbody>();

        if (rb)
            rb.freezeRotation = true;

        originalRotation = transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        rotX += Input.GetAxisRaw("Mouse Y") * sensitivity;
        rotY += Input.GetAxisRaw("Mouse X")  * sensitivity;

        rotX = Mathf.Clamp(rotX, minimumX, maximumX);

        if (wallRunningMode)
        {

            float minDist = Mathf.DeltaAngle(startRot + minimumY, rotY);
            float maxDist = Mathf.DeltaAngle(rotY, startRot + maximumY);

            if(minDist < 0)
            {
                rotY = startRot + minimumY;
            }
            else if(maxDist < 0)
            {
                rotY = startRot + maximumY;
            }
        }

        rotY = mod(rotY, 360);

        Quaternion yQuaternion = Quaternion.AngleAxis(rotY, Vector3.up);
        character.transform.localRotation = originalRotation * yQuaternion;

        //character.transform.localRotation = Quaternion.Euler(0, rotY, 0);
        transform.localEulerAngles = new Vector3(-rotX, 0, 0);
    }

    float ClampAngle(float angle, float min)
    {
        float max = min + 160;

        angle = mod(angle, 360);


        if(min < 0 && angle > max)
        {
            min = mod(min, 360);

            max = min + 160;

            Debug.Log(min + " min, " + max + " max, " + angle + " angle --");

            angle += 360;
        }
        else
        {
            min = mod(min, 360);

            max = min + 160;

            Debug.Log(min + " min, " + max + " max, " + angle + " angle ++");
        }

        if (angle > max && angle < min)
        {
            return angle;
        }
        else if(angle > max)
        {
            return max;
        }
        else if(angle < min)
        {
            return min;
        }

        return angle;
    }

    float mod(float x, float m)
    {
        return (x % m + m) % m;
    }
}

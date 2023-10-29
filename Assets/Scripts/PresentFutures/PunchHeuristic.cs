using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PunchHeuristic : MonoBehaviour
{
    /// <summary>
    /// Which direction is considered "forward" on the hand game object (direction of punch, where the knuckles are).
    /// </summary>
    [Tooltip("Which direction is considered 'forward' on the hand game object (direction of punch, where the knuckles are).")]
    public GameObject FrontalDirection = null;

    /// <summary>
    /// Angular threshold (in degrees) between accepting a straight punch as a 'punch' and a 'slap' (lower is more strickt).
    /// </summary>
    [Tooltip("Angular threshold (in degrees) between accepting a straight punch as a 'punch' and a 'slap' (lower is more strickt).")]
    public float AngleThresholdStraight = 45;

    /// <summary>
    /// Angular threshold (in degrees) between accepting an uppercut as a 'punch' and a 'slap' (lower is more strickt).
    /// </summary>
    [Tooltip("Angular threshold (in degrees) between accepting an uppercut as a 'punch' and a 'slap' (lower is more strickt).")]
    public float AngleThresholdUppercut = 20;

    /// <summary>
    /// Angular threshold (in degrees) between accepting a body blow as a 'punch' and a 'slap' (lower is more strickt).
    /// </summary>
    [Tooltip("Angular threshold (in degrees) between accepting a body blow as a 'punch' and a 'slap' (lower is more strickt).")]
    public float AngleThresholdBodyblow = 20;

    /// <summary>
    /// Angular threshold (in degrees) between accepting a hook as a 'punch' and a 'slap' (lower is more strickt).
    /// </summary>
    [Tooltip("Angular threshold (in degrees) between accepting a hook as a 'punch' and a 'slap' (lower is more strickt).")]
    public float AngleThresholdHook = 20;

    private List<Vector3> LastPositions = new List<Vector3>();

    void Update()
    {
        LastPositions.Add(this.transform.position);
        if (LastPositions.Count > 3) {
            LastPositions.RemoveRange(0, LastPositions.Count - 3);
        }
    }

    public bool ProcessCollision() //Collider collider
    {
    Vector3 motionDirection = Vector3.zero;
    foreach (Vector3 p in this.LastPositions) 
    {
        motionDirection += p;
    }
    motionDirection /= (float)this.LastPositions.Count;
    motionDirection = this.transform.position - motionDirection;
    motionDirection.Normalize();

    Vector3 fistDirection = (this.FrontalDirection != null)
        ? this.FrontalDirection.transform.position - this.transform.position
        : this.transform.forward;
    fistDirection.Normalize();

    Vector3 front = this.transform.position - Player.Instance.head.transform.position;
    front.y = 0;
    front.Normalize();
    Vector3 up = Vector3.up;
    Vector3 right = Vector3.Cross(up, front);
    Matrix4x4 frameOfReference = new Matrix4x4(right, up, front, new Vector4(0, 0, 0, 1)).inverse;

    motionDirection = frameOfReference * motionDirection;
    fistDirection = frameOfReference * fistDirection;
    Vector3 fistPosition = frameOfReference * (this.transform.position - Player.Instance.head.transform.position);
    float angle = Vector3.Angle(motionDirection, fistDirection);

    string msg = "";

    Vector3 motionDirAbs = new Vector3(
        Mathf.Abs(motionDirection.x),
        Mathf.Abs(motionDirection.y),
        Mathf.Abs(motionDirection.z)
    );

    // Assume that the motion is invalid
    bool validMotion = false;

    if (motionDirAbs.y > motionDirAbs.x && motionDirAbs.y > motionDirAbs.z) // vertical motion
    { 
        if (angle <= this.AngleThresholdUppercut)
        {
            msg += "\n-> Uppercut";
            validMotion = true;
        }
        else
        {
            msg += "\n-> invalid (slapping) motion!";
        }
    } 
    else if (motionDirAbs.x > motionDirAbs.z) // horizontal sideways motion
    { 
        if (angle <= this.AngleThresholdHook)
        {
            msg += "\n-> Hook";
            validMotion = true;
        }
        else
        {
            msg += "\n-> invalid (slapping) motion!";
        }
    } 
    else // forward motion
    { 
        if (fistPosition.y < -0.3f) // more than 30cm below the players head
        { 
            if (angle <= this.AngleThresholdBodyblow)
            {
                msg += "\n-> Body blow";
                validMotion = true;
            }
            else
            {
                msg += "\n-> invalid (slapping) motion!";
            }
        } 
        else 
        {
            if (angle <= this.AngleThresholdStraight)
            {
                msg += "\n-> Straight";
                validMotion = true;
            }
            else
            {
                msg += "\n-> invalid (slapping) motion!";
            }
        }
    }

        Debug.Log(msg);
        Player.Instance.punchDebugText.text = msg;

        return validMotion;
    }
 }

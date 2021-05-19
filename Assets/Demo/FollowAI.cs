using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowAI : MonoBehaviour
{
    Animator animator;
    Transform mTarget;
    Vector3 mTargetPos;
    float mTargetEulerY;
    Vector3 mOffset;

    float walkSpeed = 0.3f;
    float runSpeed = 0.6f;
    float rotationSpeed = 5f;

    public Transform target
    {
        get { return mTarget; }
        set {
            mTarget = value;
            if (mTarget != null) {
                mTargetPos = mTarget.position;
                mTargetEulerY = mTarget.eulerAngles.y;
                Debug.Log(mTargetPos);
                Debug.Log(mTargetEulerY);
            }
        }
    }

    public Vector3 offset
    {
        get { return mOffset; }
        set { mOffset = value; }
    }

    // public void SetTarget(Transform target, Vector3 offset = new Vector3()) {
    //     mTarget = target;
    //     mOffset = offset;
    // }

    // Use this for initialization
    void Start () {
        animator = GetComponent<Animator>();
        // SetTarget(GameObject.Find("Capsule").transform, new Vector3(0f, 0f, -1f));
        target = GameObject.Find("Capsule").transform;
        mOffset = new Vector3(0f, 0f, -1f);
    }

    void RandomIdle() {
        var r = Random.value;
        if (r < 0.002f) {
            animator.SetTrigger("triggerIdleB");
        } else if (r < 0.004) {
            animator.SetTrigger("triggerIdleC");
        } else {
            animator.SetFloat("dis", 0.001f);
        }
    }

    // Update is called once per frame
    void Update () {
        if (mTarget == null) {
            animator.SetFloat("dis", 0.001f);
            RandomIdle();
            return;
        }

        // var offsetPosition = mTarget.TransformPoint(mOffset);
        var offsetPosition = Quaternion.AngleAxis(mTargetEulerY, Vector3.up) * mOffset + mTargetPos;
        offsetPosition.y = transform.position.y;
        var targetPosition = mTargetPos;
        targetPosition.y = transform.position.y;
        var dis = Vector3.Distance(transform.position, offsetPosition);
        animator.SetFloat("dis", dis);
        // Debug.Log(dis);

        if (dis <= 0.01) {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetPosition - transform.position), rotationSpeed * Time.deltaTime);
            RandomIdle();
            return;
        }

        //rotate to look at the player
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(offsetPosition - transform.position), rotationSpeed * Time.deltaTime);

         //move towards the player
        if (dis > 1.0f)
            transform.position += transform.forward * Time.deltaTime * runSpeed;
        else
            transform.position += transform.forward * Time.deltaTime * walkSpeed;
    }
}

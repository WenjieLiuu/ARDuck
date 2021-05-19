using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation.Samples;

public class FlyAI : MonoBehaviour
{
    [SerializeField]
    HumanBodyTracker mHumanBodyTracker;
    public HumanBodyTracker humanBodyTracker
    {
        get { return mHumanBodyTracker; }
        set { mHumanBodyTracker = value; }
    }

    public Vector3 startOffsetPos;

    float flySpeed = 0.6f;
    float rotationSpeed = 5f;
    Vector3 scaleSpeed = new Vector3(0.0002f, 0.0002f, 0.0002f);

    enum State
    {
        Flying,
        TouchDown,
        FlyOut,
    }

    State state = State.Flying;
    Animator animator;
    Vector3 startPos;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        HumanBodyTracker.m_leftArmUp += OnLeftArmUp;
        HumanBodyTracker.m_rightArmUp += OnRightArmUp;
        HumanBodyTracker.m_leftArmDown += OnLeftArmDown;
        HumanBodyTracker.m_rightArmDown += OnRightArmDown;
    }

    void OnDisable()
    {
        HumanBodyTracker.m_leftArmUp -= OnLeftArmUp;
        HumanBodyTracker.m_rightArmUp -= OnRightArmUp;
        HumanBodyTracker.m_leftArmDown -= OnLeftArmDown;
        HumanBodyTracker.m_rightArmDown -= OnRightArmDown;
    }

    ARHumanBody mHumanBody;
    bool isLeftUp = false;
    bool isRightUp = false;
    void OnLeftArmUp(ARHumanBody humanBody) {
        mHumanBody = humanBody;
        isLeftUp = true;
    }

    void OnLeftArmDown() {
        isLeftUp = false;
    }

    void OnRightArmUp(ARHumanBody humanBody) {
        mHumanBody = humanBody;
        isRightUp = true;
    }

    void OnRightArmDown() {
        isRightUp = false;
    }

    void RandomIdle() {
        var r = Random.value;
        if (r < 0.002f) {
            animator.SetTrigger("jump");
        } else if (r < 0.004f) {
            animator.SetTrigger("roll");
        }
    }

    Vector3 GetTargetPosition() {
        if (!mHumanBody) return Vector3.zero;

        if (isLeftUp) {
            return mHumanBody.transform.TransformPoint(mHumanBody.joints[22].anchorPose.position);
        }

        if (isRightUp) {
            return mHumanBody.transform.TransformPoint(mHumanBody.joints[66].anchorPose.position);
        }

        return Vector3.zero;
    }

    Quaternion GetTargetRotation() {
        if (!mHumanBody) return Quaternion.identity;
        return  Quaternion.Inverse(mHumanBody.transform.rotation);
    }

    // Update is called once per frame
    void Update()
    {
        if (isLeftUp || isRightUp) {
            var targetPosition = GetTargetPosition();

            var dis = Vector3.Distance(transform.position, targetPosition);
            // Debug.Log(dis);
            if (dis < 0.02f && state != State.TouchDown) {
                animator.SetTrigger("touchDown");
                state = State.TouchDown;
            }

            if (state == State.TouchDown) {
                transform.rotation = Quaternion.Slerp(transform.rotation, GetTargetRotation(), rotationSpeed * Time.deltaTime);
                transform.position = targetPosition;
                RandomIdle();
            } else if (state == State.Flying) {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetPosition - transform.position), rotationSpeed * Time.deltaTime);
                transform.position += transform.forward * Time.deltaTime * flySpeed;
                transform.localScale += scaleSpeed;
                if (transform.localScale.x > 0.1f) {
                    transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                }
            }
        } else {
            if (state == State.TouchDown) {
                animator.SetTrigger("fly");
                state = State.Flying;
            }
            var targetPosition = startOffsetPos;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetPosition - transform.position), rotationSpeed * Time.deltaTime);
            transform.position += transform.forward * Time.deltaTime * flySpeed;
            transform.localScale -= scaleSpeed;
            if (transform.localScale.x < 0f) {
                transform.localScale = Vector3.zero;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARFoundation.Samples
{
    public class HumanBodyTracker : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The Skeleton prefab to be controlled.")]
        GameObject m_SkeletonPrefab;

        [SerializeField]
        [Tooltip("The ARHumanBodyManager which will produce body tracking events.")]
        ARHumanBodyManager m_HumanBodyManager;

        /// <summary>
        /// Get/Set the <c>ARHumanBodyManager</c>.
        /// </summary>
        public ARHumanBodyManager humanBodyManager
        {
            get { return m_HumanBodyManager; }
            set { m_HumanBodyManager = value; }
        }

        public Text leftText;
        public Text leftPosText;
        public Text rightText;
        public Text rightPosText;
        /// <summary>
        /// Get/Set the skeleton prefab.
        /// </summary>
        public GameObject skeletonPrefab
        {
            get { return m_SkeletonPrefab; }
            set { m_SkeletonPrefab = value; }
        }

        Dictionary<TrackableId, BoneController> m_SkeletonTracker = new Dictionary<TrackableId, BoneController>();

        void OnEnable()
        {
            Debug.Assert(m_HumanBodyManager != null, "Human body manager is required.");
            m_HumanBodyManager.humanBodiesChanged += OnHumanBodiesChanged;
        }

        void OnDisable()
        {
            if (m_HumanBodyManager != null)
                m_HumanBodyManager.humanBodiesChanged -= OnHumanBodiesChanged;
        }

        void OnHumanBodiesChanged(ARHumanBodiesChangedEventArgs eventArgs)
        {
            BoneController boneController;

            foreach (var humanBody in eventArgs.added)
            {
                if (!m_SkeletonTracker.TryGetValue(humanBody.trackableId, out boneController))
                {
                    Debug.Log($"Adding a new skeleton [{humanBody.trackableId}].");
                    var newSkeletonGO = Instantiate(m_SkeletonPrefab, humanBody.transform);
                    boneController = newSkeletonGO.GetComponent<BoneController>();
                    m_SkeletonTracker.Add(humanBody.trackableId, boneController);
                }

                boneController.InitializeSkeletonJoints();
                boneController.ApplyBodyPose(humanBody);
                Debug.Log("New body found");
                ArmsCheck(humanBody);
            }

            foreach (var humanBody in eventArgs.updated)
            {
                if (m_SkeletonTracker.TryGetValue(humanBody.trackableId, out boneController))
                {
                    Debug.Log("Body update");
                    boneController.ApplyBodyPose(humanBody);
                    ArmsCheck(humanBody);
                }
                if (humanBody.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.None) {
                    LostTracking();
                }
            }

            foreach (var humanBody in eventArgs.removed)
            {
                Debug.Log($"Removing a skeleton [{humanBody.trackableId}].");
                if (m_SkeletonTracker.TryGetValue(humanBody.trackableId, out boneController))
                {
                    Destroy(boneController.gameObject);
                    LostTracking();
                }
            }
        }

        private int[] m_leftArmIndces = {20, 21, 22};
        private int[] m_rightArmIndces = {64, 65, 66};
        private int m_leftArmUpCount = 0;
        private int m_rightArmUpCount = 0;

        private bool m_leftUp = false;
        private bool m_rightUp = false;
        private float kMaxHeightdiff = 0.15f; // in centimeter
        public delegate void LeftArmUpEventHandler(ARHumanBody humanBody);
        public static event LeftArmUpEventHandler m_leftArmUp;

        public delegate void LeftArmDownEventHandler();
        public static event LeftArmDownEventHandler m_leftArmDown;

        public delegate void RightArmUpEventHandler(ARHumanBody humanBody);
        public static event RightArmUpEventHandler m_rightArmUp;

        public delegate void RightArmDownEventHandler();
        public static event RightArmDownEventHandler m_rightArmDown;

        void LostTracking() {
            m_leftArmUpCount = 0;
            m_rightArmUpCount = 0;
            if (m_leftArmDown != null && m_leftUp) {
                m_leftArmDown();
            }
            if (m_rightArmDown != null && m_rightUp) {
                m_rightArmDown();
            }
            m_leftUp = false;
            m_rightUp = false;
            leftText.text = "Left";
            rightText.text = "Right";
        }

        void ArmsCheck(ARHumanBody humanBody) {
            bool leftUp = LeftArmUpTest(humanBody);
            m_leftArmUpCount += leftUp ? 1 : -1;
            if (m_leftArmUpCount == 20) {
                if (m_leftArmUp != null && !m_leftUp) {
                    m_leftArmUp(humanBody);
                }
                m_leftUp = true;
                leftText.text = "Left Up";
                m_leftArmUpCount = 0;
            } else if (m_leftArmUpCount == -20) {
                if (m_leftArmDown != null && m_leftUp) {
                    m_leftArmDown();
                }
                m_leftUp = false;
                leftText.text = "Left Down";
                m_leftArmUpCount = 0;
            }
            bool rightUp = RightArmUpTest(humanBody);
            m_rightArmUpCount += rightUp ? 1 : -1;
            if (m_rightArmUpCount == 20) {
                if (m_rightArmUp != null && !m_rightUp) {
                    m_rightArmUp(humanBody);
                }
                m_rightUp = true;
                rightText.text = "Right Up";
                m_rightArmUpCount = 0;
            } else if (m_rightArmUpCount == -20) {
                if (m_rightArmDown != null && m_rightUp) {
                    m_rightArmDown();
                }
                m_rightUp = false;
                rightText.text = "right Down";
                m_rightArmUpCount = 0;
            }
        }
        bool LeftArmUpTest(ARHumanBody humanBody) {
            leftPosText.text = humanBody.joints[20].anchorPose.position.x.ToString("0.000") + "  " + humanBody.joints[20].anchorPose.position.y.ToString("0.000") + "  " + humanBody.joints[20].anchorPose.position.z.ToString("0.000") + "\n" +
                humanBody.joints[21].anchorPose.position.x.ToString("0.000") + "  " + humanBody.joints[21].anchorPose.position.y.ToString("0.000") + "  " + humanBody.joints[21].anchorPose.position.z.ToString("0.000") + "\n" +
                humanBody.joints[22].anchorPose.position.x.ToString("0.000") + "  " + humanBody.joints[22].anchorPose.position.y.ToString("0.000") + "  " + humanBody.joints[22].anchorPose.position.z.ToString("0.000") + "\n";
            float y = humanBody.joints[m_leftArmIndces[1]].anchorPose.position.y;
            for (int i = 0; i < m_leftArmIndces.Length; ++i) {
                if (Math.Abs(humanBody.joints[m_leftArmIndces[i]].anchorPose.position.y - y) > kMaxHeightdiff) {
                    return false;
                }
            }
            return true;
        }

        bool RightArmUpTest(ARHumanBody humanBody) {
            rightPosText.text = humanBody.joints[64].anchorPose.position.x.ToString("0.000") + "  " + humanBody.joints[64].anchorPose.position.y.ToString("0.000") + "  " + humanBody.joints[64].anchorPose.position.z.ToString("0.000") + "\n" +
                humanBody.joints[65].anchorPose.position.x.ToString("0.000") + "  " + humanBody.joints[65].anchorPose.position.y.ToString("0.000") + "  " + humanBody.joints[65].anchorPose.position.z.ToString("0.000") + "\n" +
                humanBody.joints[66].anchorPose.position.x.ToString("0.000") + "  " + humanBody.joints[66].anchorPose.position.y.ToString("0.000") + "  " + humanBody.joints[66].anchorPose.position.z.ToString("0.000") + "\n";
            float y = humanBody.joints[m_rightArmIndces[1]].anchorPose.position.y;
            for (int i = 0; i < m_rightArmIndces.Length; ++i) {
                if (Math.Abs(humanBody.joints[m_rightArmIndces[i]].anchorPose.position.y - y) > kMaxHeightdiff) {
                    return false;
                }
            }
            return true;
        }
    }
}
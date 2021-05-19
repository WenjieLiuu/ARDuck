using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARFoundation.Samples
{
    /// <summary>
    /// Listens for touch events and performs an AR raycast from the screen touch point.
    /// AR raycasts will only hit detected trackables like feature points and planes.
    ///
    /// If a raycast hits a trackable, the <see cref="placedPrefab"/> is instantiated
    /// and moved to the hit position.
    /// </summary>
    [RequireComponent(typeof(ARRaycastManager))]
    public class PlaceDuck : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Instantiates this prefab on a plane at the touch location.")]
        GameObject m_PlacedPrefab;

        [SerializeField]
        [Tooltip("The ARHumanBodyManager which will produce frame events.")]
        ARHumanBodyManager m_HumanBodyManager;

        /// <summary>
        /// Get or set the <c>ARHumanBodyManager</c>.
        /// </summary>
        public ARHumanBodyManager humanBodyManager
        {
            get { return m_HumanBodyManager; }
            set { m_HumanBodyManager = value; }
        }

        List<FollowAI> ducks = new List<FollowAI>();

        /// <summary>
        /// The prefab to instantiate on touch.
        /// </summary>
        public GameObject placedPrefab
        {
            get { return m_PlacedPrefab; }
            set { m_PlacedPrefab = value; }
        }

        /// <summary>
        /// The object instantiated as a result of a successful raycast intersection with a plane.
        /// </summary>
        public GameObject spawnedObject { get; private set; }

        void Awake()
        {
            m_RaycastManager = GetComponent<ARRaycastManager>();
        }

        void OnEnable()
        {
            m_HumanBodyManager.humanBodiesChanged += OnHumanBodiesChanged;
        }

        void OnDisable()
        {
            m_HumanBodyManager.humanBodiesChanged -= OnHumanBodiesChanged;
        }


        bool TryGetTouchPosition(out Vector2 touchPosition)
        {
            if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                touchPosition = Input.GetTouch(0).position;
                return true;
            }

            touchPosition = default;
            return false;
        }

        static Vector3 RandomPosition(Vector3 center, float radius) {
            var pos = Random.insideUnitCircle * radius;
            return new Vector3(pos.x, 0f, pos.y) + center;
        }

        void Update()
        {
            if (!TryGetTouchPosition(out Vector2 touchPosition))
                return;

            if (m_RaycastManager.Raycast(touchPosition, s_Hits, TrackableType.PlaneWithinPolygon))
            {
                // Raycast hits are sorted by distance, so the first one
                // will be the closest hit.
                var hitPose = s_Hits[0].pose;
                spawnedObject = Instantiate(m_PlacedPrefab, hitPose.position, hitPose.rotation);
                FollowAI duck = spawnedObject.GetComponent<FollowAI>();
                duck.offset = RandomPosition(new Vector3(0,0,1), 0.5f);
                ducks.Add(duck);
            }
        }

        Vector3 posBefore = new Vector3(100, 100, 100);
        float eulerY = -100f;
        void OnHumanBodiesChanged(ARHumanBodiesChangedEventArgs eventArgs)
        {
            if (eventArgs.updated.Count > 0) {
                var bodyTransform = eventArgs.updated[0].transform;
                if (Vector3.Distance(posBefore, bodyTransform.position) < 0.5f && Mathf.Abs(eulerY - bodyTransform.eulerAngles.y) < 25f) {
                    return;
                }
                Debug.Log("~~~~~~~~~~~~~~~distance = " + Vector3.Distance(posBefore, bodyTransform.position));
                posBefore = bodyTransform.position;
                eulerY = bodyTransform.eulerAngles.y;
                foreach(var duck in ducks) {
                    duck.target = bodyTransform;
                }
            } else {
                foreach(var duck in ducks) {
                    duck.target = null;
                    posBefore = new Vector3(100, 100, 100);
                    eulerY = -100f;
                }
            }
        }

        static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

        ARRaycastManager m_RaycastManager;
    }
}

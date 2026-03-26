using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace PxP
{
    [RequireComponent(typeof(Collider)), RequireComponent(typeof(MeshFilter))]
    public class Rotator : MonoBehaviour
    {
        [Tooltip("The desired axis of rotation")]
        public RotationAxis rotationAxis = RotationAxis.Z;
        [Tooltip("The speed of rotation")]
        public float rotationSpeed = 50.0f;
        [Tooltip("The list of events that will be triggered")]
        public List<TimedEvents> timedEvents = new List<TimedEvents>();

        private MeshCollider meshCollider = null;
        private MeshFilter meshFilter = null;

        [System.Serializable]
        public class TimedEvents
        {
            [Tooltip("The delay (in seconds) before the activation of the following events")]
            public float activationDelay = 0.0f;
            [Tooltip("The events to be triggered after the delay has reached 0")]
            public UnityEvent events;
        }

        public enum RotationAxis
        {
            X,
            Y,
            Z
        }

        private void Start()
        {
            meshCollider = GetComponent<MeshCollider>();
            meshFilter = GetComponent<MeshFilter>();
        }

        // Update is called once per frame
        void Update()
        {
            //Selection of the rotation axis
            Vector3 currentrotationAxis = new Vector3();
            switch (rotationAxis)
            {
                case RotationAxis.X:
                    currentrotationAxis = Vector3.right;
                    break;
                case RotationAxis.Y:
                    currentrotationAxis = Vector3.up;
                    break;
                case RotationAxis.Z:
                    currentrotationAxis = Vector3.forward;
                    break;
            }
            //Rotation and Physics "Update"
            transform.Rotate(currentrotationAxis, Time.deltaTime * rotationSpeed);
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = meshFilter.mesh;

            //Events time & activation
            foreach (TimedEvents TEvent in timedEvents)
            {
                switch (TEvent.activationDelay)
                {
                    case > 0.0f:
                        TEvent.activationDelay -= Time.deltaTime;
                        break;
                    case -1.0f:
                        break;
                    case <= 0.0f:
                        TEvent.events.Invoke();
                        TEvent.activationDelay = -1.0f;
                        break;
                }
            }
        }

        /// <summary>
        /// Sets the RotationAxis for a specific elements in the RotatingElements list
        /// </summary>
        /// <param name="rotationAxis">The axis around which the element rotates (X||Y||Z)</param>
        public void SetRotationAxis(string rotationAxis)
        {
            rotationAxis = rotationAxis.ToLower();
            char axisChar = rotationAxis[0];
            switch (axisChar)
            {
                case 'x':
                    this.rotationAxis = RotationAxis.X;
                    break;
                case 'y':
                    this.rotationAxis = RotationAxis.Y;
                    break;
                case 'z':
                    this.rotationAxis = RotationAxis.Z;
                    break;
                default:
                    Debug.Log("Incorect Axis chosen for rotation, values must be X or Y or Z");
                    break;
            }
        }
        /// <summary>
        ///  Sets the RotationSpeed for a specific element in the RotatingElements list
        /// </summary>
        /// <param name="rotationSpeed">The speed at which the element rotates</param>
        public void SetRotationSpeed(float rotationSpeed)
        {
            this.rotationSpeed = rotationSpeed;
        }
    }
}

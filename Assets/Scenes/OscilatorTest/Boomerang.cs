using System;
using UnityEngine;

namespace Hykudoru
{
    public static class Math
    {
        public const float Tau = 2*Mathf.PI; // Put it simply, its just better!

        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
            {
                value = min;
            }
            else if (value > max)
            {
                value = max;
            }
            
            return value;
        }
    }

    public enum Plane
    {
        Undefined = 0,
        XY,
        XZ,
        ZY
    }

    public class Boomerang : MonoBehaviour
    {
        private Vector3 initialPosition;
        private Vector3 deltaPosition = Vector3.zero;

        // Creates bouncing like motion similar to ellastic collisions.
        [SerializeField] private bool halfCycle = false;
        public bool HalfCycle
        {
            get { return halfCycle; }
            set { halfCycle = value; }
        }

        // Uses the objects frame of reference ensuring the object maintains the same motion no matter its orientation.
        [SerializeField] private bool localSpace = false;
        public bool LocalSpace
        {
            get { return localSpace; }
            set { localSpace = value; }
        }

        // Set the Plane to 'undefined' if you want one dimensional linear oscillations or linear diagnal oscillations. All axis are considered, however, every dimension is modified by the sine function. 
        // Set the Plane to any other option if you want to constrain motion to that plane specifically to create planetary motion. This will utilize both the "cosin" and "sine" function so the outcome may not be what you normally would expect.
        [SerializeField]
        private Plane orbitalPlane = Plane.Undefined;
        public Plane OrbitalPlane
        {
            get { return orbitalPlane; }
            set { orbitalPlane = value; }
        }

        [SerializeField]
        private Vector3 velocity = Vector3.zero;
        public Vector3 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        //Radial distance from its initial starting point.
        [SerializeField]
        private Vector3 displacement = Vector3.one;
        public Vector3 Displacement
        {
            get { return displacement; }
            set { velocity = value; }
        }

        //Speed Control
        [SerializeField] [Range(0f, 10f)]
        private float frequencyMultiplier = 1f;
        public float FrequencyMultiplier
        {
            get { return frequencyMultiplier; }
            set { frequencyMultiplier = Math.Clamp(value, 0f, 10f); }
        }

        //Increases the displacement or radial distance of the object.
        [SerializeField] [Range(0f, 1000f)]
        private float amplitudeMultiplier = 1f;
        public float AmplitudeMultiplier
        {
            get { return amplitudeMultiplier; }
            set { amplitudeMultiplier = Math.Clamp(value, 0f, 1000f); }
        }

        // Use this for initialization
        private void Start()
        {
            initialPosition = transform.position;
        }

        // Update is called once per frame
        private void Update()
        {
            Oscillate();
        }

        private void Oscillate()
        {
            float xSpeed = (Math.Tau * Velocity.x * Time.timeSinceLevelLoad) * FrequencyMultiplier;
            float ySpeed = (Math.Tau * Velocity.y * Time.timeSinceLevelLoad) * FrequencyMultiplier;
            float zSpeed = (Math.Tau * Velocity.z * Time.timeSinceLevelLoad) * FrequencyMultiplier;
            float deltaX = Displacement.x * AmplitudeMultiplier;
            float deltaY = Displacement.y * AmplitudeMultiplier;
            float deltaZ = Displacement.z * AmplitudeMultiplier;

            switch (orbitalPlane)
            {
                case Plane.XY:
                    deltaX *= Mathf.Cos(xSpeed);
                    deltaY *= Mathf.Sin(ySpeed);
                    if (halfCycle)
                    {
                        deltaY = deltaY < 0 ? Mathf.Abs(deltaY) : deltaY;
                    }
                    break;

                case Plane.XZ:
                    deltaX *= Mathf.Cos(xSpeed);
                    deltaZ *= Mathf.Sin(zSpeed);
                    if (halfCycle)
                    {
                        deltaZ = deltaZ < 0 ? Mathf.Abs(deltaZ) : deltaZ;
                    }
                    break;

                case Plane.ZY:
                    deltaZ *= Mathf.Cos(zSpeed);
                    deltaY *= Mathf.Sin(ySpeed);
                    if (halfCycle)
                    {
                        deltaY = deltaY < 0 ? Mathf.Abs(deltaY) : deltaY;
                    }
                    break;

                default:
                    deltaX *= Mathf.Sin(xSpeed);
                    deltaY *= Mathf.Sin(ySpeed);
                    deltaZ *= Mathf.Sin(zSpeed);
                    if (halfCycle)
                    {
                        deltaX = deltaX < 0 ? Mathf.Abs(deltaX) : deltaX;
                        deltaY = deltaY < 0 ? Mathf.Abs(deltaY) : deltaY;
                        deltaZ = deltaZ < 0 ? Mathf.Abs(deltaZ) : deltaZ;
                    }
                    break;
            }

            if (localSpace)
            {
                deltaPosition = (transform.right * deltaX) + (transform.up * deltaY) + (transform.forward * deltaZ);
            }
            else
            {
                deltaPosition.Set(deltaX, deltaY, deltaZ);
            }

            transform.position = initialPosition + deltaPosition;
        }
    }
}
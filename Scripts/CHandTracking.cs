using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR;


namespace Mocap
{
    public class CHandTracking : MonoBehaviour
    {
        public float maxDeltaPos;  // Maximum movement within a frame [m/s]
        public float maxDeltaRot;  // Maximum rotation within a frame [deg/s]
        public Vector3 offset;     // Offset to tracked hand position


        Vector3 mPos = Vector3.zero;
        Quaternion mRot = Quaternion.identity;
        Quaternion mRotOffs = Quaternion.identity;
        XRNode mNode = XRNode.LeftHand;
        InputDevice mHand;
        bool mIsDefined = false;

        readonly InputFeatureUsage<Vector3> USEPOS = CommonUsages.devicePosition;
        readonly InputFeatureUsage<Quaternion> USEROT = CommonUsages.deviceRotation;

        [SerializeField]
        private bool mStartAsLeft;  // Right or left hand
        [SerializeField]
        private Vector3 mOffsetAngles;


        public void SetOffsetRotation(Vector3 euler)
        {
            // No argument validation to +-180 is done
            mRotOffs = Quaternion.Euler(euler.x, euler.y, euler.z);
            if (mOffsetAngles != euler)
                mOffsetAngles = euler;
        }

        public void SetHand(bool isLeft)
        {
            mNode = isLeft ? XRNode.LeftHand : XRNode.RightHand;
            mRotOffs = Quaternion.Euler(mOffsetAngles.x, mOffsetAngles.y, mOffsetAngles.z);
            if (mStartAsLeft != isLeft)
            {
                mStartAsLeft = isLeft;
            }
            RefreshDevice();
        }

        public bool IsLeft => mNode == XRNode.LeftHand;

        void Awake()
        {
            InputDevices.deviceConnected += OnDeviceConnected;
        }


        void Start()
        {
            mPos = transform.localPosition;
            mRot = transform.localRotation;
            SetHand(mStartAsLeft);
            if (!IsInvoking("UpdateHandPose"))
            {
                InvokeRepeating("UpdateHandPose", 3f, 0.04f);
            }
        }


        private void OnDisable()
        {
            CancelInvoke("UpdateHandPose");
        }


        private void OnEnable()
        {
            if (!IsInvoking("UpdateHandPose"))
            {
                RefreshDevice();
                InvokeRepeating("UpdateHandPose", .5f, 0.04f);
            }
        }


        public void OnDeviceConnected(InputDevice value)
        {
            if ((mNode == XRNode.LeftHand && value.characteristics.HasFlag(InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Left)) ||
                (mNode == XRNode.RightHand && value.characteristics.HasFlag(InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Right)))
            {
                mHand = value;  // Assuming this gives only valids
            }
        }


        void Update()
        {
            if (mIsDefined)
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, mPos, maxDeltaPos * Time.deltaTime);
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, mRot, maxDeltaRot * Time.deltaTime);
            }
        }


        void UpdateHandPose()
        {
            mIsDefined = false;
            if (mHand.isValid && mHand.TryGetFeatureValue(USEROT, out mRot) && mHand.TryGetFeatureValue(USEPOS, out mPos))
            {
                mRot *= mRotOffs;
                mPos += mRot * offset;
                mIsDefined = true;
            }
        }


        void RefreshDevice()
        {
            if (!XRDevice.isPresent)
                return;

            var handDevices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(mNode, handDevices);
            if (handDevices.Count > 0)
            {
                mHand = handDevices.Find(dev => dev.isValid);
            }
            else
            {
                Debug.Log("No compatible XR device found");
            }
        }


#if UNITY_EDITOR
        private void OnValidate()
        {
            SetOffsetRotation(mOffsetAngles);
        }
#endif
    }
}

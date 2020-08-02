using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.XR;


namespace Mocap
{
    public class CFootTracking : MonoBehaviour
    {
        public float offsetAngle;
        public float maxDeltaPos;
        public float maxDeltaRot;
        public Vector3 offset;

        Vector3 mPos = Vector3.zero;
        Quaternion mRot = Quaternion.identity;
        Quaternion mRotOffs = Quaternion.identity;
        //InputDevice[] mFeets = new InputDevice[2];
        InputDevice mFoot;
        bool mIsDefined = false;
        readonly InputFeatureUsage<Vector3> USEPOS = CommonUsages.devicePosition;
        readonly InputFeatureUsage<Quaternion> USEROT = CommonUsages.deviceRotation;

        
        public bool IsLeft { get; set; }


        void Awake()
        {
            InputDevices.deviceConnected += OnDeviceConnected;
        }


        void Start()
        {
            mPos = transform.localPosition;
            mRot = transform.localRotation;
            if (!IsInvoking("UpdatePose"))
            {
                InvokeRepeating("UpdatePose", 3f, 0.04f);
            }
            RefreshDevice();
        }


        private void OnDisable()
        {
            CancelInvoke("UpdatePose");
        }


        private void OnEnable()
        {
            if (!IsInvoking("UpdatePose"))
            {
                InvokeRepeating("UpdatePose", .5f, 0.04f);
            }
        }


        void OnDeviceConnected(InputDevice value)
        {
            // todo check is tracker role feets
        }


        void Update()
        {
            if (mIsDefined)
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, mPos, maxDeltaPos * Time.deltaTime);
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, mRot, maxDeltaRot * Time.deltaTime);
            }
        }


        void UpdatePose()
        {
            mIsDefined = false;
            if (mFoot.isValid)
            {
                mFoot.TryGetFeatureValue(USEROT, out mRot);
                mRot *= mRotOffs;

                mFoot.TryGetFeatureValue(USEPOS, out mPos);
                mPos += mRot * offset;
                mIsDefined = true;
            }
        }


        void RefreshDevice()
        {
            var trackers = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.HardwareTracker, trackers);
            if (trackers.Count > 1)
            {
                var tracker = trackers.Find(id => id.isValid);
                //todo get serialnumber and search steamvr for its tracker role

                //mFeets[1] = trackers.FindLast(id => id.isValid);
            }
        }
    }
}

using UnityEngine;
using UnityEngine.XR;


namespace Mocap
{
    public class CHandTracking : MonoBehaviour
    {
        public bool  isLeft;       // Right or left hand
        public float offsetAngle;  // left hand z-rot offset
        public float MaxDeltaPos;  // Maximum movement within a frame [m/s]
        public float MaxDeltaRot;  // Maximum rotation within a frame [deg/s]
        public Vector3 Offset;     // Offset to tracked hand position

        Vector3      mPos = Vector3.zero;
        Quaternion   mRot = Quaternion.identity;
        Quaternion   mRotOffs = Quaternion.identity;
        XRNode node = XRNode.LeftHand;

        
        void Start()
        {
            mPos = transform.localPosition;
            mRot = transform.localRotation;
            if (isLeft)
            {
                mRotOffs = Quaternion.Euler(0.0f, 0.0f, offsetAngle);
            }
            else
            {
                node = XRNode.RightHand;
                mRotOffs = Quaternion.Euler(0.0f, 0.0f, -offsetAngle);
            }
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
                InvokeRepeating("UpdateHandPose", .5f, 0.04f);
            }
        }


        void Update()
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, mPos, MaxDeltaPos * Time.deltaTime);
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, mRot, MaxDeltaRot * Time.deltaTime);
        }


        void UpdateHandPose()
        {
            
            mRot = InputTracking.GetLocalRotation(node) * mRotOffs;
            
            //Vector3 vec;
            //vec = transform.right * Offset.x;     // horizontal correction
            //vec += transform.up * Offset.y;       // vertical correction
            //vec += transform.forward * Offset.z;  // Transversal correction

            mPos = InputTracking.GetLocalPosition(node) + mRot * Offset;
        }
    }
}

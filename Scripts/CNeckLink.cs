using UnityEngine;


namespace Mocap
{
    public class CNeckLink : MonoBehaviour
    {
        /**
         * Adds lateral rotation from target transform to the referenced humanoid head and neck.
         */
        
        public Transform masterTransform;
        public Transform slaveTransform;
        public bool switchAxisXZ;
        
        Transform neckTransform = null;

        void Awake()
        {
          neckTransform = slaveTransform.parent.transform;
        }
        
        
        void LateUpdate()
        {
            if (switchAxisXZ)
            {
                float latRot = Mathf.DeltaAngle(360.0f, masterTransform.eulerAngles.z);
                Vector3 eulerRot = slaveTransform.localEulerAngles;
                eulerRot.x = latRot;
                slaveTransform.rotation = Quaternion.Euler(eulerRot);

                eulerRot = neckTransform.localEulerAngles;
                eulerRot.x = latRot * 0.5f;
                neckTransform.rotation = Quaternion.Euler(eulerRot);
            }
            else
            {
                Quaternion HeadToEyeRot = masterTransform.rotation * Quaternion.Inverse(slaveTransform.localRotation);
                slaveTransform.rotation = HeadToEyeRot * slaveTransform.rotation;
                neckTransform.rotation = Quaternion.Slerp(Quaternion.identity, HeadToEyeRot, 0.5f) * neckTransform.rotation;
            }
        }
    }

}

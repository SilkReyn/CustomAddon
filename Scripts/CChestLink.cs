using UnityEngine;


namespace Mocap
{
    public class CChestLink : MonoBehaviour
    {

        public Transform headTarget;
        public Transform leftHandTarget;
        public Transform rightHandTarget;
        public Transform leftFootTarget;
        public Transform rightFootTarget;
        public Transform pivotTransform;

        public Vector3   headPosOffset;
        public float     maxTorsionAngle;
        public float     armLength;
        public float     torsionSpeedDeg;

        
        void Update()
        {
            Vector3 v3;
            Vector3 dir;
            Quaternion pivRotInv;
            float headLinkageAng;
            float lHLinkAng;
            float rHLinkAng;
            
            // Set pivot to the center of the feet.
            v3 = Vector3.Lerp(rightFootTarget.position, leftFootTarget.position, 0.5f);
            v3.y = pivotTransform.position.y;
            pivotTransform.position = v3;

            // Set pivot orientation to the intermediate angle of the feet.
            pivotTransform.rotation = Quaternion.Euler(0f, Mathf.LerpAngle(rightFootTarget.eulerAngles.y, leftFootTarget.eulerAngles.y, 0.5f), 0f);
            
            // Linked with eye position.
            v3.y = headTarget.position.y - headPosOffset.y;
            if (v3.y < pivotTransform.position.y)
            {
                v3.y = pivotTransform.position.y;
            }
            // Linked with "IK_Pivot" position.
            v3.x = pivotTransform.position.x;
            v3.z = pivotTransform.position.z;

            // Set position.
            transform.position = v3;
            
            // Yaw linked to lateral distance from head to hands
            pivRotInv = Quaternion.Inverse(pivotTransform.localRotation);
            v3 = headTarget.localPosition;
            if (armLength == 0)
            {
                armLength = 0.01f;
            }
            lHLinkAng = Mathf.LerpAngle(0.0f, maxTorsionAngle, (pivRotInv * (leftHandTarget.localPosition - v3)).x / armLength);
            rHLinkAng = Mathf.LerpAngle(0.0f, -maxTorsionAngle, (pivRotInv * (rightHandTarget.localPosition - v3)).x / -armLength);
            
            // Additional yaw based on difference from body orientation to head angle
            headLinkageAng = Vector3.SignedAngle(pivotTransform.forward, headTarget.forward, Vector3.up);
            if (Mathf.Abs(headLinkageAng) < 90)
            {
                headLinkageAng = 0;
            }

            // Tilt linked with head position.
            dir = pivRotInv * (headTarget.localPosition - transform.localPosition);

            // Set rotation.
            v3.x = -Mathf.Atan2(dir.y, dir.z + headPosOffset.z) * Mathf.Rad2Deg + 90.0f;
            v3.y =
                pivotTransform.localEulerAngles.y
                + lHLinkAng
                + rHLinkAng
                + headLinkageAng * 0.5f;
            v3.z = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90.0f;;
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(v3), torsionSpeedDeg);
        }
    }
}

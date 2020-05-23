using UnityEngine;


namespace Mocap
{
    public class CBoneHinge : MonoBehaviour
    {
        // Manager values
        public volatile bool isUseEachBoneForceSettings; // if false, drag and stiffness is overwritten by bone manager curves
        public Vector3 worldAccel;                       //constant acceleration vector, eg gravity in world direction
        public float springFactor;
        public float dragFactor;
        public ushort rotationLimit;

        //protected enum ForwardAxis{ ax = 0, ay, az };

        private float springLength = 1f;
        private float radius = 0f;
        private Vector3 boneDirLocal = Vector3.forward;  // bone chain (local) direction
        private Vector3 boneForward = Vector3.down;      // bone chain (world) direction
        private Vector3 prevTipPos = Vector3.zero;
        private Quaternion initialLocalRot = Quaternion.identity;
        private float cycleTimeSqr = 123.45e-6f;
        private byte fwd = 2;
        

        private void Awake()
        {
            CBoneManager managerRef = GetParentManager(transform);
            if (managerRef!=null)
            {
                radius = managerRef.globalRadius;
                cycleTimeSqr = Mathf.Pow(1.0f / managerRef.sampleRate, 2);
                if (rotationLimit == 0)
                {
                    if (managerRef.globalRotLim != 0)
                    {
                        rotationLimit = managerRef.globalRotLim;
                    }else{
                        rotationLimit = 360;
                    }
                }
            }
        }

        
        private void Start()
        {
            Transform t = transform.GetChild(0);  //should be the next bone
            Vector3 childPos;
            if (t != null)
            {
                childPos = t.position;
            } else {
                childPos = transform.position;
            }
            
            springLength = Vector3.Distance(transform.position, childPos);
            prevTipPos = childPos;

            // Direction of the child relative to this transform (normed).
            // Should be (0|1|0) for directly connected bones created in Blender.
            boneDirLocal = transform.InverseTransformPoint(childPos).normalized;
            initialLocalRot = transform.localRotation;

            childPos = boneDirLocal;
            for(int i=0; i<3; ++i)
            {
                childPos[i] = Mathf.Abs(childPos[i]);
            }
            if ((childPos.x > childPos.z) && (childPos.y <= childPos.x))
            {
                // ForwardAxis.ax;
                fwd = 0;
            }
            else if (childPos.y > childPos.z)
            {
                // ForwardAxis.ay;
                fwd = 1;
            }
        }


        private CBoneManager GetParentManager(Transform t)
        {
            var bm = t.GetComponent<CBoneManager>();
            if (bm != null)
            {
                return bm;
            }
            if (t.parent != null)
            {// Look in parent's components
                return GetParentManager(t.parent);
            }
            return null;
        }


        public void UpdateHinge()
        {
            springFactor = Mathf.Clamp01(springFactor);
            dragFactor = Mathf.Clamp01(dragFactor);

            // Current chain direction in world space
            boneForward = transform.TransformDirection(boneDirLocal);  // is direction

            // Current real position of child  with distance to this transform of springlenght
            Vector3 currTipPos = transform.position + boneForward * springLength;

            // Constant extrinsic acceleration
            // ds = 0.5 * a * dt²
            Vector3 tipMove = 0.5f * worldAccel * cycleTimeSqr;  // * Mathf.Sin(Vector3.Angle(boneForward, worldAccel) * Mathf.Deg2Rad)

            // Drag (velocity based friction)
            tipMove -= (currTipPos - prevTipPos) * dragFactor;  // drag of 1 should freeze the point in space

            // Spring force (force to retain pose)
            Quaternion currRot = transform.rotation;
            transform.localRotation = initialLocalRot;  // reset, but in respect to parents current orientation
            tipMove += ((transform.rotation * boneDirLocal) - boneForward) * (springFactor * springLength);

            // Direction to new tip position
            Vector3 tipDir = boneForward * springLength + tipMove;  // is non-normalized direction

            if (Physics.Raycast(transform.position, tipDir, out RaycastHit hitInfo, springLength))  //from initial origin along new bone axis
            {
                tipDir = boneForward; // dont move it
                if (Physics.Raycast(transform.position, tipDir, springLength))  // if the other collides too (joint has moved towards collider)
                {// Move it and check again next cycle
                    tipDir = hitInfo.point + hitInfo.normal * radius - transform.position;  // does not help on acute angles
                }
            }
            prevTipPos = currTipPos;

            // Apply rotation
            Quaternion nextRot = Quaternion.FromToRotation(boneForward, tipDir) * currRot;  // identity to target rot
            float dA = Mathf.DeltaAngle(360f, Quaternion.Angle(transform.rotation, nextRot));
            if (dA <= rotationLimit)  // initial to target rot (might take different way), angle <180
            {
                transform.rotation = nextRot;
            } else {//if (Mathf.DeltaAngle(360f, Quaternion.Angle(transform.rotation, currRot)) <= rotationLimit) {  // initial to current rot
                transform.rotation = Quaternion.Lerp(transform.rotation, nextRot, (float)rotationLimit / dA);  // identity to interpolated target
            } //else {
                //Quaternion.RotateTowards(currRot, transform.rotation, 4 * Time.deltaTime);  // might rotate inwards
                //transform.rotation = Quaternion.FromToRotation(boneForward, -tipDir * 0.7f) * currRot;
            //}

            // Garantee bone roll remains same
            tipDir = transform.localRotation.eulerAngles;
            tipDir[fwd] = initialLocalRot.eulerAngles[fwd];
            transform.localRotation = Quaternion.Euler(tipDir);

        }
    }
}

using UnityEngine;


namespace Mocap
{
    public class CFootLink : MonoBehaviour
    {
        public Transform pivotTrns;
        public Transform headTrns;  // TODO Name "HeadTarget" (is not the head bone!)
        public Transform handTarget;  // deprecated
        public Transform altTarget=null;

        public float     maxDragLengthSqr;  // Max foot distance relative to head
        public float     maxLeanLengthSqr;  // How far body can lean over to stay balanced
        public Vector3   footPlacing;       // Signed offset from foot relative to hand or pivot
        public float     followSpeed;       // How fast foots move to target [m/s]
        public bool      isLeft;
        public int       mouseBtn;  // deprecated
        
        Vector3 targetPos = Vector3.zero;
        //Vector3 altTargetPos = Vector3.zero;
        Vector3 altTarOffs = Vector3.zero;
        //bool  controlStarted = false;
        //float ctrlStartHeight = 0f;

        static System.Collections.BitArray movingLeg = new System.Collections.BitArray(2);


        void Awake()
        {
            targetPos = transform.position;
        }

        void Update()
        {
            if (movingLeg.Get(isLeft ? 0 : 1) && Mathf.Abs((transform.position - targetPos).sqrMagnitude) < 1e-4)
            {// Unset moving
                movingLeg[isLeft ? 0 : 1] = false;
            }

            if (altTarget != null && Mathf.Abs((altTarget.position + altTarOffs).y - (pivotTrns.position.y + footPlacing.y)) > 4e-2)
            {// Override (foot lifted)
                /*targetPos = altTarget.position + altTarOffs;
                Vector3 vec = headTrns.position - pivotTrns.position;
                vec.y = 0;
                if ((vec.sqrMagnitude > Mathf.Abs(maxLeanLengthSqr)) || (Vector3.Dot(vec, pivotTrns.forward) < -Mathf.Abs(footPlacing.z)))
                {
                    vec = headTrns.forward;  // normed
                    vec.y = 0;
                    if (vec.sqrMagnitude > 0.12)  // ~20deg to world.up
                    {
                        transform.LookAt(transform.position + vec);
                    }
                }*/
                
                SetTarget(ref headTrns, 0f, 0f, 0f);
            }
            else if ((isLeft && !movingLeg.Get(1)) || (!isLeft && !movingLeg.Get(0)))  // Always keep one foot fix to avoid hovering
            {// None or only this foot is moving
                Vector3 dist = headTrns.position - transform.position;  //from trans to head
                dist.y = 0;
                if (dist.sqrMagnitude > Mathf.Abs(maxDragLengthSqr))
                {// drag legs
                    if (90 > Vector3.Angle(pivotTrns.right, dist))// seems to be always positive and under 180
                    {// lean right
                        if (isLeft)
                        {
                            SetTarget(ref pivotTrns, 0, footPlacing.y, 0.0f);
                        }
                    }
                    else
                    {// lean left
                        if (!isLeft)
                        {
                            SetTarget(ref pivotTrns, 0, footPlacing.y, 0.0f);
                        }
                    }
                }
                else
                {// stabilize
                    dist = headTrns.position - pivotTrns.position;
                    dist.y = 0;
                    if ((dist.sqrMagnitude > Mathf.Abs(maxLeanLengthSqr)) || (Vector3.Dot(dist, pivotTrns.forward) < -Mathf.Abs(footPlacing.z)))
                    {
                        if (90 > Vector3.Angle(pivotTrns.right, dist))
                        {// lean right
                            if (!isLeft)
                            {
                                SetTarget(ref headTrns, footPlacing.x, -headTrns.localPosition.y + footPlacing.y, 0.0f);
                            }
                        }
                        else
                        {// lean left
                            if (isLeft)
                            {
                                SetTarget(ref headTrns, -footPlacing.x, -headTrns.localPosition.y + footPlacing.y, 0.0f);
                            }
                        }
                    }
                }
                
            }

            if (targetPos.y < pivotTrns.position.y)
            { // Its already under under the ground.
                targetPos.y = pivotTrns.position.y + footPlacing.y;
            }

            // Move to position
            transform.position = Vector3.MoveTowards(transform.position, targetPos, followSpeed * Time.deltaTime);
        }


        void SetTarget(ref Transform refPos, float offsX, float offsY, float offsZ)
        {
            //if (movingLeg.Get(0) || movingLeg.Get(1))
            //    return;

            // Set the target position.
            if (null == altTarget)
            {
                targetPos = refPos.position + (refPos.right * offsX) + (refPos.forward * offsZ);
                targetPos.y = Mathf.Max(targetPos.y + offsY, pivotTrns.position.y + footPlacing.y);
            }
            else
            {
                targetPos = altTarget.position + altTarOffs;
            }

            // Set moving
            movingLeg[isLeft ? 0 : 1] = Mathf.Abs((transform.position - targetPos).sqrMagnitude) > 1e-4;

            // Set foot rotation
            Vector3 fwdHnd = refPos.forward;  // normed
            fwdHnd.y = 0;
            if (fwdHnd.sqrMagnitude > 0.12)  // ~20deg to world.up
            {
                transform.LookAt(transform.position + fwdHnd);
            }
        }


        public void CalibrateTrackedTargetNow()
        {
            altTarOffs = (null == altTarget) ? Vector3.zero :  transform.position - altTarget.position;
        }
    }

}

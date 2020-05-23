using UnityEngine;


namespace Mocap
{
    public class CFootLink : MonoBehaviour
    {
        public Transform pivotTrns;
        public Transform headTrns;
        public Transform handTarget;
        public float     maxDragLengthSqr;  // Max foot distance relative to head
        public float     maxLeanLengthSqr;  // How far body can lean over to stay balanced
        public Vector3   footPlacing;       // Signed offset from foot relative to hand or pivot
        public float     followSpeed;       // How fast foots move to target [m/s]
        public bool      isLeft;
        public int       mouseBtn;
        
        Vector3 targetPos = Vector3.zero;
        bool    controlStarted = false;
        float   ctrlStartHeight = 0f;
        static byte movingLeg = 0;

        private void Awake()
        {
            targetPos = transform.position;
        }


        void Update()
        {

            if (Input.GetMouseButton(mouseBtn))  // control override
            {
                movingLeg = 0;
                if (controlStarted == false)
                {
                    controlStarted = true;
                    ctrlStartHeight = handTarget.position.y - transform.position.y;
                }
                SetTarget(ref handTarget, 0, -ctrlStartHeight, footPlacing.z);
            } else if(controlStarted){ // Quit manual control
                controlStarted = false;
                targetPos.y = pivotTrns.position.y + footPlacing.y;
            } else {
                if ((isLeft && (movingLeg == 1)) || (!isLeft && (movingLeg == 2)))
                {
                    if (Mathf.Abs((transform.position - targetPos).sqrMagnitude) < 1e-4)
                    {
                        movingLeg = 0;
                    }
                }

                if (movingLeg==0)
                {
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
                        } else
                        {// lean left
                            if (!isLeft)
                            {
                                SetTarget(ref pivotTrns, 0, footPlacing.y, 0.0f);
                            }
                        }
                    } else {// lean over
                        dist = headTrns.position - pivotTrns.position;
                        dist.y = 0;
                        if ((dist.sqrMagnitude > Mathf.Abs(maxLeanLengthSqr)) || (Vector3.Dot(dist, pivotTrns.forward) < -Mathf.Abs(footPlacing.z)))
                        {
                            if (90 > Vector3.Angle(pivotTrns.right, dist))// seems to be always positive and under 180
                            {// lean right
                                if (!isLeft)
                                {
                                    SetTarget(ref headTrns, footPlacing.x, -headTrns.localPosition.y + footPlacing.y, 0.0f);
                                }
                            } else {// lean left
                                if (isLeft)
                                {
                                    SetTarget(ref headTrns, -footPlacing.x, -headTrns.localPosition.y + footPlacing.y, 0.0f);
                                }
                            }
                        }
                    }
                }//legs not in motion
            }

            if (targetPos.y < pivotTrns.position.y)
            { // Its already under under the ground.
                targetPos.y = pivotTrns.position.y;
            }

            // Move to position
            transform.position = Vector3.MoveTowards(transform.position, targetPos, followSpeed * Time.deltaTime);
        }


        void SetTarget(ref Transform refPos, float offsX, float offsY, float offsZ)
        {
            if (movingLeg != 0)
                return;

            // Set the target position.
            targetPos = refPos.position + (refPos.right * offsX) + (refPos.forward * offsZ);
            targetPos.y += offsY;
            if (Mathf.Abs((transform.position - targetPos).sqrMagnitude) > 1e-4)
            {
                movingLeg = isLeft ? (byte)1 : (byte)2;
            }
            
            // Set foot rotation
            Vector3 fwdHnd = refPos.forward;  // normed
            fwdHnd.y = 0;
            if (fwdHnd.sqrMagnitude > 0.12)  // ~20deg to world.up
            {
                transform.LookAt(transform.position + fwdHnd);
            }
        }

    }

}

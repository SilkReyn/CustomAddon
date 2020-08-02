using UnityEngine;


namespace Mocap
{
    public class CIkConf : MonoBehaviour
    {
        public Transform bodyTarget;
        public Transform leftHandTarget;
        public Transform rightHandTarget;
        public Transform leftFootTarget;
        public Transform rightFootTarget;

        public bool doEyeControl;
        public bool doHeadControl;
        public Transform lookAtTarget;

        public bool doHeadRoll;
        public Transform headTarget;
        public Vector3 headOrientationEuler;


        Animator animator;
        private Quaternion headBoneOrientation = Quaternion.identity;
        private Transform headBone = null;
        //private Transform footBoneL = null;
        //private Transform footBoneR = null;
        private readonly Vector3 kneeHintOffset = new Vector3(0f, .5f, 1f);

        private void Awake()
        {
            animator = gameObject.GetComponent<Animator>();
        }

        private void Start()
        {
            headBone = animator?.GetBoneTransform(HumanBodyBones.Head);
            //footBoneL = animator?.GetBoneTransform(HumanBodyBones.LeftFoot);
            //footBoneR = animator?.GetBoneTransform(HumanBodyBones.RightFoot);
            headBoneOrientation = Quaternion.Inverse(Quaternion.Euler(headOrientationEuler));
        }

        void OnAnimatorIK()
        {
            animator.SetLookAtWeight(1f, bodyWeight: 0f, headWeight: doHeadControl ? 1f : 0f, eyesWeight: doEyeControl ? 1f : 0f, clampWeight: 0f);
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
            animator.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, 1f);
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);
            animator.SetIKHintPositionWeight(AvatarIKHint.RightKnee, 1f);

            if (doHeadRoll && (headTarget != null))
            {
                // its not possible to set bone transform directly (locked by animator)
                // therefore, the rotation is applied relative to body (neck.up)
                Vector3 rotAxis;
                Quaternion lRot = headTarget.localRotation;
                rotAxis.x = lRot.x;
                rotAxis.y = lRot.y;
                rotAxis.z = lRot.z;
                rotAxis = headBoneOrientation * rotAxis;
                lRot.x = rotAxis.x;
                lRot.y = rotAxis.y;
                lRot.z = rotAxis.z;
                animator.SetBoneLocalRotation(HumanBodyBones.Head, lRot);
            }
            if ((doHeadControl || doEyeControl) && (lookAtTarget != null))
            {
                animator.SetLookAtPosition(lookAtTarget.position);
            }
            if (bodyTarget != null)
            {
                animator.bodyPosition = bodyTarget.position;
                animator.bodyRotation = bodyTarget.rotation;
            }
            if (leftHandTarget != null)
            {
                animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
                animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
            }
            if (rightHandTarget != null)
            {
                animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
                animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
            }
            if (leftFootTarget != null)
            {
                animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootTarget.position);
                animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootTarget.rotation);
                animator.SetIKHintPosition(AvatarIKHint.LeftKnee,
                    Vector3.Lerp(bodyTarget?.position ?? leftFootTarget.position + leftFootTarget.up, leftFootTarget.position, .5f) +
                        leftFootTarget.forward
                );
            }
            if (rightFootTarget != null)
            {
                animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootTarget.position);
                animator.SetIKRotation(AvatarIKGoal.RightFoot, rightFootTarget.rotation);
                animator.SetIKHintPosition(AvatarIKHint.RightKnee,
                    Vector3.Lerp(bodyTarget?.position ?? rightFootTarget.position + rightFootTarget.up, rightFootTarget.position, .5f) +
                        rightFootTarget.forward
                );
            }
        }
    }
}

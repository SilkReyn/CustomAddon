using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;


namespace Mocap
{
    // Allows sepearate head and eye targets but ignores rotation inherit by head's parents.
    public class CIkHeadOverride : MonoBehaviour
    {
        public Transform headTarget;
        public Transform eyeTarget;
        public Transform leftEyeBone;
        public Transform rightEyeBone;
        public Vector3 headOrientationEuler;
        public Vector3 leftEyeForward = Vector3.forward;
        public Vector3 rightEyeForward = Vector3.forward;
        

        private Quaternion headBoneOrientation = Quaternion.identity;
        private Quaternion rEyeOrientation = Quaternion.identity;
        private Quaternion lEyeOrientation = Quaternion.identity;
        private Transform headBone = null;


        private void Start()
        {
            if (null != rightEyeBone)
            {
                rEyeOrientation = rightEyeBone.localRotation;
            }
            if (null != leftEyeBone)
            {
                lEyeOrientation = leftEyeBone.localRotation;
            }
            headBone = leftEyeBone.parent;
            headBoneOrientation = Quaternion.Euler(headOrientationEuler);
            //if (null != headBone)
            //{
            //    headBoneOrientation = headBone.rotation;
            //}
        }

        private void LateUpdate()
        {
            if ((headTarget != null) && (null != headBone))
            {
                headBone.rotation = headTarget.rotation * headBoneOrientation;
            }
            if ((null != eyeTarget) && (null != leftEyeBone) && (null != rightEyeBone))
            {
                leftEyeBone.rotation = Quaternion.FromToRotation(leftEyeBone.TransformDirection(leftEyeForward), eyeTarget.position - leftEyeBone.position) * leftEyeBone.rotation;
                rightEyeBone.rotation = Quaternion.FromToRotation(rightEyeBone.TransformDirection(rightEyeForward), eyeTarget.position - rightEyeBone.position) * rightEyeBone.rotation;
            }
        }
    }
}

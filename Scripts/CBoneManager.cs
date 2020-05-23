using UnityEngine;


namespace Mocap
{
    public class CBoneManager : MonoBehaviour
    {
        public float globalSpringFactor;
        public float globalDragFactor;
        public Vector3 globalForce;
        public float globalRadius;
        public byte sampleRate;
        public ushort globalRotLim;
        public CBoneHinge[] bones;  // required for execution order
        //public byte forwardAxisIndex;
        
        public void Start()
        {
            if (!IsInvoking("UpdateBones"))
            {
                InvokeRepeating("UpdateBones", 0.02f, 1.0f/sampleRate);
            }
        }


        public void OnDisable()
        {
            CancelInvoke("UpdateBones");
        }


        private void OnEnable()
        {
            if (!IsInvoking("UpdateBones"))
            {
                InvokeRepeating("UpdateBones", 0.02f, 1.0f / sampleRate);
            }
        }


        private void UpdateBones()
        {
            if (bones == null)
            {
                return;
            }
            for (int i = 0; i < bones.Length; i++)
            {
                if (bones[i] == null)
                {
                    continue;
                }
                if (!bones[i].isUseEachBoneForceSettings)
                {
                    bones[i].worldAccel = globalForce;
                    bones[i].springFactor = globalSpringFactor;
                    bones[i].dragFactor = globalDragFactor;
                }
                bones[i].UpdateHinge();
            }
        }
    }
}

using UnityEngine;
using UnityEngine.XR;


namespace Mocap
{
    public class CHeadTracking : MonoBehaviour
    {
        public Vector3 Offset;
        // Update is called once per frame
        void Update()
        {
            if (XRDevice.isPresent)
            {
                Vector3 vec = Vector3.zero;
                
                // horizontal correction
                vec += transform.right * Offset.x;

                // vertical correction
                vec += transform.up * Offset.y;

                // Transversal correction
                vec += transform.forward * Offset.z;
                
                vec = InputTracking.GetLocalPosition(XRNode.Head) + vec;
                if (vec.y < 0.6f)
                {
                    vec.y = 0.6f;
                }
                transform.localPosition = vec;
                transform.localRotation = InputTracking.GetLocalRotation(XRNode.Head);
            }
        }
    }
}
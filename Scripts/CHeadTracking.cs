using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR;


namespace Mocap
{
    public class CHeadTracking : MonoBehaviour
    {
        public Vector3 Offset
        {
            get => _offset;
            set {
                _offset = value;
                _posCorr = Vector3.zero;
                // horizontal correction
                _posCorr += transform.right * value.x;

                // vertical correction
                _posCorr += transform.up * value.y;

                // Transversal correction
                _posCorr += transform.forward * value.z;

                if (_offset != value)
                    _offset = value;
            }
        }

        [SerializeField]
        private Vector3 _offset;

        InputDevice _head;
        Vector3 _posCorr = Vector3.zero;
        Vector3 _pos;
        Quaternion _rot;
        readonly InputFeatureUsage<Vector3> _USEPOS = CommonUsages.devicePosition;
        readonly InputFeatureUsage<Quaternion> _USEROT = CommonUsages.deviceRotation;


        void Start()
        {
            var headNodes = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.Head, headNodes);
            if (headNodes.Count > 0)
            {
                _head = headNodes.Find(headI => headI.isValid);
            }
        }


        void Awake()
        {
            InputDevices.deviceConnected += OnDeviceConnected;
        }

        
        // Update is called once per frame
        void Update()
        {
            if (_head.isValid && _head.TryGetFeatureValue(_USEPOS, out _pos) && _head.TryGetFeatureValue(_USEROT, out _rot))
            {
                _pos += _posCorr;
                if (_pos.y < 0.6f)
                {
                    _pos.y = 0.6f;
                }
                transform.localPosition = _pos;
                transform.localRotation = _rot;
            }
        }


        void OnDeviceConnected(InputDevice value)
        {
            Debug.Log("Device appeared: " + value.serialNumber);
            if (InputDeviceCharacteristics.HeadMounted == (value.characteristics & InputDeviceCharacteristics.HeadMounted))
            {
                _head = value;
            }
        }


#if UNITY_EDITOR
        private void OnValidate()
        {
            Offset = _offset;
        }
#endif
    }
}
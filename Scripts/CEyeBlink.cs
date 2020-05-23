using UnityEngine;
using System.Collections;

namespace Mocap
{
    public class CEyeBlink : MonoBehaviour
    {
        enum Status
        {
            Close,
            HalfClose,
            Open
        }

        
        public SkinnedMeshRenderer meshRenderer;
        
        public float ratio_Close = 85.0f;
        public float ratio_HalfClose = 20.0f;
        public float ratio_Open = 0.0f;
        public float timeBlink = 0.4f;
        public float interval = 3.0f;
        public int[] blinkShapeIndex;

        bool timerStarted = false;
        bool isBlink = false;
        float timeRemaining = 0.0f;
        float threshold = 0.3f;
        
        Status eyeStatus = Status.Open;

        
        void Start()
        {
            ResetTimer();
        }

        
        void Update()
        {
            if (!timerStarted)
            {
                eyeStatus = Status.Close;
                timerStarted = true;
            }
            if (timerStarted)
            {
                timeRemaining -= Time.deltaTime;
                if (timeRemaining <= 0.0f)
                {
                    eyeStatus = Status.Open;
                    ResetTimer();
                } else if (timeRemaining <= timeBlink * 0.3f)
                {
                    eyeStatus = Status.HalfClose;
                }
            }
        }


        void LateUpdate()
        {
            if (enabled && isBlink)
            {
                switch (eyeStatus)
                {
                case Status.Close:
                    SetCloseEyes();
                    break;
                case Status.HalfClose:
                    SetHalfCloseEyes();
                    break;
                case Status.Open:
                    SetOpenEyes();
                    isBlink = false;
                    break;
                }
            }
        }


        public void OnDisable()
        {
            StopCoroutine(RandomChange());
        }


        private void OnEnable()
        {
            StartCoroutine(RandomChange());
        }


        void ResetTimer()
        {
            timeRemaining = timeBlink;
            timerStarted = false;
        }


        void SetCloseEyes()
        {
            if (null == blinkShapeIndex || null == meshRenderer)
            {
                return;
            }
            foreach (var idx in blinkShapeIndex)
            {
                meshRenderer.SetBlendShapeWeight(idx, ratio_Close);
            }
        }


        void SetHalfCloseEyes()
        {
            if (null == blinkShapeIndex || null == meshRenderer)
            {
                return;
            }
            foreach (var idx in blinkShapeIndex)
            {
                meshRenderer.SetBlendShapeWeight(idx, ratio_HalfClose);
            }
        }


        void SetOpenEyes()
        {
            if (null == blinkShapeIndex || null == meshRenderer)
            {
                return;
            }
            foreach (var idx in blinkShapeIndex)
            {
                meshRenderer.SetBlendShapeWeight(idx, ratio_Open);
            }
        }

        
        IEnumerator RandomChange()
        {
            while (enabled)
            {
                float _seed = UnityEngine.Random.Range(0.0f, 1.0f);
                if (!isBlink)
                {
                    if (_seed > threshold)
                    {
                        isBlink = true;
                    }
                }
                yield return new WaitForSeconds(interval);
            }
        }
    }
}
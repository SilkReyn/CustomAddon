
#if !UNITY_EDITOR

using System.Linq;
//using System.Collections.Generic;
using System.IO;
using System.Reflection;

using UnityEngine;
//using UnityEngine.SceneManagement;

using IllusionPlugin;


namespace CustomAddon
{
    public class CPlugin : IPlugin
    {
        bool isSceneLoaded = false;

        public string Name { get; } = "Custom Addon";

        public string Version { get; } = "1.3.3";
        
        AssetBundle mAssetBundle = null;  // data needs to remain in memory, because of direct link
        GameObject mCustomPrefab = null;
        GameObject mAvatarBase = null;
        GameObject[] mAvatars = new GameObject[3];

        Mocap.CChestLink mAvatarProp = null;
        
        int mAvN = 0;

        public void OnApplicationQuit()
        {
            isSceneLoaded = false;
            if (mAvatarBase != null)
            {
                UnityEngine.Object.Destroy(mAvatarBase);
                mAvatarBase = null;
            }
        }


        public void OnApplicationStart()
        {
            AssetBundleCreateRequest assetHndl = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, "customaddon"));

            void onLoad(AsyncOperation aOp)
            {
                assetHndl.completed -= onLoad;
                if (!aOp.isDone)
                {
                    Debug.Log("Failed to load customaddon");
                    return;
                }

                mAssetBundle = assetHndl.assetBundle;
                if (mAssetBundle == null)
                {
                    Debug.Log("customaddon: Failed to load AssetBundle");
                    return;
                }
                mCustomPrefab = mAssetBundle.LoadAsset<GameObject>("AvatarsPref");

                if (mCustomPrefab == null)
                {
                    Debug.Log("customaddon: Could not load prefab asset");
                    return;
                }

                GameObject go;
                const int cullLayer = 8; // zero based, "NotVisibleInVR" (5: FirstPersonVisible)
                Transform parent = mCustomPrefab.transform.Find("IK_Link_Pivot/YohioLoid");
                for (int i = parent.childCount - 1; i >= 0; i--)
                {
                    go = parent.GetChild(i).gameObject;  //direct childs
                    if ((go != null) && ((go.name == "Hair_Mesh") || (go.name == "Head_Mesh")))
                    {
                        go.layer = cullLayer;
                    }
                }

                parent = mCustomPrefab.transform.Find("IK_Link_Pivot/Sakura");
                for (int i = parent.childCount-1; i >= 0; i--)
                {
                    go = parent.GetChild(i).gameObject;
                    if ((go != null) && !((go.name == "body") || (go.name == "Sakura")))
                    {
                        go.layer = cullLayer;
                    }
                }

                go = mCustomPrefab.transform.Find("IK_Link_Pivot/SonicoKB/hair_front")?.gameObject;
                if (null != go)
                {
                    go.layer = cullLayer;
                }
                go = mCustomPrefab.transform.Find("IK_Link_Pivot/SonicoKB/head")?.gameObject;
                if (null != go)
                {
                    go.layer = cullLayer;
                }

                var hands = mCustomPrefab.GetComponentsInChildren<Mocap.CHandTracking>();
                foreach (var hand in hands)
                {// Controller and game specific offset adjustment
                    hand.offset.x = hand.IsLeft ? -0.08f : 0.08f;
                    hand.offset.y = 0.03f;
                    hand.offset.z = -0.14f;
                }
                mAvatarProp = mAvatarBase.transform.Find("IK_Link_Chest")?.GetComponent<Mocap.CChestLink>();
            };

            assetHndl.completed += onLoad;

        }


        public void OnFixedUpdate()
        {
            
        }


        public void OnLevelWasInitialized(int level)
        {
            
        }


        public void OnLevelWasLoaded(int level)
        {
            isSceneLoaded = true;
        }


        public void OnUpdate()
        {
            if (Input.GetKeyUp(KeyCode.L) && (mCustomPrefab != null) && isSceneLoaded)
            {
                if (mAvatarBase == null)
                {
                    GameObject original = GameObject.Find("PlayerRoot/PlayerRootTranslation");
                    if (original != null)
                    {
                        mAvatarBase = UnityEngine.Object.Instantiate(mCustomPrefab, original.transform.position, original.transform.rotation, original.transform);
                    }
                    else
                    {
                        mAvatarBase = UnityEngine.Object.Instantiate(mCustomPrefab);
                    }

                    if (mAvatarBase != null)
                    {
                        UnityEngine.Object.DontDestroyOnLoad(mAvatarBase);  // Still gets unload or destroyed manually.
                        mAvatars[0] = mAvatarBase.transform.Find("IK_Link_Pivot/YohioLoid").gameObject;
                        mAvatars[1] = mAvatarBase.transform.Find("IK_Link_Pivot/Sakura").gameObject;
                        mAvatars[2] = mAvatarBase.transform.Find("IK_Link_Pivot/SonicoKB").gameObject;

                        GameObject.Find("TrackedInput/TrackingOrigin/HMD/Model")?.SetActive(false);
                        ToggleVrGloves(0);
                    }
                }
                else
                {
                    //mAvatarBase.SetActive(!mAvatarBase.activeSelf);
                    UnityEngine.Object.Destroy(mAvatarBase);
                    mAvatarBase = null;
                    GameObject.Find("TrackedInput/TrackingOrigin/HMD/Model")?.SetActive(true);
                    ToggleVrGloves(1);
                }
            }

            else if (Input.GetKeyUp(KeyCode.Less))
            {
                mAvatarBase.transform.rotation = Quaternion.Euler(0, -45, 0) * mAvatarBase.transform.rotation;
            }

            else if (Input.GetKeyUp(KeyCode.Greater))
            {
                mAvatarBase.transform.rotation = Quaternion.Euler(0, 45, 0) * mAvatarBase.transform.rotation;
            }

            //else if (Input.GetKey(KeyCode.C)) does not work
            //{
            //    //Camera dpc = Camera.allCameras.First(cam => cam.name == "Cam_DragonPerspective");
            //    GameObject dpo = GameObject.Find("PlayerRoot/SpecialCams/Cam_DragonPerspective");
            //    if (null != dpo)
            //    {
            //        dpo.transform.localPosition = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.RightHand);
            //        dpo.transform.localRotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.RightHand);
            //    }
            //}

            else if (Input.GetKeyUp(KeyCode.T))
            {
                SwapAvatar();
            }

            else if (Input.GetKeyUp(KeyCode.S))
            {
                GameObject go = mAvatarBase.transform.Find("SpotLight")?.gameObject;
                go.SetActive(!go.activeSelf);
            }

            else if (Input.GetKeyUp(KeyCode.UpArrow))
            {
                if (null != mAvatarProp)
                {
                    mAvatarProp.headPosOffset.y += .05f;
                }
            }

            else if (Input.GetKeyUp(KeyCode.DownArrow))
            {
                if (null != mAvatarProp)
                {
                    mAvatarProp.headPosOffset.y -= .05f;
                }
            }

            else if (Input.GetKeyUp(KeyCode.G)) 
            {
                ToggleVrGloves();
            }

            else if (Input.GetKeyUp(KeyCode.C))  // Configure and calibrate foot trackers
            {
                var leftTar = GameObject.Find("TrackedInput/TrackingOrigin/ViveTrackers/Tracker - Left Foot")?.transform;
                var rightTar = GameObject.Find("TrackedInput/TrackingOrigin/ViveTrackers/Tracker - Right Foot")?.transform;
                var feets = mAvatarBase.GetComponentsInChildren<Mocap.CFootLink>();
                if (null != feets)
                {
                    foreach (var foot in feets)
                    {
                        foot.altTarget = foot.isLeft ? leftTar : rightTar;
                        foot.CalibrateTrackedTargetNow();
                    }
                }
            }
        }

        void SwapAvatar()
        {
            mAvatars[mAvN]?.SetActive(false);
            mAvN = (mAvN < mAvatars.Length - 1) ? mAvN+1 : 0;
            mAvatars[mAvN]?.SetActive(true);
        }

        void ToggleVrGloves(byte offOnToggle = 2)
        {
            var obj = GameObject.Find("Controller-Left/vr_glove_left/vr_glove_model");
            obj?.SetActive(offOnToggle == 2 ? !obj.activeSelf : offOnToggle == 1);
            obj = GameObject.Find("Controller-Right/vr_glove_right/vr_glove_model");
            obj?.SetActive(offOnToggle == 2 ? !obj.activeSelf : offOnToggle == 1);
        }

    }// Class CPlugin
}// Namespace
#endif

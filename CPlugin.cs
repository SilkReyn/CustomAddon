
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

        public string Version { get; } = "1.0.0";
        
        AssetBundle mAssetBundle = null;  // data needs to remain in memory, because of direct link
        GameObject mCustomPrefab = null;
        GameObject mAvatarBase = null;
        GameObject[] mAvatars = new GameObject[3];
        //GameObject mRotatorHook = null;

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

                GameObject go;// = GameObject.Find("PlayerRoot/AvatarsRoot/AvatarPlaceholder/HeartYOffset/PlayerChest");
                const int cullLayer = 5; // zero based, 6th is "NotVisibleInVR"
                /*Transform[] children = mCustomPrefab.transform.Find("IK_Link_Pivot/YOHIOloid")?.GetComponentsInChildren<Transform>();
                foreach (Transform childTrns in children)  // all childs in hierachy
                {
                    childTrns.gameObject.layer = cullLayer;
                }*/
                Transform parent = mCustomPrefab.transform.Find("IK_Link_Pivot/YOHIOloid");
                for (int i = parent.childCount - 1; i >= 0; i--)
                {
                    go = parent.GetChild(i).gameObject;  //direct childs
                    if ((go != null) && ((go.name == "Hair") || (go.name == "Head")))
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

                go = mCustomPrefab.transform.Find("IK_Link_Pivot/SonicoKi/hair_front_curl")?.gameObject;
                if (null != go)
                {
                    go.layer = cullLayer;
                }
                go = mCustomPrefab.transform.Find("IK_Link_Pivot/SonicoKi/head")?.gameObject;
                if (null != go)
                {
                    go.layer = cullLayer;
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
            //mRotatorHook = GameObject.Find("PlayerRoot/SpecialCams/Rotate360CCW-Rotator");
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
                        GameObject.Find("PlayerRoot/AvatarsRoot/AvatarPlaceholder")?.SetActive(false);
                        UnityEngine.Object.DontDestroyOnLoad(mAvatarBase);  // Still gets unload or destroyed manually.
                        mAvatars[0] = mAvatarBase.transform.Find("IK_Link_Pivot/YOHIOloid").gameObject;
                        mAvatars[1] = mAvatarBase.transform.Find("IK_Link_Pivot/Sakura").gameObject;
                        mAvatars[2] = mAvatarBase.transform.Find("IK_Link_Pivot/SonicoKi").gameObject;
                    }
                }
                else
                {
                    //mAvatarBase.SetActive(!mAvatarBase.activeSelf);
                    UnityEngine.Object.Destroy(mAvatarBase);
                    mAvatarBase = null;
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

            //else if (Input.GetKey(KeyCode.Home)) unnecessary
            //{
            //    mAvatarBase.transform.localPosition = Vector3.zero;
            //    if (mAvatars[mAvN] != null)
            //    {
            //        mAvatars[mAvN].transform.localPosition = Vector3.zero;
            //    }
            //}

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

            //else if (Input.GetKey(KeyCode.R)) does not work
            //{
            //    if (null != mRotatorHook)
            //    {
            //        mRotatorHook.transform.rotation *= Quaternion.Euler(0, Time.deltaTime * 6f, 0);
            //    }
            //}
        }


        void SwapAvatar()
        {
            mAvatars[mAvN]?.SetActive(false);
            mAvN = (mAvN < mAvatars.Length - 1) ? mAvN+1 : 0;
            mAvatars[mAvN]?.SetActive(true);
        }

    };// Class CPlugin
}// Namespace
#endif

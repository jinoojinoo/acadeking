using UnityEngine;
using System;



namespace GameModule
{
    public class AndroidKeyManager : MonoBehaviour
    {
        #region create instance

        private static bool applicationIsQuitting = false;
        public static bool ApplicationIsQuitting
        {
            get { return applicationIsQuitting; }
            set { applicationIsQuitting = value; }
        }
        
        private static AndroidKeyManager instance = null;

        public static AndroidKeyManager Instance
        {
            get
            {
                if (applicationIsQuitting)
                    return null;

                if ( instance == null )
                {
                    instance = FindObjectOfType ( typeof ( AndroidKeyManager ) ) as AndroidKeyManager;
                }

                if ( instance == null )
                {
                    GameObject obj = new GameObject ( "AndroidKeyManager" );
                    instance = obj.AddComponent ( typeof ( AndroidKeyManager ) ) as AndroidKeyManager;
                }

                return instance;
            }
        }

        #endregion


        #region delegate declaration

        public delegate void Delegate_AndroidHomeKey();
        public delegate void Delegate_AndroidBackKey();
        public delegate void Delegate_AndroidMenuKey();

        public Delegate_AndroidHomeKey androidHomeKey;
        public Delegate_AndroidBackKey androidBackKey;
        public Delegate_AndroidMenuKey androidMenuKey;

        #endregion  //  delegate


        #region delegate add, remove

        /// <summary>
        /// add home key delegate
        /// </summary>
        /// <param name="del">add Delegate_AndroidHomeKey</param>
        public void AddDelegate_AndroidHomeKey(Delegate_AndroidHomeKey del)
        {
            if (null != androidHomeKey && androidHomeKey.Equals(del))
                return;

            androidHomeKey += del;

#if UNITY_EDITOR
            Debug.Log("AndroidKeyManager::AddDelegate_AndroidHomeKey - AndroidHomeKey count = " + androidHomeKey.GetInvocationList().Length);
#endif
        }

        /// <summary>
        /// remove home key delegate
        /// </summary>
        /// <param name="del">remove Delegate_AndroidHomeKey</param>
        public void RemoveDelegate_AndroidHomeKey(Delegate_AndroidHomeKey del)
        {
            androidHomeKey -= del;

#if UNITY_EDITOR
            int count = 0;
            if( null != androidHomeKey && null != androidHomeKey.GetInvocationList() )
                count = androidHomeKey.GetInvocationList().Length;

            Debug.Log("AndroidKeyManager::AddDelegate_AndroidHomeKey - AndroidHomeKey count = " + count);
#endif
        }

        /// <summary>
        /// add back key delegate
        /// </summary>
        /// <param name="del">add Delegate_AndroidBackKey</param>
        public void AddDelegate_AndroidBackKey(Delegate_AndroidBackKey del)
        {
            if (null != androidBackKey && androidBackKey.Equals(del))
                return;

            androidBackKey += del;

#if UNITY_EDITOR
            Debug.Log("AndroidKeyManager::AddDelegate_AndroidBackKey - AndroidBackKey count = " + androidBackKey.GetInvocationList().Length);
#endif
        }

        /// <summary>
        /// remove back key delegate
        /// </summary>
        /// <param name="del">remove Delegate_AndroidBackKey</param>
        public void RemoveDelegate_AndroidBackKey(Delegate_AndroidBackKey del)
        {
            androidBackKey -= del;

#if UNITY_EDITOR
            int count = 0;
            if (null != androidBackKey && null != androidBackKey.GetInvocationList())
                count = androidBackKey.GetInvocationList().Length;

            Debug.Log("AndroidKeyManager::RemoveDelegate_AndroidBackKey - AndroidBackKey count = " + count);
#endif
        }

        /// <summary>
        /// add home key delegate
        /// </summary>
        /// <param name="del">add Delegate_AndroidMenuKey</param>
        public void AddDelegate_AndroidMenuKey(Delegate_AndroidMenuKey del)
        {
            if (null != androidMenuKey && androidMenuKey.Equals(del))
                return;

            androidMenuKey += del;

#if UNITY_EDITOR
            Debug.Log("AndroidKeyManager::AddDelegate_AndroidMenuKey - AndroidMenuKey count = " + androidMenuKey.GetInvocationList().Length);
#endif
        }

        /// <summary>
        /// remove menu key delegate
        /// </summary>
        /// <param name="del">remove Delegate_AndroidMenuKey</param>
        public void RemoveDelegate_AndroidMenuKey(Delegate_AndroidMenuKey del)
        {
            androidMenuKey -= del;

#if UNITY_EDITOR
            int count = 0;
            if (null != androidMenuKey && null != androidMenuKey.GetInvocationList())
                count = androidMenuKey.GetInvocationList().Length;

            Debug.Log("AndroidKeyManager::RemoveDelegate_AndroidMenuKey - AndroidMenuKey count = " + count);
#endif
        }

        #endregion      //  delegate add, remove


        #region static - safe delegate remove

        /// <summary>
        /// remove home key delegate
        /// </summary>
        /// <param name="del">remove Delegate_AndroidHomeKey</param>
        public void Safe_RemoveDelegate_AndroidHomeKey(Delegate_AndroidHomeKey del)
        {
            if (null != instance)
                instance.RemoveDelegate_AndroidHomeKey(del);
        }

        /// <summary>
        /// remove back key delegate
        /// </summary>
        /// <param name="del">remove Delegate_AndroidBackKey</param>
        public static void Safe_RemoveDelegate_AndroidBackKey(Delegate_AndroidBackKey del)
        {
            if (null != instance)
                instance.RemoveDelegate_AndroidBackKey(del);
        }

        /// <summary>
        /// remove menu key delegate
        /// </summary>
        /// <param name="del">remove Delegate_AndroidMenuKey</param>
        public void Safe_RemoveDelegate_AndroidMenuKey(Delegate_AndroidMenuKey del)
        {
            if (null != instance)
                instance.RemoveDelegate_AndroidMenuKey(del);
        }

        #endregion      //  delegate add, remove


        protected void Awake()
        {
            DontDestroyOnLoad(this);
            instance = this;
        }


        #region Destory

        public void OnDestroy()
        {
            applicationIsQuitting = true;
        }

        #endregion


        #region LogOut Delegate callback

        protected void OnDelegate_LogOut()
        {
            androidHomeKey = null;
            androidBackKey = null;
            androidMenuKey = null;
        }

        #endregion

        public void OnClickHomeButton()
        {
            if (null == androidHomeKey)
                return;

            Delegate_AndroidHomeKey mTempAndroidHomeKey = null;

            //  null delegate remove
            {
                System.Delegate[] dels = androidHomeKey.GetInvocationList();

                for (int i = 0; i < dels.Length; i++)
                {
                    //  dels[i].Target - "null" string 으로 넘어온다. 혹시나 해서 추가해 놓음..
                    if (null == dels[i] || null == dels[i].Target)
                        continue;

                    if (dels[i].Target.ToString().Equals("null"))
                        continue;

                    mTempAndroidHomeKey += dels[i] as Delegate_AndroidHomeKey;
                }
            }

            //  call...
            if (null != mTempAndroidHomeKey)
                mTempAndroidHomeKey.Invoke();

            //  update delegate...
            androidHomeKey = mTempAndroidHomeKey;
        }

        public void OnClickEscapeButton()
        {
            //Del_AndroidBackKey.Invoke ();

            if (null == androidBackKey)
                return;

            Delegate_AndroidBackKey mTempAndroidBackKey = null;

            //  null delegate remove
            {
                System.Delegate[] dels = androidBackKey.GetInvocationList();

                for (int i = 0; i < dels.Length; i++)
                {
                    //  dels[i].Target - "null" string 으로 넘어온다. 혹시나 해서 추가해 놓음..
                    if (null == dels[i] || null == dels[i].Target)
                        continue;

                    if (dels[i].Target.ToString().Equals("null"))
                        continue;

                    mTempAndroidBackKey += dels[i] as Delegate_AndroidBackKey;
                }
            }

            //  update delegate...
            androidBackKey = mTempAndroidBackKey;

            if (null == androidBackKey)
                return;


            {
                Delegate[] del = androidBackKey.GetInvocationList();

                int count = del.Length;

                if (0 < count)
                {
                    ((Delegate_AndroidBackKey)del[count - 1]).Invoke();
                }
            }



            //	exit
            //UIEventManager.ApplicationIsQuitting = true;
            //Application.Quit();
        }

        public void OnClickMenuButton()
        {
            if (null == androidMenuKey)
                return;

            Delegate_AndroidMenuKey mTempAndroidMenuKey = null;

            //  null delegate remove
            {
                System.Delegate[] dels = androidMenuKey.GetInvocationList();

                for (int i = 0; i < dels.Length; i++)
                {
                    //  dels[i].Target - "null" string 으로 넘어온다. 혹시나 해서 추가해 놓음..
                    if (null == dels[i] || null == dels[i].Target)
                        continue;

                    if (dels[i].Target.ToString().Equals("null"))
                        continue;

                    mTempAndroidMenuKey += dels[i] as Delegate_AndroidMenuKey;
                }
            }

            //  call...
            if (null != mTempAndroidMenuKey)
                mTempAndroidMenuKey.Invoke();

            //  update delegate...
            androidMenuKey = mTempAndroidMenuKey;
        }

        // Update is called once per frame
        void LateUpdate()
        {
            if (InGameUIScene.Instance != null)
                return;

            //  home button
            if (Input.GetKeyDown(KeyCode.Home))
            {
                OnClickHomeButton();
            }
            //  back button
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnClickEscapeButton();
            }
            //  menu button
            else if (Input.GetKeyDown(KeyCode.Menu))
            {
                OnClickMenuButton();
            }
        }

    }


}   //  namespace NGUI_GameModule

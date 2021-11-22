using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

#if UNITY_EDITOR	
	using UnityEditor;
#endif

namespace AssetBundles
{	
	public class AssetBundleData
	{
		[XmlAttribute("AssetName")]
		public string AssetName;

		[XmlAttribute("BundleName")]
		public string BundleName;
	}

	public class AssetBundleTable
	{
		[XmlArray("AssetBundleDatas")]
		[XmlArrayItem("AssetBundleData")]
		public List<AssetBundleData> AssetBundleList;

		public bool CheckInBundleTable(string assetName)
		{
			bool res = false;

			for(int index = 0; index < AssetBundleList.Count; index++)
			{
				if(AssetBundleList[index].AssetName == assetName)
				{
					res = true;
					break;
				}
			}

			return res;
		}
		public string GetBundleName(string assetName)
		{
			for(int index = 0; index < AssetBundleList.Count; index++)
			{
				if(AssetBundleList[index].AssetName == assetName)
					return AssetBundleList[index].BundleName.ToLower();
			}

			return null;
		}
		public void Save(string filename)
		{
			if (filename.Contains(".xml") == false)
				filename += ".xml";

			string xmlfilename = filename;
			TextWriter stream = new StreamWriter(xmlfilename);
			XmlSerializer serializer = new XmlSerializer(typeof(AssetBundleTable));
			serializer.Serialize(stream, this);
			stream.Close();
		}

		public static AssetBundleTable Load(TextAsset xmlasset)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(AssetBundleTable));
			MemoryStream xmlstream = new MemoryStream(xmlasset.bytes);
			return serializer.Deserialize(xmlstream) as AssetBundleTable;
		}
	}

    public class AssetBundleLoader
    {
        public delegate void OnDownloadProgress(int progress);
        public OnDownloadProgress onDownloadProgress;

        string uri;
        string bundleName;
        Hash128 hash;
        bool useHash;

        public AssetBundle assetBundle
        {
            get;
            private set;
        }

        public static AssetBundleLoader Factory(string uri, string bundleName)
        {
            return new AssetBundleLoader(uri, bundleName);
        }

        public static AssetBundleLoader Factory(string uri, string bundleName, Hash128 hash)
        {
            return new AssetBundleLoader(uri, bundleName, hash);
        }

        private AssetBundleLoader(string uri, string bundleName)
        {
            this.uri = uri;
            this.bundleName = bundleName;

            useHash = false;
        }

        private AssetBundleLoader(string uri, string bundleName, Hash128 hash)
        {
            this.uri = uri;
            this.bundleName = bundleName;
            this.hash = hash;

            useHash = true;
        }

        public IEnumerator Load()
        {
            var bundleUrl = Path.Combine(this.uri, this.bundleName);

            UnityWebRequest webReq = null;

            Debug.LogError("useHash : " + useHash + ", bundleUrl : " + bundleUrl);
            if (useHash)
                webReq = UnityWebRequestAssetBundle.GetAssetBundle(bundleUrl, this.hash, 0);
            else
                webReq = UnityWebRequestAssetBundle.GetAssetBundle(bundleUrl);

            var handler = webReq.downloadHandler as DownloadHandlerAssetBundle;

            AsyncOperation ao = webReq.Send();

            if (webReq.isNetworkError)
            {
                Debug.LogError(webReq.error);
                yield break;
            }

            while (!ao.isDone)
            {
                Debug.Log("Download " + (ao.progress * 100).ToString());

                if (onDownloadProgress != null)
                    onDownloadProgress((int)(ao.progress * 100));

                yield return null;
            }

            if (ao.isDone)
            {
                Debug.Log("Download " + (ao.progress * 100).ToString());

                if (onDownloadProgress != null)
                    onDownloadProgress(100);

                yield return null;
            }

            assetBundle = handler.assetBundle;

            webReq = null;
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
        }
    }

	public class AssetBundleManager : SingletonMono<AssetBundleManager>
	{
		private Dictionary<string, AssetBundle> loadedAssetBundles = new Dictionary<string, AssetBundle>();
		private Dictionary<string, string> bundleVariants = new Dictionary<string, string>();
		private AssetBundleManifest bundleManifest = null;
		private string baseDownloadURL = "";
		private AssetBundleTable bundleTable = null;

		public delegate void OnDownloadProgress(string bundleName, int progress, int currentCount, int totalCount);
		public OnDownloadProgress onDownloadProgress;

		#if UNITY_EDITOR

			static int simulateAssetBundleInEditor = -1;
			const string kSimulatorAssetBundles = "SimulateAssetBundles";

		#endif

		public AssetBundleTable BundleTable
		{
			get
			{
				return bundleTable;
			}

			set
			{
				bundleTable = value;
			}
		}

		public AssetBundleManifest BundleManifest
		{
			get 
			{
				return bundleManifest;
			}
		}

		public bool IsReady
		{
			get
			{
				return !object.ReferenceEquals(bundleManifest, null);
			}
		}

		#if UNITY_EDITOR

			public static bool SimulateAssetBundleInEditor
			{
				get
				{
					if(simulateAssetBundleInEditor == -1)
						simulateAssetBundleInEditor = EditorPrefs.GetBool(kSimulatorAssetBundles, true) ? 1 : 0;

					return simulateAssetBundleInEditor != 0;
				}

				set
				{
					int newValue = value ? 1 : 0;

					if(newValue != simulateAssetBundleInEditor)
					{
						simulateAssetBundleInEditor = newValue;
						EditorPrefs.SetBool(kSimulatorAssetBundles, value);
					}
				}
			}

		#endif

		public void SetAssetBundleURL(string url)
		{
			baseDownloadURL = url + Utility.GetPlatformName() + "/";
		}

		public void Initialize()
		{
			#if UNITY_EDITOR

				Debug.Log("Simulation Mode: " + (SimulateAssetBundleInEditor ? "Enabled" : "Disabled"));

			#endif

			StartCoroutine(LoadManifest());
		}

		IEnumerator LoadManifest()
		{
			Debug.Log("Loading Manifest");

			var loader = AssetBundleLoader.Factory(baseDownloadURL, Utility.GetPlatformName());

			yield return loader.Load();

			var bundle = loader.assetBundle;

			if(bundle == null)
			{
				Debug.Log("Could not load asset bundle manifest.");
				yield break;
			}

			bundleManifest = (AssetBundleManifest)bundle.LoadAsset("AssetBundleManifest", typeof(AssetBundleManifest));

			bundle.Unload(false);

			if(!IsReady)
				Debug.Log("There was an error loading manifest");
			else
				Debug.Log("Manifest loaded successfully");
		}

		public void RegisterVariant(string bundleName, string variantName)
		{
			if(bundleVariants.ContainsValue(bundleName)) 
			{
				Debug.Log(string.Format("Variant for {0} cannot be added. {1} already registered. " +
				                    "Two vartiants of same bundle cannot be loaded (this is a safety check)", bundleName, variantName));
				return;
			}

			bundleVariants.Add(bundleName, variantName);
		}

		private string RemapVariantName(string assetBundleName)
		{
			string[] splitBundleName = assetBundleName.Split('.');
			string variant;

			if(!bundleVariants.TryGetValue(splitBundleName[0], out variant))
				return assetBundleName;

			string[] bundlesWithVariant = bundleManifest.GetAllAssetBundlesWithVariant();
			string newBundleName = splitBundleName [0] + "." + variant;

			if(System.Array.IndexOf(bundlesWithVariant, newBundleName) < 0 )
				return assetBundleName;

			return newBundleName;
		}

		public bool IsBundleLoaded(string bundleName)
		{
			#if UNITY_EDITOR

			if(SimulateAssetBundleInEditor)
				return true;
			else
			
			#endif
			
			return loadedAssetBundles.ContainsKey(bundleName);
		}

		public bool LoadBundle(string bundleName)
		{
			if(IsBundleLoaded(bundleName))
				return true;

			StartCoroutine(LoadBundleCoroutine(bundleName));

			return false;
		}

		IEnumerator LoadBundleCoroutine(string bundleName)
		{			
			bundleName = RemapVariantName(bundleName);

			var dependencies = bundleManifest.GetAllDependencies(bundleName);

			foreach(var dep in dependencies)
			{
				Debug.LogFormat("Loading asset bundle dependency {0} / {1}", dep, bundleManifest.GetAssetBundleHash(dep));

				if(!IsBundleLoaded(dep))
					yield return CallBundleLoader(dep);
			}

			if(!IsBundleLoaded(bundleName))
				yield return CallBundleLoader(bundleName);

		Debug.Log ("Finished loading " + bundleName);
		}

		IEnumerator CallBundleLoader(string bundleName)
		{
			var bundleLoader = AssetBundleLoader.Factory(baseDownloadURL, bundleName, bundleManifest.GetAssetBundleHash(bundleName));

			bundleLoader.onDownloadProgress += (int progress) =>
			{
				if(this.onDownloadProgress != null && BundleManifest != null)
					this.onDownloadProgress(bundleName, progress, loadedAssetBundles.Count + 1, BundleManifest.GetAllAssetBundles().Length);
			};

			yield return bundleLoader.Load();

			if(bundleLoader.assetBundle != null)
			{
				loadedAssetBundles.Add(bundleName, bundleLoader.assetBundle);	

				if(bundleName != "assetdata_scenes")
					ReArrangeShaderInAssetBundle(loadedAssetBundles[bundleName]);
			}
		}
			
		public void UnloadAllAssetBundles()
		{
			#if UNITY_EDITOR

			if(SimulateAssetBundleInEditor)
				return;

			#endif

			Debug.Log ("Unloading Bundles");

			foreach(KeyValuePair<string, AssetBundle> entry in loadedAssetBundles)
				entry.Value.Unload(true);

			loadedAssetBundles.Clear();

			Resources.UnloadUnusedAssets();
			System.GC.Collect();

			Debug.Log ("Bundles Unloaded");
		}

		public bool CheckBundleLoadByAssetName(string assetName)
		{
			bool exist = false;

			foreach(KeyValuePair<string, AssetBundle> bundle in loadedAssetBundles)
			{
				if(bundle.Key == bundleTable.GetBundleName(assetName))
				{
					exist = true;
					break;
				}
			}

			return exist;
		}

		public bool CheckBundleLoadByBundleName(string bundleName)
		{
			bool exist = false;

			foreach(KeyValuePair<string, AssetBundle> bundle in loadedAssetBundles)
			{
				if(bundle.Key == bundleName)
				{
					exist = true;
					break;
				}
			}

			return exist;
		}

		public void ReArrangeShaderInAllMaterials()
		{
			foreach(KeyValuePair<string, AssetBundle> bundle in loadedAssetBundles)
			{
				AssetBundle value = bundle.Value;

				var materials = value.LoadAllAssets<Material>();

				foreach(Material m in materials)
				{
					var shaderName = m.shader.name;
					var newShader = Shader.Find(shaderName);

					if(newShader == null)
						newShader = ResourceManager.Instance.LoadResourceFromCache<Shader>(shaderName);

					if(newShader != null)
						m.shader = newShader;
					else
						Debug.LogWarning("unable to refresh shader: "+shaderName+" in material "+m.name);
				}

			}
		}

		public void ReArrangeShaderInAssetBundle(AssetBundle assetBundle)
		{
			var materials = assetBundle.LoadAllAssets<Material>();

			foreach(Material m in materials)
			{
				var shaderName = m.shader.name;
				var newShader = Shader.Find(shaderName);

				if(newShader == null)
					newShader = ResourceManager.Instance.LoadResourceFromCache<Shader>(shaderName);

				if(newShader != null)
					m.shader = newShader;
				else
					Debug.LogWarning("unable to refresh shader: "+shaderName+" in material "+m.name);
			}
		}

		public string[] GetAllAssetName(string bundleName)
		{
			#if UNITY_EDITOR

			if(SimulateAssetBundleInEditor)
				return GetAllAssetNameInSimulateMode(bundleName.ToLower());
			else
				return GetAllAssetNameInBundle(bundleName.ToLower());

			#else

			return GetAllAssetNameInBundle(bundleName.ToLower());

			#endif
		}

		#if UNITY_EDITOR

		private string[] GetAllAssetNameInSimulateMode(string bundleName)
		{
			string[] assetNames = null;

			string[] bundlenames = AssetDatabase.GetAllAssetBundleNames();

			for(int index=0; index<bundlenames.Length; index++)
			{
				if(bundlenames[index] == bundleName)
				{
					assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
				}
			}

			return assetNames;
		}

		#endif

		private string[] GetAllAssetNameInBundle(string bundleName)
		{
			string[] assetNames = null;

			foreach(KeyValuePair<string, AssetBundle> bundle in loadedAssetBundles)
			{
				AssetBundle value = bundle.Value;

				if(value.name == bundleName.ToLower())
				{
					assetNames = value.GetAllAssetNames();
				}
			}

			return assetNames;
		}

		public T[] GetSubAssets<T>(string assetName) where T : Object
		{
			#if UNITY_EDITOR

			if(SimulateAssetBundleInEditor)
				return GetSubAssetsInSimulateMode<T>(assetName);
			else
				return GetSubAssetsInBundle<T>(assetName);

			#else

			return GetSubAssetsInBundle<T>(assetName);

			#endif
		}

		#if UNITY_EDITOR

		public T[] GetSubAssetsInSimulateMode<T>(string assetName) where T : Object
		{
			T[] loadObjects = null;

			string[] bundlenames = AssetDatabase.GetAllAssetBundleNames();

			for(int index=0; index<bundlenames.Length; index++)
			{
				string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(bundlenames[index], assetName);

				if(assetPaths.Length > 0)
				{
					Object[] objs = AssetDatabase.LoadAllAssetsAtPath(assetPaths[0]);

					if(objs.Length > 0)
					{
						loadObjects = new T[objs.Length];

						for(int oindex=0; oindex<objs.Length; oindex++)
						{
							if(objs[oindex].GetType() == typeof(T))
								loadObjects[oindex] = objs[oindex] as T;		
						}

						loadObjects = loadObjects.Where(l => l != null).ToArray();
					}					
				}
			}

			return loadObjects;
		}

		#endif

		public T[] GetSubAssetsInBundle<T>(string assetName) where T : Object
		{
			T[] loadObjects = null;

			foreach(KeyValuePair<string, AssetBundle> bundle in loadedAssetBundles)
			{
				AssetBundle value = bundle.Value;

				if(value != null && bundle.Key != "assetdata_scenes")
				{
					Object[] objs = value.LoadAssetWithSubAssets(assetName);

					if(objs.Length > 0)
					{
						loadObjects = new T[objs.Length];

						for(int oindex=0; oindex<objs.Length; oindex++)
						{
							if(objs[oindex].GetType() == typeof(T))
								loadObjects[oindex] = objs[oindex] as T;
						}

						loadObjects = loadObjects.Where(l => l != null).ToArray();
					}	
				}
			}

			return loadObjects;
		}

		public T GetAsset<T>(string assetName) where T : Object
		{
			#if UNITY_EDITOR

			if(SimulateAssetBundleInEditor)
				return GetAssetInSimulateMode<T>(assetName);
			else
				return GetAssetInBundle<T>(assetName);

			#else

			return GetAssetInBundle<T>(assetName);

			#endif
		}


		#if UNITY_EDITOR

		public static T GetAssetInSimulateMode<T>(string assetName) where T : Object
		{
			T loadObject = null;

			string[] bundlenames = AssetDatabase.GetAllAssetBundleNames();

			for(int index=0; index<bundlenames.Length; index++)
			{
				string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(bundlenames[index], assetName);

				if(assetPaths.Length > 1)
				{
					for(int aindex=0; aindex<assetPaths.Length; aindex++)
					{
						Object obj = AssetDatabase.LoadAssetAtPath(assetPaths[aindex], typeof(T));

						if(obj != null)
						{
							loadObject = obj as T;
							break;
						}
					}
				}
				else if (assetPaths.Length == 1)
				{
					loadObject = AssetDatabase.LoadAssetAtPath(assetPaths[0], typeof(T)) as T;
				}

				if(loadObject != null)
					break;
			}

			return loadObject;
		}

		#endif

		public T GetAssetInBundle<T>(string assetName) where T : Object
		{
			T loadObject = null;

			foreach(KeyValuePair<string, AssetBundle> bundle in loadedAssetBundles)
			{
				AssetBundle value = bundle.Value;

				if(value != null && bundle.Key != "assetdata_scenes")
				{
					if(typeof(T) == typeof(UIAtlas))
					{
						Object objatlas = value.LoadAsset(assetName, typeof(GameObject));

						if(objatlas != null)
						{
							loadObject = ((GameObject)objatlas).GetComponent<T>();
							break;
						}
					}
						
					Object obj = value.LoadAsset(assetName, typeof(T));

					if(obj != null)
					{
						loadObject = obj as T;
						break;
					}
				}
			}

			return loadObject;
		}

		public T GetAssetInBundle<T>(string bundleName, string assetName) where T : Object
		{
			T loadObject = null;
			AssetBundle bundle = null;

			loadedAssetBundles.TryGetValue(bundleName.ToLower(), out bundle);

			if(bundle == null)
				StartCoroutine(LoadBundleCoroutine(bundleName));
			
			if(bundle != null)
			{
				Object obj = bundle.LoadAsset(assetName, typeof(T));

				if(obj != null)
					loadObject = obj as T;
			}

			return loadObject;
		}
	}
}
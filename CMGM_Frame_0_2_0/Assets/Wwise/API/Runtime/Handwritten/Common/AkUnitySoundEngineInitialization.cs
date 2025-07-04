/*******************************************************************************
The content of this file includes portions of the proprietary AUDIOKINETIC Wwise
Technology released in source code form as part of the game integration package.
The content of this file may not be used without valid licenses to the
AUDIOKINETIC Wwise Technology.
Note that the use of the game engine is subject to the Unity(R) Terms of
Service at https://unity3d.com/legal/terms-of-service
 
License Usage
 
Licensees holding valid licenses to the AUDIOKINETIC Wwise Technology may use
this file in accordance with the end user license agreement provided with the
software or, alternatively, in accordance with the terms contained
in a written agreement between you and Audiokinetic Inc.
Copyright (c) 2025 Audiokinetic Inc.
*******************************************************************************/

using System;
using System.Threading.Tasks;

public class AkUnitySoundEngineInitialization
{
	protected static AkUnitySoundEngineInitialization m_Instance;

	public delegate void InitializationDelegate();
	public InitializationDelegate initializationDelegate;
	
	public delegate void ReInitializationDelegate();
	public ReInitializationDelegate reInitializationDelegate;
	
	public delegate void TerminationDelegate();
	public TerminationDelegate terminationDelegate;
	public static AkUnitySoundEngineInitialization Instance
	{
		get
		{
			if (m_Instance == null)
			{
				m_Instance = new AkUnitySoundEngineInitialization();
			}

			return m_Instance;
		}
	}

	public bool InitializeSoundEngine()
	{
		UnityEngine.Debug.LogFormat("WwiseUnity: Wwise(R) SDK Version {0}.", AkUnitySoundEngine.WwiseVersion);
		
#if UNITY_ANDROID && ! UNITY_EDITOR
		//Obtains the Android Java Object "currentActivity" in order to set it for the android io hook initialization
		try
		{
			// Get the current Activity using AndroidJavaClass
			using (var unityPlayer = new UnityEngine.AndroidJavaClass("com.unity3d.player.UnityPlayer"))
			{
				UnityEngine.AndroidJavaObject activity = unityPlayer.GetStatic<UnityEngine.AndroidJavaObject>("currentActivity");
				IntPtr rawActivityPtr = activity.GetRawObject(); // Get the JNI pointer

				// Pass the raw pointer to the native side
				AkUnitySoundEngine.SetAndroidActivity(rawActivityPtr);
			}
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError($"Failed to pass activity to native code: {ex.Message}");
		}
#endif
		var ActivePlatformSettings = AkWwiseInitializationSettings.ActivePlatformSettings;
		var initResult = AkUnitySoundEngine.Init(ActivePlatformSettings.AkInitializationSettings);
		if (initResult != AKRESULT.AK_Success)
		{
			UnityEngine.Debug.LogError($"WwiseUnity: Failed to initialize the sound engine. Reason: {initResult}");
			AkUnitySoundEngine.Term();
			return false;
		}

		if (AkUnitySoundEngine.InitSpatialAudio(ActivePlatformSettings.AkSpatialAudioInitSettings) != AKRESULT.AK_Success)
		{
			UnityEngine.Debug.LogWarning("WwiseUnity: Failed to initialize spatial audio.");
		}

		AkUnitySoundEngine.InitCommunication(ActivePlatformSettings.AkCommunicationSettings);

		var akBasePathGetterInstance = AkBasePathGetter.Get();
		var soundBankBasePath = akBasePathGetterInstance.SoundBankBasePath;
#if UNITY_OPENHARMONY && !UNITY_EDITOR
		soundBankBasePath = "rawfile://" + soundBankBasePath;
#endif
		if (string.IsNullOrEmpty(soundBankBasePath))
		{
			// this is a nearly impossible situation
			UnityEngine.Debug.LogError("WwiseUnity: Couldn't find SoundBanks base path. Terminating sound engine.");
			AkUnitySoundEngine.Term();
			return false;
		}

		var persistentDataPath = akBasePathGetterInstance.PersistentDataPath;
		var isBasePathSameAsPersistentPath = soundBankBasePath == persistentDataPath;
		
#if UNITY_ANDROID
		var canSetBasePath = !isBasePathSameAsPersistentPath;
		var canSetPersistentDataPath = true;
#else
		var canSetBasePath = true;
		var canSetPersistentDataPath = !isBasePathSameAsPersistentPath;
#endif

		if (canSetBasePath && AkUnitySoundEngine.SetBasePath(soundBankBasePath) != AKRESULT.AK_Success)
		{
#if !AK_WWISE_ADDRESSABLES
#if !UNITY_ANDROID || UNITY_EDITOR
#if UNITY_EDITOR
			var format = "WwiseUnity: Failed to set SoundBanks base path to <{0}>. Make sure SoundBank path is correctly set under Edit > Project Settings > Wwise > Editor > Asset Management.";
#else
			var format = "WwiseUnity: Failed to set SoundBanks base path to <{0}>. Make sure SoundBank path is correctly set under Edit > Project Settings > Wwise > Initialization.";
#endif
			// It might be normal for SetBasePath to return AK_PathNotFound on Android. Silence the error log to avoid confusion.
			UnityEngine.Debug.LogErrorFormat(format, soundBankBasePath);
#endif
#endif
		}

		if (canSetPersistentDataPath && !string.IsNullOrEmpty(persistentDataPath))
		{
			AkUnitySoundEngine.AddBasePath(persistentDataPath);
		}

		var decodedBankFullPath = akBasePathGetterInstance.DecodedBankFullPath;
		if (!string.IsNullOrEmpty(decodedBankFullPath) && AkWwiseInitializationSettings.Instance.IsDecodedBankEnabled)
		{
			// AkUnitySoundEngine.SetDecodedBankPath creates the folders for writing to (if they don't exist)
			AkUnitySoundEngine.SetDecodedBankPath(decodedBankFullPath);

			// Adding decoded bank path last to ensure that it is the first one used when writing decoded banks.
			AkUnitySoundEngine.AddBasePath(decodedBankFullPath);
		}

		AkUnitySoundEngine.SetCurrentLanguage(ActivePlatformSettings.InitialLanguage);

		AkCallbackManager.Init(ActivePlatformSettings.CallbackManagerInitializationSettings);
		UnityEngine.Debug.Log("WwiseUnity: Sound engine initialized successfully.");
		LoadInitBank();
		initializationDelegate?.Invoke();
		return true;
	}

	protected virtual void LoadInitBank()
	{
		AkBankManager.LoadInitBank();
	}

	protected virtual void ClearBanks()
	{
		AkUnitySoundEngine.ClearBanks();
	}

	protected virtual void ResetBanks()
	{
		AkBankManager.Reset();
	}


	public bool ResetSoundEngine(bool isInPlayMode)
	{
		if (isInPlayMode)
		{
			ClearBanks();
			LoadInitBank();
		}

		AkCallbackManager.Init(AkWwiseInitializationSettings.ActivePlatformSettings.CallbackManagerInitializationSettings);
		
		reInitializationDelegate?.Invoke();
		return true;
	}

	public bool ShouldKeepSoundEngineEnabled()
	{
#if UNITY_EDITOR
		bool result = true;
		if(UnityEditor.EditorApplication.isUpdating || UnityEditor.EditorApplication.isCompiling)
		{
			return false;
		}

		if (UnityEngine.Application.isPlaying)
		{
			return true;
		}
		if (!UnityEngine.Application.isPlaying)
		{
			result = AkWwiseEditorSettings.Instance.LoadSoundEngineInEditMode;
		}
#if UNITY_2019_3_OR_NEWER
		if(UnityEditor.EditorSettings.enterPlayModeOptionsEnabled)
		{
			result &= UnityEditor.EditorSettings.enterPlayModeOptions.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload);
		}
		else
		{
			result = false;
		}
#endif // UNITY_2019_3_OR_NEWER
		return result;
#else
		return false;
#endif // UNITY_EDITOR 
	}

	public void ResetSoundEngine()
    {
		TerminateSoundEngine(forceReset : true);
		if(ShouldKeepSoundEngineEnabled())
		{
			ResetSoundEngine(isInPlayMode : true);
			InitializeSoundEngine();
		}
    }

	public void TerminateSoundEngine()
    {
		TerminateSoundEngine(forceReset : false);
    }

	private void TerminateSoundEngine(bool forceReset)
	{
		if (!AkUnitySoundEngine.IsInitialized())
		{
			return;
		}

		if (ShouldKeepSoundEngineEnabled() && !forceReset)
		{
			return;
		}
		
		terminationDelegate?.Invoke();

		AkUnitySoundEngine.SetOfflineRendering(false);

		// Stop everything, and make sure the callback buffer is empty. We try emptying as much as possible, and wait 10 ms before retrying.
		// Callbacks can take a long time to be posted after the call to RenderAudio().
		AkUnitySoundEngine.StopAll();
		AkUnitySoundEngine.UnregisterAllGameObjects();
		ClearBanks();
		AkUnitySoundEngine.Term();

		// Make sure we have no callbacks left after Term. Some might be posted during termination.
		AkCallbackManager.PostCallbacks();

		AkCallbackManager.Term();
		ResetBanks();

		UnityEngine.Debug.Log("WwiseUnity: Sound engine terminated successfully.");
	}
}

#if WWISE_ADDRESSABLES_23_1_OR_LATER
[System.Obsolete(AkUnitySoundEngine.Deprecation_2024_1_0)]
public class AkSoundEngineInitialization  : AkUnitySoundEngineInitialization {}
#endif
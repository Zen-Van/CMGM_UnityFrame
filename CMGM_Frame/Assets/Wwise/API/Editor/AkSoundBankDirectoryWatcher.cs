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
using UnityEngine;

#if UNITY_EDITOR
namespace Wwise.API.Editor.SoundBankDirectoryWatcher.Common
{
	[UnityEditor.InitializeOnLoad]
	public class AkSoundBankDirectoryWatcher 
	{
		private static readonly AkSoundBankDirectoryWatcher instance;
		public static AkSoundBankDirectoryWatcher Instance { get { return instance; } }

		private bool emptyPathErrorWasLogged = false;
		private string soundBankDirectoryPath;
		private string platformName;
		private string languageName;
		private bool forceUpdate = false;
		private bool initCallbackRequired = false;
		
		private const int SecondsBetweenChecks = 3;
		private static System.DateTime s_lastFileCheck = System.DateTime.Now.AddSeconds(-SecondsBetweenChecks);
		private static System.DateTime s_lastSoundBankDirectoryUpdate = System.DateTime.MinValue;

		private AkSoundBankDirectoryWatcher()
		{
			platformName = AkBasePathGetter.GetPlatformName();
			UnityEditor.EditorApplication.update += OnEditorUpdate;
			WwiseProjectDatabase.SoundBankDirectoryUpdated += AkPlatformPluginList.ExecuteParse;
			forceUpdate = true;
			Execute();
		}
		
		static AkSoundBankDirectoryWatcher()
		{
			instance = new AkSoundBankDirectoryWatcher();				
		}

		~AkSoundBankDirectoryWatcher()
		{
			UnityEditor.EditorApplication.update -= OnEditorUpdate;
			WwiseProjectDatabase.SoundBankDirectoryUpdated -= AkPlatformPluginList.ExecuteParse;
		}

		private void Execute()
		{
			if (initCallbackRequired)
			{
				initCallbackRequired = false;
				WwiseProjectDatabase.PostInitCallback();
			}
			if (AkWwiseEditorSettings.Instance.WwiseProjectPath == "")
			{
				if (!emptyPathErrorWasLogged)
				{
					Debug.LogWarning("WwiseProjectPath is empty in the Wwise integration settings. Set it in order to enable the project database.");
					emptyPathErrorWasLogged = true;
				}
				return;
			}
			
			if (System.DateTime.Now.Subtract(s_lastFileCheck).Seconds >= SecondsBetweenChecks &&
			    !UnityEditor.EditorApplication.isCompiling && !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode && !AkUtilities.GeneratingSoundBanks)
			{
				var filename = System.IO.Path.Combine(AkBasePathGetter.GetWwiseRootOutputPath());
				var wProjPath = System.IO.Path.Combine(AkBasePathGetter.GetWwiseProjectPath());
				var time = System.IO.File.GetLastWriteTime(filename);
				Task.Run(() => InitProjectDB(filename, wProjPath, time));
			    s_lastFileCheck = System.DateTime.Now;
			}
		}

		private void OnEditorUpdate()
		{
			Execute();
		}
		
		private async Task InitProjectDB(string filename, string wProjPath, DateTime time)
		{
			if (time > s_lastSoundBankDirectoryUpdate || forceUpdate)
			{
				if (!await WwiseProjectDatabase.InitAsync(filename, platformName))
				{	
					var userWarning = "";
					if (!AkUtilities.IsSettingEnabled(wProjPath,"GenerateSoundBankJSON"))
					{
						userWarning = "Ensure that the Generate JSON Metadata option is enabled in the Wwise Project Settings on the SoundBanks tab, then regenerate the SoundBanks.";
					}
					else
					{
						userWarning = "Ensure that the SoundBanks Path in the Integration Settings matches the Root Output Path in the Wwise Project Settings on the SoundBanks tab, then regenerate the SoundBanks.";
					}

					UnityEngine.Debug.LogError("WwiseUnity: Cannot find ProjectInfo.json at " + filename + ". " + userWarning);
				}
				s_lastSoundBankDirectoryUpdate = time;
				forceUpdate = false;

				initCallbackRequired = true;
			}
		}
	}
}
#endif
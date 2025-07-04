using UnityEngine;

#if UNITY_EDITOR
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

[UnityEditor.InitializeOnLoad]
public class AkWwiseJSONBuilder : UnityEditor.AssetPostprocessor
{
	private static bool isSubscribedToInvokePopulate = false;
	private static readonly System.DateTime s_LastParsed = System.DateTime.MinValue;

	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
	{
		if (UnityEditor.AssetDatabase.IsAssetImportWorkerProcess())
		{
			return;
		}

		if (!isSubscribedToInvokePopulate)
		{
			WwiseProjectDatabase.SoundBankDirectoryUpdated += InvokePopulate;
			isSubscribedToInvokePopulate = true;
		}
		if (didDomainReload)
		{
			UnityEditor.EditorApplication.playModeStateChanged += PlayModeChanged;
		}
	}

	private static void PlayModeChanged(UnityEditor.PlayModeStateChange mode)
	{
		if (mode == UnityEditor.PlayModeStateChange.EnteredEditMode)
		{
			AkWwiseProjectInfo.Populate();
		}
	}

	public static void InvokePopulate()
	{
		Populate();
		WwiseProjectDatabase.SoundBankDirectoryUpdated -= InvokePopulate;
		isSubscribedToInvokePopulate = false;
	}

	public static bool Populate()
	{

		if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode || UnityEditor.EditorApplication.isCompiling)
		{
			return false;
		}

		try
		{
			var bChanged = false;
			WwiseSoundBankRefArray soundBankRefArray = new WwiseSoundBankRefArray();
			for (var i = 0; i < WwiseProjectDatabase.GetSoundBankCount(); i++)
			{
				var soundBankRef = soundBankRefArray[i];
				bChanged = SerialiseSoundBank(soundBankRef) || bChanged;
			}

			return bChanged;
		}
		catch (System.Exception e)
		{
			UnityEngine.Debug.Log("WwiseUnity: Exception occured while parsing SoundbanksInfo.xml: " + e.ToString());
			return false;
		}
	}

	private static bool SerialiseSoundBank(WwiseSoundBankRef soundBankRef)
	{
		var bChanged = false;
		var eventCount = soundBankRef.EventsCount;
		for (var i = 0; i < eventCount; i++)
		{
			var events = soundBankRef.Events[i];
			bChanged = SerialiseEventData(events) || bChanged;
		}

		return bChanged;
	}

	private static float GetFloatFromString(string s)
	{
		if (string.Compare(s, "Infinite") == 0)
		{
			return UnityEngine.Mathf.Infinity;
		}
		else
		{
			System.Globalization.CultureInfo CultInfo = System.Globalization.CultureInfo.CurrentCulture.Clone() as System.Globalization.CultureInfo;
			CultInfo.NumberFormat.NumberDecimalSeparator = ".";
			CultInfo.NumberFormat.CurrencyDecimalSeparator = ".";
			float Result;
			if(float.TryParse(s, System.Globalization.NumberStyles.Float, CultInfo, out Result))
			{
				return Result;
			}
			else
			{
				UnityEngine.Debug.Log("WwiseUnity: Could not parse float number " + s);
				return 0.0f;
			}
		}
	}

	private static bool SerialiseEventData(WwiseEventRef eventRef)
	{
		float maxAttenuation = eventRef.MaxAttenuation;
		var minDuration = eventRef.MinDuration;
		var maxDuration = eventRef.MaxDuration;
		var name = eventRef.Name;
		
		var bChanged = false;
		foreach (var wwu in AkWwiseProjectInfo.GetData().EventWwu)
		{
			var eventData = wwu.Find(name);
			if (eventData == null)
				continue;
		
			if (eventData.maxAttenuation != maxAttenuation)
			{
				eventData.maxAttenuation = maxAttenuation;
				bChanged = true;
			}
	
			if (eventData.minDuration != minDuration)
			{
				eventData.minDuration = minDuration;
				bChanged = true;
			}
		
			if (eventData.maxDuration != maxDuration)
			{
				eventData.maxDuration = maxDuration;
				bChanged = true;
			}
		}
		return bChanged;
	}
}
#endif
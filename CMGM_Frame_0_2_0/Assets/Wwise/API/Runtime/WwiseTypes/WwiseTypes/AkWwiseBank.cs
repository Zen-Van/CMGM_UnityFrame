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

#if !(UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.

#if AK_WWISE_ADDRESSABLES && UNITY_ADDRESSABLES
using AK.Wwise.Unity.WwiseAddressables;
#endif

namespace AK.Wwise
{
	[System.Serializable]
	///@brief This type can be used to load/unload SoundBanks.
	public class Bank : BaseType
	{
		public override WwiseObjectType WwiseObjectType { get { return WwiseObjectType.Soundbank; } }
		
		///@brief The reference to the SoundBank the script loads or unloads.
		public WwiseBankReference WwiseObjectReference;

		public override WwiseObjectReference ObjectReference
		{
			get { return WwiseObjectReference; }
			set { WwiseObjectReference = value as WwiseBankReference; }
		}

#if AK_WWISE_ADDRESSABLES && UNITY_ADDRESSABLES
		public override bool IsValid()
		{
			return base.IsValid() && WwiseObjectReference.AddressableBank !=null;
		}

		public void Load(bool decodeBank = false, bool saveDecodedBank = false)
		{
			if (IsValid())
			{
				AkAddressableBankManager.Instance.LoadBank(WwiseObjectReference.AddressableBank, decodeBank, saveDecodedBank, loadAsync: false);
			}
		}

		public void LoadAsync(AkCallbackManager.BankCallback callback = null)
		{	
			if(IsValid())
			{
				AkAddressableBankManager.Instance.LoadBank(WwiseObjectReference.AddressableBank, loadAsync: true);
			}
		}
		public void Unload()
		{
			if (IsValid())
			{
				AkAddressableBankManager.Instance.UnloadBank(WwiseObjectReference.AddressableBank);
			}
		}
#else
		///@brief Load the SoundBank
		///For more details about bank decoding, see 'Saving a Decoded SoundBank' in Lesson 3 of the Wwise 301 certification.
		///@param[in] decodeBank (Deprecated) Whether to decode the bank or not. 
		///@param[in] saveDecodedBank (Deprecated) Whether to save the decoded bank or not.
		public void Load(bool decodeBank = false, bool saveDecodedBank = false)
		{
			if (IsValid())
			{
				AkBankManager.LoadBank(Name, decodeBank, saveDecodedBank);
			}
		}

		///@brief Load the SoundBank asynchronously.
		///@param[in] callback A callback called when the loading operation is complete.
		public void LoadAsync(AkCallbackManager.BankCallback callback = null)
		{
			if (IsValid())
			{
				AkBankManager.LoadBankAsync(Name, callback);
			}
		}
		
		///@brief Unload the SoundBank.
		public void Unload()
		{
			if (IsValid())
			{
				AkBankManager.UnloadBank(Name);
			}
		}
#endif
	}
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
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

/// @brief Maintains the list of loaded SoundBanks loaded. This is currently used only with AkAmbient objects.
public static class AkBankManager
{
	private static readonly System.Collections.Generic.Dictionary<string, BankHandle> m_BankHandles =
		new System.Collections.Generic.Dictionary<string, BankHandle>();

	private static readonly System.Collections.Generic.List<BankHandle> BanksToUnload =
		new System.Collections.Generic.List<BankHandle>();

	public static void DoUnloadBanks()
	{
		var count = BanksToUnload.Count;
		for (var i = 0; i < count; ++i)
			BanksToUnload[i].UnloadBank();

		BanksToUnload.Clear();
	}

	internal static void Reset()
	{
		lock (m_BankHandles)
		{
			m_BankHandles.Clear();
		}

		BanksToUnload.Clear();
	}

	public static void ReloadAllBanks()
	{
		if (!AkUnitySoundEngine.IsInitialized())
		{
			return;
		}
		lock (m_BankHandles)
		{
			foreach (var bankHandle in m_BankHandles.Values)
			{
				if (bankHandle != null)
				{
					bankHandle.UnloadBank(false);
				}
			}

			UnloadInitBank();
			LoadInitBank(false);

			foreach (var bankHandle in m_BankHandles.Values)
			{
				if (bankHandle != null)
				{
					bankHandle.DoLoadBank();
				}
			}
		}
	}

	public static void LoadInitBank(bool doReset = true)
	{
		if (doReset)
		{
			Reset();
		}

		uint BankID;
		var result = AkUnitySoundEngine.LoadBank("Init.bnk", out BankID);
		if (result != AKRESULT.AK_Success)
		{
			UnityEngine.Debug.LogError("WwiseUnity: Failed load Init.bnk with result: " + result);
		}
	}

	public static void UnloadInitBank()
	{
		AkUnitySoundEngine.UnloadBank("Init.bnk", System.IntPtr.Zero);
	}

	/// Loads a SoundBank. This version blocks until the bank is loaded. See AK::SoundEngine::LoadBank for more information.
	public static uint LoadBank(string name, bool decodeBank, bool saveDecodedBank, AkBankTypeEnum bankType = AkBankTypeEnum.AkBankType_User)
	{
		BankHandle handle = null;
		lock (m_BankHandles)
		{
			if (m_BankHandles.TryGetValue(name, out handle))
			{
				// Bank already loaded, increment its ref count.
				handle.IncRef();
				return AkUnitySoundEngine.AK_INVALID_UNIQUE_ID;
			}

#if UNITY_SWITCH
			// No bank decoding on Nintendo switch
			handle = new BankHandle(name, bankType);
#else
			if (decodeBank && bankType != AkBankTypeEnum.AkBankType_User)
			{
				UnityEngine.Debug.LogError("Decoding Auto-generated SoundBanks is not supported.");
			}

			handle = decodeBank ? new DecodableBankHandle(name, saveDecodedBank) : new BankHandle(name, bankType);
#endif
			m_BankHandles.Add(name, handle);
		}
		return handle.LoadBank();
	}

	/// Loads a SoundBank. This version returns right away and loads in background. See AK::SoundEngine::LoadBank for more information.
	public static uint LoadBankAsync(string name, AkCallbackManager.BankCallback callback = null, AkBankTypeEnum bankType = AkBankTypeEnum.AkBankType_User)
	{
		BankHandle handle = null;
		lock (m_BankHandles)
		{
			if (m_BankHandles.TryGetValue(name, out handle))
			{
				// Bank already loaded, increment its ref count.
				handle.IncRef();
				return AkUnitySoundEngine.AK_INVALID_UNIQUE_ID;
			}

			handle = new AsyncBankHandle(name, callback, bankType);
			m_BankHandles.Add(name, handle);
		}
		return handle.LoadBank();
	}

	/// Unloads a SoundBank. See AK::SoundEngine::UnloadBank for more information.
	public static void UnloadBank(string name)
	{
		lock (m_BankHandles)
		{
			BankHandle handle = null;
			if (m_BankHandles.TryGetValue(name, out handle))
				handle.DecRef();
		}
	}

	public static void UnloadAllBanks()
	{
		lock (m_BankHandles)
		{
			foreach(var bank in m_BankHandles)
			{
				bank.Value.UnloadBank(false);
			}
			Reset();
		}
	}

	private class BankHandle
	{
		protected readonly string bankName;
		protected uint m_BankID;
		protected AkBankTypeEnum m_BankType;

		public BankHandle(string name, AkBankTypeEnum bankType)
		{
			bankName = name;
			m_BankType = bankType;
		}

		public int RefCount { get; private set; }

		/// Loads a bank. This version blocks until the bank is loaded. See AK::SoundEngine::LoadBank for more information.
		public virtual AKRESULT DoLoadBank()
		{
			return AkUnitySoundEngine.LoadBank(bankName, out m_BankID, (uint)m_BankType);
		}

		public uint LoadBank()
		{
			if (RefCount == 0 && !BanksToUnload.Remove(this))
			{
				var res = DoLoadBank();
				LogLoadResult(res);
			}

			IncRef();
			return m_BankID;
		}

		/// Unloads a bank.
		public virtual void UnloadBank(bool remove = true)
		{
			AkUnitySoundEngine.UnloadBank(m_BankID, System.IntPtr.Zero, null, null, (uint) m_BankType);

			if (remove)
			{
				lock (m_BankHandles)
					m_BankHandles.Remove(bankName);
			}
		}

		public void IncRef()
		{
			if (RefCount == 0)
				BanksToUnload.Remove(this);
			RefCount++;
		}

		public void DecRef()
		{
			RefCount--;
			if (RefCount == 0)
				BanksToUnload.Add(this);
		}

		protected void LogLoadResult(AKRESULT result)
		{
			if (result != AKRESULT.AK_Success && AkUnitySoundEngine.IsInitialized())
				UnityEngine.Debug.LogWarning("WwiseUnity: Bank " + bankName + " failed to load (" + result + ")");
		}
	}

	private class AsyncBankHandle : BankHandle
	{
		private readonly AkCallbackManager.BankCallback bankCallback;

		public AsyncBankHandle(string name, AkCallbackManager.BankCallback callback, AkBankTypeEnum bankType) : base(name, bankType)
		{
			bankCallback = callback;
		}

		private static void GlobalBankCallback(uint in_bankID, System.IntPtr in_pInMemoryBankPtr, AKRESULT in_eLoadResult, object in_Cookie)
		{
			var handle = (AsyncBankHandle)in_Cookie;
			var callback = handle.bankCallback;

			if (in_eLoadResult != AKRESULT.AK_Success)
			{
				handle.LogLoadResult(in_eLoadResult);

				if (in_eLoadResult != AKRESULT.AK_BankAlreadyLoaded)
					lock (m_BankHandles)
						m_BankHandles.Remove(handle.bankName);
			}

			if (callback != null)
				callback(in_bankID, in_pInMemoryBankPtr, in_eLoadResult, null);
		}

		/// Loads a bank.  This version returns right away and loads in background. See AK::SoundEngine::LoadBank for more information
		public override AKRESULT DoLoadBank()
		{
			return AkUnitySoundEngine.LoadBank(bankName, GlobalBankCallback, this, out m_BankID, (uint)m_BankType);
		}
	}

	private class DecodableBankHandle : BankHandle
	{
		private readonly bool decodeBank = true;
		private readonly string decodedBankPath;
		private readonly bool saveDecodedBank;

		public DecodableBankHandle(string name, bool save) : base(name, AkBankTypeEnum.AkBankType_User)
		{
			saveDecodedBank = save;

			var bankFileName = bankName + ".bnk";

			// test language-specific decoded file path
			var language = AkUnitySoundEngine.GetCurrentLanguage();
			var akBasePathGetterInstance =  AkBasePathGetter.Get();
			var decodedBankFullPath = akBasePathGetterInstance.DecodedBankFullPath;
			decodedBankPath = System.IO.Path.Combine(decodedBankFullPath, language);
			var decodedBankFilePath = System.IO.Path.Combine(decodedBankPath, bankFileName);

			var decodedFileExists = System.IO.File.Exists(decodedBankFilePath);
			if (!decodedFileExists)
			{
				// test non-language-specific decoded file path
				decodedBankPath = decodedBankFullPath;
				decodedBankFilePath = System.IO.Path.Combine(decodedBankPath, bankFileName);
				decodedFileExists = System.IO.File.Exists(decodedBankFilePath);
			}

			if (decodedFileExists)
			{
				try
				{
					var decodedFileTime = System.IO.File.GetLastWriteTime(decodedBankFilePath);
					var encodedBankFilePath = System.IO.Path.Combine(akBasePathGetterInstance.SoundBankBasePath, bankFileName);
					var encodedFileTime = System.IO.File.GetLastWriteTime(encodedBankFilePath);

					decodeBank = decodedFileTime <= encodedFileTime;
				}
				catch
				{
					// Assume the decoded bank exists, but is not accessible. Re-decode it anyway, so we do nothing.
				}
			}
		}

		/// Loads a bank. This version blocks until the bank is loaded. See AK::SoundEngine::LoadBank for more information.
		public override AKRESULT DoLoadBank()
		{
			if (decodeBank)
			{
				return AkUnitySoundEngine.LoadAndDecodeBank(bankName, saveDecodedBank, out m_BankID);
			}

			if (string.IsNullOrEmpty(decodedBankPath))
			{
				return AkUnitySoundEngine.LoadBank(bankName, out m_BankID, (uint) m_BankType);
			}

			var res = AkUnitySoundEngine.SetBasePath(decodedBankPath);
			if (res == AKRESULT.AK_Success)
			{
				res = AkUnitySoundEngine.LoadBank(bankName, out m_BankID, (uint)m_BankType);
				AkUnitySoundEngine.SetBasePath(AkBasePathGetter.Get().SoundBankBasePath);
			}
			return res;
		}

		/// Unloads a bank.
		public override void UnloadBank(bool remove = true)
		{
			if (decodeBank && !saveDecodedBank)
				AkUnitySoundEngine.PrepareBank(AkPreparationType.Preparation_Unload, m_BankID);
			else
				base.UnloadBank(remove);
		}
	}
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
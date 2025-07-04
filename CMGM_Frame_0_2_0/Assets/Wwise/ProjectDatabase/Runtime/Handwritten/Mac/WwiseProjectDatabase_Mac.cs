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
#if UNITY_EDITOR_OSX || (UNITY_STANDALONE_OSX && !UNITY_EDITOR)
using System.Threading.Tasks;

public partial class WwiseProjectDatabase
{
    static WwiseProjectDatabase()
    {
        PlatformName = "Mac";
    }
    public static void PostInitCallback()
    {
        SoundBankDirectoryUpdated?.Invoke();
    }
    public static event System.Action SoundBankDirectoryUpdated;
    public static async Task<bool> InitAsync(string inDirectoryPath, string inDirectoryPlatformName)
    {
       InitCheckUp(inDirectoryPath);
       if (!ProjectInfoExists)
           return false;

       try
       {
           await Task.Run(() => WwiseProjectDatabasePINVOKE_Mac.Init(inDirectoryPath, inDirectoryPlatformName));
       }
       catch (System.Exception e)
       {
           LogProjectDatabaseDLLException(e);
           throw;
       }
       return ProjectInfoExists;
    }

    public static void Init(string inDirectoryPath, string inDirectoryPlatformName, string language = null)
    {
        InitCheckUp(inDirectoryPath);
        if (!ProjectInfoExists)
            return;
        
        try
        {
            WwiseProjectDatabasePINVOKE_Mac.Init(inDirectoryPath, inDirectoryPlatformName);
            if (!string.IsNullOrEmpty(language))
            {
                WwiseProjectDatabasePINVOKE_Mac.SetCurrentLanguage(language);
            }
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }

    public static void SetCurrentPlatform(string inDirectoryPlatformName)
    {
        try
        {
            WwiseProjectDatabasePINVOKE_Mac.SetCurrentPlatform(inDirectoryPlatformName);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }

    public static void SetCurrentLanguage(string inLanguageName)
    {
        try
        {
            WwiseProjectDatabasePINVOKE_Mac.SetCurrentLanguage(inLanguageName);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }

    public static string StringFromIntPtrString(System.IntPtr ptr)
    {
        return System.Runtime.InteropServices.Marshal.PtrToStringAnsi(ptr);
    }
    
    /*
     * SoundBanks
     */

    public static global::System.IntPtr GetSoundBankRefString(string soundBankName, string soundBankType)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Mac.GetSoundBankRefString(soundBankName, soundBankType);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static global::System.IntPtr GetAllSoundBanksRef()
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Mac.GetAllSoundBanksRef();
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static uint GetSoundBankCount()
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Mac.GetSoundBankCount();
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static global::System.IntPtr GetSoundBankRefIndex(global::System.IntPtr soundBankArrayRef, int index)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Mac.GetSoundBankRefIndex(soundBankArrayRef, index);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static void DeleteSoundBanksArrayRef(global::System.IntPtr soundBankArrayRef)
    {
        try
        {
            WwiseProjectDatabasePINVOKE_Mac.DeleteSoundBanksArrayRef(soundBankArrayRef);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static string GetSoundBankName(global::System.IntPtr soundBankRefPtr)
    {
        try
        {
            return WwiseProjectDatabase.StringFromIntPtrString(WwiseProjectDatabasePINVOKE_Mac.GetSoundBankName(soundBankRefPtr));
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static string GetSoundBankPath(global::System.IntPtr soundBankRefPtr)
    {
        try
        {
            return WwiseProjectDatabase.StringFromIntPtrString(WwiseProjectDatabasePINVOKE_Mac.GetSoundBankPath(soundBankRefPtr));
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static string GetSoundBankLanguage(global::System.IntPtr soundBankRefPtr)
    {
        try
        {
            return WwiseProjectDatabase.StringFromIntPtrString(WwiseProjectDatabasePINVOKE_Mac.GetSoundBankLanguage(soundBankRefPtr));
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static uint GetSoundBankLanguageId(global::System.IntPtr soundBankRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Mac.GetSoundBankLanguageId(soundBankRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static System.IntPtr GetSoundBankGuid(global::System.IntPtr soundBankRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Mac.GetSoundBankGuid(soundBankRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static uint GetSoundBankShortId(global::System.IntPtr soundBankRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Mac.GetSoundBankShortId(soundBankRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static bool IsUserBank(global::System.IntPtr soundBankRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Mac.IsUserBank(soundBankRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static bool IsInitBank(global::System.IntPtr soundBankRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Mac.IsInitBank(soundBankRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static bool IsSoundBankValid(global::System.IntPtr soundBankRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Mac.IsSoundBankValid(soundBankRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }

    public static global::System.IntPtr GetSoundBankMedia(global::System.IntPtr soundBankRefPtr, int index)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Mac.GetSoundBankMedia(soundBankRefPtr, index);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }

    public static uint GetSoundBankMediasCount(global::System.IntPtr soundBankRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Mac.GetSoundBankMediasCount(soundBankRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }

    public static global::System.IntPtr GetSoundBankEvent(global::System.IntPtr soundBankRefPtr, int index)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Mac.GetSoundBankEvent(soundBankRefPtr, index);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }

    public static uint GetSoundBankEventsCount(global::System.IntPtr soundBankRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Mac.GetSoundBankEventsCount(soundBankRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }

    public static void DeleteSoundBankRef(global::System.IntPtr soundBankRefPtr)
    {
        try
        {
            WwiseProjectDatabasePINVOKE_Mac.DeleteSoundBankRef(soundBankRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    
    /*
     * Medias
    */
    
    public static string GetMediaName(global::System.IntPtr mediaRefPtr)
    {
        try
        {
            return WwiseProjectDatabase.StringFromIntPtrString(WwiseProjectDatabasePINVOKE_Mac.GetMediaName(mediaRefPtr));
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static string GetMediaPath(global::System.IntPtr mediaRefPtr)
    {
        try
        {
            return WwiseProjectDatabase.StringFromIntPtrString(WwiseProjectDatabasePINVOKE_Mac.GetMediaPath(mediaRefPtr));
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static uint GetMediaShortId(global::System.IntPtr mediaRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Mac.GetMediaShortId(mediaRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static string GetMediaLanguage(global::System.IntPtr mediaRefPtr)
    {
        try
        {
            return WwiseProjectDatabase.StringFromIntPtrString(WwiseProjectDatabasePINVOKE_Mac.GetMediaLanguage(mediaRefPtr));
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static bool GetMediaIsStreaming(global::System.IntPtr mediaRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Mac.GetMediaIsStreaming(mediaRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static uint GetMediaLocation(global::System.IntPtr mediaRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Mac.GetMediaLocation(mediaRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static string GetMediaCachePath(global::System.IntPtr mediaRefPtr)
    {
        try
        {
            return WwiseProjectDatabase.StringFromIntPtrString(WwiseProjectDatabasePINVOKE_Mac.GetMediaCachePath(mediaRefPtr));
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static void DeleteMediaRef(global::System.IntPtr mediaRefPtr)
    {
        try
        {
            WwiseProjectDatabasePINVOKE_Mac.DeleteMediaRef(mediaRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    
    /*
     * Events
    */
    
    public static global::System.IntPtr GetEventRefString(string soundBankName)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Mac.GetEventRefString(soundBankName);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static string GetEventName(global::System.IntPtr eventRefPtr)
    {
        try
        {
            return WwiseProjectDatabase.StringFromIntPtrString(WwiseProjectDatabasePINVOKE_Mac.GetEventName(eventRefPtr));
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static string GetEventPath(global::System.IntPtr eventRefPtr)
    {
        try
        {
            return WwiseProjectDatabase.StringFromIntPtrString(WwiseProjectDatabasePINVOKE_Mac.GetEventPath(eventRefPtr));
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static System.IntPtr GetEventGuid(global::System.IntPtr soundBankRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Mac.GetEventGuid(soundBankRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static uint GetEventShortId(global::System.IntPtr soundBankRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Mac.GetEventShortId(soundBankRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static float GetEventMaxAttenuation(global::System.IntPtr soundBankRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Mac.GetEventMaxAttenuation(soundBankRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static float GetEventMinDuration(global::System.IntPtr soundBankRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Mac.GetEventMinDuration(soundBankRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static float GetEventMaxDuration(global::System.IntPtr soundBankRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Mac.GetEventMaxDuration(soundBankRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static global::System.IntPtr GetEventMedia(global::System.IntPtr soundBankRefPtr, int index)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Mac.GetEventMedia(soundBankRefPtr, index);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }    
    public static uint GetEventMediasCount(global::System.IntPtr soundBankRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Mac.GetEventMediasCount(soundBankRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static void DeleteEventRef(global::System.IntPtr eventRefPtr)
    {
        try
        {
            WwiseProjectDatabasePINVOKE_Mac.DeleteEventRef(eventRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    
    /*
     * Platform
    */
    
    public static global::System.IntPtr GetPlatformRef(string soundBankName)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Mac.GetPlatformRef(soundBankName);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static string GetPlatformName(global::System.IntPtr platformRefPtr)
    {
        try
        {
            return WwiseProjectDatabase.StringFromIntPtrString(WwiseProjectDatabasePINVOKE_Mac.GetPlatformName(platformRefPtr));
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static global::System.IntPtr GetPlatformGuid(global::System.IntPtr platformRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Mac.GetPlatformGuid(platformRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static void DeletePlatformRef(global::System.IntPtr platformRefPtr)
    {
        try
        {
            WwiseProjectDatabasePINVOKE_Mac.DeletePlatformRef(platformRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }

    /**
    * Plugin
    */

    public static global::System.IntPtr GetAllPluginRef()
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Mac.GetAllPluginRef();
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static uint GetPluginCount()
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Mac.GetPluginCount();
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static global::System.IntPtr GetPluginRefIndex(global::System.IntPtr soundBankArrayRef, int index)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Mac.GetPluginRefIndex(soundBankArrayRef, index);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static uint GetPluginId(global::System.IntPtr pluginRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Mac.GetPluginId(pluginRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static string GetPluginName(global::System.IntPtr pluginRefPtr)
    {
        try
        {
            return WwiseProjectDatabase.StringFromIntPtrString(WwiseProjectDatabasePINVOKE_Mac.GetPluginName(pluginRefPtr));
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static string GetPluginDLL(global::System.IntPtr pluginRefPtr)
    {
        try
        {
            return WwiseProjectDatabase.StringFromIntPtrString(WwiseProjectDatabasePINVOKE_Mac.GetPluginDLL(pluginRefPtr));
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static string GetPluginStaticLib(global::System.IntPtr pluginRefPtr)
    {
        try
        {
            return WwiseProjectDatabase.StringFromIntPtrString(WwiseProjectDatabasePINVOKE_Mac.GetPluginStaticLib(pluginRefPtr));
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static void DeletePluginRef(global::System.IntPtr pluginRefPtr)
    {
        try
        {
            WwiseProjectDatabasePINVOKE_Mac.DeletePluginRef(pluginRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static void DeletePluginArrayRef(global::System.IntPtr pluginArrayRefPtr)
    {
        try
        {
            WwiseProjectDatabasePINVOKE_Mac.DeletePluginArrayRef(pluginArrayRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
}
#endif
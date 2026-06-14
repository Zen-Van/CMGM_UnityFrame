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
using System.Runtime.InteropServices;
public class WwiseProjectDatabasePINVOKE_Mac
{
    /*
     * Utility
    */
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern void Init(string InDirectoryPath, string InDirectoryPlatformName);

    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetCurrentPlatform(string InDirectoryPlatformName);

    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetCurrentLanguage(string InLanguageName);
    
    /*
     * SoundBank Ref
    */
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern global::System.IntPtr GetAllSoundBanksRef();
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern uint GetSoundBankCount();
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern global::System.IntPtr GetSoundBankRefIndex(global::System.IntPtr soundBankRefPtr, int index);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern void DeleteSoundBanksArrayRef(global::System.IntPtr soundBankRefPtr);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern global::System.IntPtr GetSoundBankRefString(string soundBankName, string soundBankType);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern System.IntPtr GetSoundBankName(global::System.IntPtr soundBankRefPtr);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern System.IntPtr GetSoundBankPath(global::System.IntPtr soundBankRefPtr);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern System.IntPtr GetSoundBankLanguage(global::System.IntPtr soundBankRefPtr);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern uint GetSoundBankLanguageId(global::System.IntPtr soundBankRefPtr);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern System.IntPtr GetSoundBankGuid(global::System.IntPtr soundBankRefPtr);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern uint GetSoundBankShortId(global::System.IntPtr soundBankRefPtr);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool IsUserBank(global::System.IntPtr soundBankRefPtr);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool IsInitBank(global::System.IntPtr soundBankRefPtr);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool IsSoundBankValid(global::System.IntPtr soundBankRefPtr);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern global::System.IntPtr GetSoundBankMedia(global::System.IntPtr soundBankRefPtr, int index);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern uint GetSoundBankMediasCount(global::System.IntPtr soundBankRefPtr);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern global::System.IntPtr GetSoundBankEvent(global::System.IntPtr soundBankRefPtr, int index);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern uint GetSoundBankEventsCount(global::System.IntPtr soundBankRefPtr);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern void DeleteSoundBankRef(global::System.IntPtr soundBankRefPtr);
    
    /*
     * Media Ref
    */
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern System.IntPtr GetMediaName(global::System.IntPtr mediaRef);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern System.IntPtr GetMediaPath(global::System.IntPtr mediaRef);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern uint GetMediaShortId(global::System.IntPtr mediaRef);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern System.IntPtr GetMediaLanguage(global::System.IntPtr mediaRef);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool GetMediaIsStreaming(global::System.IntPtr mediaRef);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern uint GetMediaLocation(global::System.IntPtr mediaRef);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern System.IntPtr GetMediaCachePath(global::System.IntPtr mediaRef);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern void DeleteMediaRef(global::System.IntPtr mediaRefPtr);
    
    /*
     * Event Ref
    */
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern global::System.IntPtr GetEventRefString(string InString);

    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern System.IntPtr GetEventName(global::System.IntPtr eventRefPtr);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern System.IntPtr GetEventPath(global::System.IntPtr eventRefPtr);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern System.IntPtr GetEventGuid(global::System.IntPtr eventRefPtr);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern uint GetEventShortId(global::System.IntPtr eventRefPtr);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern float GetEventMaxAttenuation(global::System.IntPtr eventRefPtr);

    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern float GetEventMinDuration(global::System.IntPtr eventRefPtr);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern float GetEventMaxDuration(global::System.IntPtr eventRefPtr);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern global::System.IntPtr GetEventMedia(global::System.IntPtr soundBankRefPtr, int index);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern uint GetEventMediasCount(global::System.IntPtr eventRefPtr);
   
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern void DeleteEventRef(global::System.IntPtr mediaRefPtr);
    
    /*
     * Platform Ref
    */
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern global::System.IntPtr GetPlatformRef(string InString);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern System.IntPtr GetPlatformName(global::System.IntPtr soundBankRefPtr);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern System.IntPtr GetPlatformGuid(global::System.IntPtr soundBankRefPtr);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern void DeletePlatformRef(global::System.IntPtr mediaRefPtr);

    /*
    * Plugin Ref
    */

    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern global::System.IntPtr GetAllPluginRef();
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern uint GetPluginCount();
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern global::System.IntPtr GetPluginRefIndex(global::System.IntPtr soundBankRefPtr, int index);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern uint GetPluginId(global::System.IntPtr pluginRefPtr);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern System.IntPtr GetPluginName(global::System.IntPtr pluginRefPtr);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern System.IntPtr GetPluginDLL(global::System.IntPtr pluginRefPtr);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern System.IntPtr GetPluginStaticLib(global::System.IntPtr pluginRefPtr);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern void DeletePluginRef(global::System.IntPtr pluginRefPtr);
    
    [DllImport("WwiseProjectDatabase", CallingConvention = CallingConvention.Cdecl)]
    public static extern void DeletePluginArrayRef(global::System.IntPtr pluginArrayRefPtr);
}
#endif
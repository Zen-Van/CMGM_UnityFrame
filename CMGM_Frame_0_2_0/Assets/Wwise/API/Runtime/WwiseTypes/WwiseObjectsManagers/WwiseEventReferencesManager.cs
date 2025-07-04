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

using System.Collections.Concurrent;
using System.Collections.Generic;
#if AK_WWISE_ADDRESSABLES && UNITY_ADDRESSABLES
using AK.Wwise.Unity.WwiseAddressables;
#endif

namespace Wwise.API.Runtime.WwiseTypes.WwiseObjectsManagers
{
    public class WwiseEventReferencesManager
    {
        private ConcurrentDictionary<string, WwiseEventReference> m_wwiseEventReferences =
            new ConcurrentDictionary<string, WwiseEventReference>();
        
        private static WwiseEventReferencesManager instance;
        public static WwiseEventReferencesManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new WwiseEventReferencesManager();
                }
                return instance;
            }
            private set { Instance = value; }
        }

        public void AddReference(WwiseEventReference eventReference)
        {
            m_wwiseEventReferences.TryAdd(eventReference.DisplayName, eventReference);
        }

        public void RemoveReference(WwiseEventReference eventReference)
        {
            m_wwiseEventReferences.TryRemove(eventReference.DisplayName, out eventReference);
        }
        
        public void SetLanguageAndReloadLocalizedBanks(string language, List<string> eventNames = null)
        {
            var eventReferences = new List<WwiseEventReference>();
            foreach (var eventReference in m_wwiseEventReferences.Values)
            {
                if (eventNames == null || (eventNames.Count > 0 && eventNames.Contains(eventReference.DisplayName)))
                {
                    eventReferences.Add(eventReference);
                    eventReference.UnloadAutoBank();
                }
            }
#if AK_WWISE_ADDRESSABLES && UNITY_ADDRESSABLES
#if WWISE_ADDRESSABLES_24_1_OR_LATER
            AkAddressableBankManager.Instance.SetLanguageAndReloadLocalizedBanks(language, false);
#else
            AkAddressableBankManager.Instance.SetLanguageAndReloadLocalizedBanks(language);
#endif
#else
            AkBankManager.DoUnloadBanks();
            AkUnitySoundEngine.SetCurrentLanguage(language);
            AkUnitySoundEngine.RenderAudio();
#endif
            foreach (var eventReference in eventReferences)
            {
                eventReference.LoadAutoBank();
            }
        }
    }
}
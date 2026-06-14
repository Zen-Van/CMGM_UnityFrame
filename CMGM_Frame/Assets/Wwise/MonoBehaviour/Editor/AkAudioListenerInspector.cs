using UnityEditor;
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

[UnityEditor.CanEditMultipleObjects]
[UnityEditor.CustomEditor(typeof(AkAudioListener), true)]
public class AkAudioListenerInspector : UnityEditor.Editor
{
	private UnityEditor.SerializedProperty m_isDefaultListener;
	private AkAudioListener m_Listener;

	private void OnEnable()
	{
		m_isDefaultListener = serializedObject.FindProperty("isDefaultListener");
		m_Listener = target as AkAudioListener;
	}

	public override void OnInspectorGUI()
	{
		using (new UnityEditor.EditorGUILayout.VerticalScope("box"))
		{
			UnityEditor.EditorGUI.BeginChangeCheck();
			UnityEditor.EditorGUILayout.PropertyField(m_isDefaultListener);
			using (new UnityEditor.EditorGUILayout.VerticalScope("box"))
			{
				m_Listener.bOverrideScalingFactor = UnityEditor.EditorGUILayout.Toggle("Override Attenuation Scaling Factor:", m_Listener.bOverrideScalingFactor);
				if (m_Listener.bOverrideScalingFactor)
				{
					m_Listener.ScalingFactor = UnityEditor.EditorGUILayout.FloatField("Attenuation Scaling Factor", m_Listener.ScalingFactor);
				}
			}
			if (UnityEditor.EditorGUI.EndChangeCheck())
			{
				if (m_Listener.ScalingFactor <= 0)
				{
					m_Listener.ScalingFactor = 0;
				}
				else
				{
					AkAudioListener selectedTarget = (AkAudioListener)target;
					if (selectedTarget.gameObject.GetComponent<AkGameObj>().enabled)
					{
						AkUnitySoundEngine.SetScalingFactor(m_Listener.gameObject, m_Listener.ScalingFactor);
					}
				}
				UnityEditor.EditorUtility.SetDirty(serializedObject.targetObject);
			}
		}
	}
}
#endif
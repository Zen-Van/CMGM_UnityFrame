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

using UnityEditor;
using UnityEngine;

namespace AK.Wwise.Editor
{
	[UnityEditor.CustomPropertyDrawer(typeof(Event))]
	public class EventDrawer : BaseTypeDrawer
	{
		protected override WwiseObjectType WwiseObjectType { get { return WwiseObjectType.Event; } }
		
		public override void OnGUI(UnityEngine.Rect position, UnityEditor.SerializedProperty property,
			UnityEngine.GUIContent label)
		{
			position.height = UnityEditor.EditorGUI.GetPropertyHeight(property, label, true);
			base.OnGUI(position, property, label);
			position.y += UnityEditor.EditorGUIUtility.standardVerticalSpacing + position.height;

			var wwiseObjectReferenceProperty = property.FindPropertyRelative("WwiseObjectReference");
			
			var wwiseProjectFullPath = AkWwiseEditorSettings.WwiseProjectAbsolutePath;
			if (wwiseObjectReferenceProperty.objectReferenceValue)
			{
				var assetObject = new UnityEditor.SerializedObject(wwiseObjectReferenceProperty.objectReferenceValue);
				var userBankProperty = assetObject.FindProperty("IsInUserDefinedSoundBank");
				if (userBankProperty != null)
				{
					bool shouldGreyOut = !AkUtilities.IsAutoBankEnabled();
					EditorGUI.BeginDisabledGroup(shouldGreyOut);
					UnityEditor.EditorGUI.PropertyField(position, userBankProperty, new GUIContent("Is In User-Defined SoundBank:"));
					EditorGUI.EndDisabledGroup();
					assetObject.ApplyModifiedProperties();
				}
			}
		}

		public override float GetPropertyHeight(UnityEditor.SerializedProperty property, UnityEngine.GUIContent label)
		{
			float totalHeight = UnityEditor.EditorGUI.GetPropertyHeight(property, label, true) + UnityEditor.EditorGUIUtility.standardVerticalSpacing;
			var wwiseObjectReferenceProperty = property.FindPropertyRelative("WwiseObjectReference");
			if (wwiseObjectReferenceProperty.objectReferenceValue)
			{
				var assetObject = new UnityEditor.SerializedObject(wwiseObjectReferenceProperty.objectReferenceValue);
				var userBankProperty = assetObject.FindProperty("IsInUserDefinedSoundBank");
				if (userBankProperty != null)
				{
					totalHeight += UnityEditor.EditorGUI.GetPropertyHeight(userBankProperty) +
					               UnityEditor.EditorGUIUtility.standardVerticalSpacing;
				}
			}

			return totalHeight;
		}
	}
	

}
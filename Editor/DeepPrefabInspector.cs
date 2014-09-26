using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

// Created by @YoriKv

// Has to be Transform instead of GameObject, otherwise the Select/Revert/Apply set of buttons disappears even with DrawDefaultInspector
[CustomEditor(typeof(Transform))]
public class DeepPrefabInspector:TransformInspector {
	static GUIStyle selectedBoxStyle = null;
	static GUIStyle defaultFoldoutStyle = null;
	static GUIStyle selectedFoldoutStyle = null;
	static GUIStyle selectedLabelStyle = null;

	// Prefab root - using statics so the same GUI state is kept across children of the same root prefab
	static Transform rootTransform = null;
	static Transform targetTransform = null;
	static bool needsRebuild = true;

	// Prefab tree
	static List<Transform> prefabTree = new List<Transform>();
	static List<bool> prefabTreeState = new List<bool>();

	// Keeps track of prefab index as the GUI is built
	int prefabIndex;

	public override void OnInspectorGUI() {
		if(PrefabUtility.GetPrefabType(target) == PrefabType.Prefab) {
			// Initialize GUI styles, if needed
			if(selectedBoxStyle == null) {
				// Start with default
				selectedBoxStyle = new GUIStyle("Box");
				// Remove box padding
				selectedBoxStyle.padding = new RectOffset(0, 0, 0, 0);
				selectedBoxStyle.margin = new RectOffset(0, 0, 0, 0);
				selectedBoxStyle.border = new RectOffset(0, 0, 0, 0);
				selectedBoxStyle.normal.background = new Texture2D(1, 1, TextureFormat.ARGB32, false);
				selectedBoxStyle.normal.background.SetPixel(0, 0, new Color(72f / 255f, 72f / 255f, 72f / 255f));
				selectedBoxStyle.normal.background.Apply();
				// Default styles - remove blue on select text
				defaultFoldoutStyle = new GUIStyle(EditorStyles.foldout);
				defaultFoldoutStyle.focused.textColor = defaultFoldoutStyle.normal.textColor;
				defaultFoldoutStyle.onFocused.textColor = defaultFoldoutStyle.normal.textColor;
				defaultFoldoutStyle.active.textColor = defaultFoldoutStyle.normal.textColor;
				defaultFoldoutStyle.onActive.textColor = defaultFoldoutStyle.normal.textColor;
				// Selected style
				selectedFoldoutStyle = new GUIStyle(EditorStyles.foldout);
				selectedFoldoutStyle.normal.textColor = Color.white;
				selectedFoldoutStyle.onNormal.textColor = Color.white;
				selectedFoldoutStyle.focused.textColor = Color.white;
				selectedFoldoutStyle.onFocused.textColor = Color.white;
				selectedFoldoutStyle.active.textColor = Color.white;
				selectedFoldoutStyle.onActive.textColor = Color.white;

				selectedLabelStyle = new GUIStyle(EditorStyles.label);
				selectedLabelStyle.normal.textColor = Color.white;
			}

			// Label
			GUILayout.Label("Prefab Navigation");

			// Box and indent
			EditorGUILayout.BeginVertical();
			EditorGUI.indentLevel++;

			// Get Prefab Root
			targetTransform = (Transform)target;
			Transform currentRoot = PrefabUtility.FindPrefabRoot(targetTransform.gameObject).transform;
			// Check for needing rebuild
			needsRebuild = rootTransform == null || (rootTransform != currentRoot);
			// Clear
			if(needsRebuild) {
				prefabTree.Clear();
				prefabTreeState.Clear();
				rootTransform = currentRoot;
			}
			prefabIndex = -1;

			// Build/Show Tree
			BuildPrefabTree(rootTransform);

			// Close box
			EditorGUI.indentLevel--;
			EditorGUILayout.EndVertical();
		}

		// Default Transform GUI UI
		base.OnInspectorGUI();
	}

	private void BuildPrefabTree(Transform parent, bool show = true) {
		// Add Parent
		prefabIndex++;
		if(needsRebuild) {
			prefabTree.Add(parent);
			prefabTreeState.Add(parent != rootTransform);
		}

		// Foldout for Parent
		if(show) {
			bool selected = (parent == targetTransform);
			// Highlight currently selected child
			if(selected) {
				EditorGUILayout.BeginHorizontal(selectedBoxStyle);
			} else {
				EditorGUILayout.BeginHorizontal();
			}

			// Foldout or label with name (depending on whether or not we have children)
			if(parent.childCount > 0) {
				prefabTreeState[prefabIndex] = LayoutFoldout(prefabTreeState[prefabIndex], parent.name, true, selected ? selectedFoldoutStyle : defaultFoldoutStyle);
			} else {
				EditorGUILayout.LabelField(parent.name, selected ? selectedLabelStyle : EditorStyles.label);
			}
			// Selection button
			if(GUILayout.Button("Select", GUILayout.ExpandWidth(false))) {
				Selection.activeTransform = parent;
			}

			// End layout
			EditorGUILayout.EndHorizontal();
		}

		// Indent children
		EditorGUI.indentLevel++;

		// Update show status based on parent visibility and if the current tree branch is folded or not
		show = show && prefabTreeState[prefabIndex];

		// Add Children
		Transform child;
		for(int i = 0; i < parent.childCount; i++) {
			child = parent.GetChild(i);
			BuildPrefabTree(child, show);
		}

		// Undo indent
		EditorGUI.indentLevel--;
	}

	// Layout foldout functions with toggleOnLabelClick functionality
	private static bool LayoutFoldout(bool foldout, GUIContent content, bool toggleOnLabelClick, GUIStyle style) {
		Rect position = GUILayoutUtility.GetRect(40f, 40f, 16f, 16f, style);
		return EditorGUI.Foldout(position, foldout, content, toggleOnLabelClick, style);
	}

	private static bool LayoutFoldout(bool foldout, string content, bool toggleOnLabelClick, GUIStyle style) {
		return LayoutFoldout(foldout, new GUIContent(content), toggleOnLabelClick, style);
	}
}

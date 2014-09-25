using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

// Created by @YoriKv

// Has to be Transform instead of GameObject, otherwise the Select/Revert/Apply set of buttons disappears even with DrawDefaultInspector
[CustomEditor(typeof(Transform))]
public class DeepPrefabInspector:TransformInspector {
	// Prefab root - using statics so the same GUI state is kept across children of the same root prefab
	static Transform rootTransform = null;
	static Transform targetTransform = null;
	static bool needsRebuild = true;

	// Prefab tree
	static List<Transform> prefabTree =  new List<Transform>();
	static List<bool> prefabTreeState =  new List<bool>();

	// Keeps track of prefab index as the GUI is built
	int prefabIndex;

	public override void OnInspectorGUI() {
		if(PrefabUtility.GetPrefabType(target) == PrefabType.Prefab) {
			// Box and indent
			EditorGUILayout.BeginVertical("Box");
			EditorGUI.indentLevel++;

			// Get Prefab Root
			targetTransform = (Transform) target;
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
			// Highlight currently selected child
			if(parent == targetTransform) {
				EditorGUILayout.BeginHorizontal("Box");
			} else {
				EditorGUILayout.BeginHorizontal();
			}

			// Foldout or label with name (depending on whether or not we have children)
			if(parent.childCount > 0) {
				prefabTreeState[prefabIndex] = EditorGUILayout.Foldout(prefabTreeState[prefabIndex], parent.name);
			} else {
				EditorGUILayout.LabelField(parent.name);
			}
			// Selection button
			if(GUILayout.Button("Select", GUILayout.ExpandWidth(false))) {
				Selection.activeTransform = parent;
			}

			// End layout
			EditorGUILayout.EndHorizontal();
		}
		
		// Indent Children
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
}

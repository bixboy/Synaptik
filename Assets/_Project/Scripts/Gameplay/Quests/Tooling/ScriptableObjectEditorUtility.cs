#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Synaptik.Gameplay.Quests
{
    internal static class ScriptableObjectEditorUtility
    {
        public static bool IsSubAssetOf(this ScriptableObject asset, ScriptableObject parent)
        {
            if (asset == null || parent == null)
            {
                return false;
            }

            var assetPath = AssetDatabase.GetAssetPath(asset);
            var parentPath = AssetDatabase.GetAssetPath(parent);

            if (string.IsNullOrEmpty(assetPath) || string.IsNullOrEmpty(parentPath))
            {
                return false;
            }

            return assetPath == parentPath;
        }
    }
}
#endif

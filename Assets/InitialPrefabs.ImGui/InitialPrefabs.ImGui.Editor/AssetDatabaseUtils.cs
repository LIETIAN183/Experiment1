using UnityEditor;

#if URP_ENABLED 
using UnityEngine.Rendering.Universal;
#endif

namespace InitialPrefabs.NimGui.Editor {
    internal static class AssetDatabaseUtils {
        public static string[] Query(string filter) {
            return AssetDatabase.FindAssets(filter);
        }

        public static string First(this string[] filters) {
            return filters[0];
        }

        public static T As<T>(this string guid) where T : UnityEngine.Object {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            return (T)AssetDatabase.LoadAssetAtPath(path, typeof(T));
        }
    }
}

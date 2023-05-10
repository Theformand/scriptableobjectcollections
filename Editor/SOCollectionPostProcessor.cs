using ScriptableObjectCollections.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SOCollectionDeleteProcessor : AssetModificationProcessor
{
    public static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions options)
    {
        var result = AssetDeleteResult.DidNotDelete;
        var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
        if (so)
        {
            var collection = SOCollectionPostProcessor.GetCollection(so);
            if (collection == null)
                return result;

            var boolField = collection.GetType().GetField("AutoRemove");
            bool autoRemove = (bool)boolField.GetValue(collection);
            if (!autoRemove)
                return result;

            var listField = collection.GetType().GetField("DataObjects").GetValue(collection);
            var list = (IList)listField;
            if (list.Contains(so))
                listField.GetType().GetMethod("Remove").Invoke(listField, new[] { so });

            EditorUtility.SetDirty(collection);
        }
        return AssetDeleteResult.DidNotDelete;
    }
}

public class SOCollectionPostProcessor : AssetPostprocessor
{
    public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (var path in importedAssets)
        {
            var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (so)
            {
                var collection = GetCollection(so);
                if (collection == null)
                    return;

                var boolField = collection.GetType().GetField("AutoAddFromFolder");
                bool autoAdd = (bool)boolField.GetValue(collection);
                if (!autoAdd)
                    return;

                var listField = collection.GetType().GetField("DataObjects").GetValue(collection);
                var list = (IList)listField;
                if (!list.Contains(so))
                    listField.GetType().GetMethod("Add").Invoke(listField, new[] { so });

                EditorUtility.SetDirty(so);
                EditorUtility.SetDirty(collection);
            }
        }
    }

    public static Object GetCollection(ScriptableObject so)
    {
        var path = AssetDatabase.GetAssetPath(so);
        var folderPath = path.Replace("/" + so.name + ".asset", "");
        var collectionType = typeof(SOCollection<>).MakeGenericType(so.GetType());
        var collections = AssetDatabase.FindAssets($"t: {collectionType.Name}", new string[] { folderPath });
        if (collections.Length < 1)
            return null;

        return AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(collections[0]), collectionType);
    }
}

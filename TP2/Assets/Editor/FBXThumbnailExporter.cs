using UnityEngine;
using UnityEditor;
using System.IO;

public class FBXThumbnailExporter
{
    [MenuItem("Tools/Export FBX Thumbnail")]
    public static void ExportThumbnail()
    {
        // Get the selected asset
        Object obj = Selection.activeObject;
        if (obj == null)
        {
            Debug.LogWarning("Select an FBX asset first.");
            return;
        }

        // Ask Unity for its preview image
        Texture2D preview = AssetPreview.GetAssetPreview(obj);
        if (preview == null)
        {
            Debug.LogWarning("No preview available yet. Click on the FBX first, wait a second, then try again.");
            return;
        }

        // Convert preview to PNG
        byte[] pngData = preview.EncodeToPNG();

        // Save inside the project (same folder as the FBX)
        string assetPath = AssetDatabase.GetAssetPath(obj);
        string directory = Path.GetDirectoryName(assetPath);
        string outputPath = Path.Combine(directory, $"{obj.name}_thumbnail.png");

        File.WriteAllBytes(outputPath, pngData);
        Debug.Log($"✅ Thumbnail exported: {outputPath}");

        AssetDatabase.Refresh();
    }
}
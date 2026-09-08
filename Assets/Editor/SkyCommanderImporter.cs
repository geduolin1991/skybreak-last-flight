using UnityEditor;
using UnityEngine;
public class SkyCommanderImporter : AssetPostprocessor {
 void OnPreprocessTexture(){if(!assetPath.StartsWith("Assets/Resources/BossPortraits/"))return;var t=(TextureImporter)assetImporter;t.mipmapEnabled=false;t.maxTextureSize=2048;t.textureCompression=TextureImporterCompression.CompressedHQ;t.wrapMode=TextureWrapMode.Clamp;t.sRGBTexture=true;t.npotScale=TextureImporterNPOTScale.None;t.isReadable=false;}
}

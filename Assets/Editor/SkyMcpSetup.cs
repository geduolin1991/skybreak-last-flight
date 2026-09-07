using UnityEditor;
using UnityEngine;
using MCPForUnity.Editor.Services.Transport.Transports;

// Development tooling only. The scene and the playable build are unaffected.
public static class SkyMcpSetup {
 [MenuItem("Skybreak/Development/Start local MCP connection")]
 public static void Start() {
  EditorPrefs.SetBool("MCPForUnity.TelemetryDisabled",true);
  EditorPrefs.SetBool("MCPForUnity.UseHttpTransport",false);
  EditorPrefs.SetBool("MCPForUnity.SetupCompleted",true);
  EditorPrefs.SetBool("MCPForUnity.SetupDismissed",true);
  EditorPrefs.SetString("MCPForUnity.UvxPath","/opt/homebrew/bin/uvx");
  MCPForUnity.Editor.Services.EditorConfigurationCache.Instance.Refresh();
  StdioBridgeHost.Start();
  Debug.Log("SKYBREAK_MCP_READY port="+StdioBridgeHost.GetCurrentPort());
 }
 [MenuItem("Skybreak/Development/Stop local MCP connection")]
 public static void Stop(){StdioBridgeHost.Stop();}
}

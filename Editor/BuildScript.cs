using UnityEditor;
using System.IO;

public class BuildScript
{
    public static void BuildWindows()
    {
        string path = "Build/Windows";
        Directory.CreateDirectory(path);

        BuildPipeline.BuildPlayer(
            EditorBuildSettings.scenes,
            path + "/Escape_AI.exe",
            BuildTarget.StandaloneWindows64,
            BuildOptions.None
        );
    }
}

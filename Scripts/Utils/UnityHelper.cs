using UnityEditor;

namespace UnityBuildTooling.Editor.build_tooling.Scripts.Utils
{
    internal static class UnityHelper
    {
        public static bool IsBuildTargetSupported(BuildTarget buildTarget)
        {
            var moduleManager = System.Type.GetType("UnityEditor.Modules.ModuleManager,UnityEditor.dll");
            var isPlatformSupportLoaded = moduleManager.GetMethod("IsPlatformSupportLoaded", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var getTargetStringFromBuildTarget = moduleManager.GetMethod("GetTargetStringFromBuildTarget", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
     
            return (bool)isPlatformSupportLoaded.Invoke(null,new object[] {(string)getTargetStringFromBuildTarget.Invoke(null, new object[] {buildTarget})});
        }
    }
}
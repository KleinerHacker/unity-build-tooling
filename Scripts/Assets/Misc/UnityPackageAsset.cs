using UnityEngine;

namespace UnityBuildTooling.Editor.build_tooling.Scripts.Assets.Misc
{
    [CreateAssetMenu(menuName = UnityBuildToolingConstants.Menu.Asset.MiscMenu + "/Unity Package")]
    public sealed class UnityPackageAsset : ScriptableObject
    {
        #region Inspector Data

        [SerializeField]
        private string packageName;

        [SerializeField]
        private string[] assetPaths;

        #endregion

        #region Propertes

        public string PackageName => packageName;

        public string[] AssetPaths => assetPaths;

        #endregion
    }
}
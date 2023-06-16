using UnityEngine;
using Forge.Helpers.Interfaces;
using Forge.Models.Upgradables;

namespace Forge.Configs
{
    [CreateAssetMenu(fileName = "UpgradableContainerConfig", menuName = "Config/Upgradable/Upgradable Container")]
    public class UpgradableContainerConfig : ScriptableObject
    {
        public UpgradableConfig[] UpgradableConfigs;

        public IFactory<IUpgradable> UpgradableFactory;

        public IParent Parent;
    }
}
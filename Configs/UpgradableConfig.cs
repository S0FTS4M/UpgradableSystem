using Forge.Helpers.Interfaces;
using UnityEngine;

namespace Forge.Configs
{
    [CreateAssetMenu(fileName = "UpgradableConfig", menuName = "Config/Generator/Upgradable/Upgradable Base")]
    public class UpgradableConfig : ScriptableObject
    {
        public float BaseCost;
        public float CostMultiplier;
        public int MaxLevel;
        public StaticUpgradableConfig staticConfig;
        public IParent Parent;
    }
}
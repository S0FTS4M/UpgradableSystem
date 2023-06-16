using Forge.Models.Upgradables;
using UnityEngine;

namespace Forge.Configs
{
    [CreateAssetMenu(fileName = "StaticUpgradableConfig", menuName = "Config/StaticUpgradable")]
    public class StaticUpgradableConfig : ScriptableObject
    {
        public UpgradableType UpgradableType;
        public string UpgradableName;
        public int Level;
    }
}
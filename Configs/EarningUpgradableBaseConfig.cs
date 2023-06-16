using UnityEngine;

namespace Forge.Configs
{
    [CreateAssetMenu(fileName = "EarningUpgradableBaseConfig", menuName = "Config/Upgradable/EarningUpgradableBase")]
    public class EarningUpgradableBaseConfig : TimedUpgradableConfig
    {
        public float BaseEarningPerSec;
    }
}
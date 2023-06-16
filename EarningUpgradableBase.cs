using Newtonsoft.Json;
using Forge.Configs;
using Forge.Models.CurrencyManagement;
using UnityEngine;
using Forge.Models.Operators;

namespace Forge.Models.Upgradables
{
    public abstract class EarningUpgradableBase : TimedUpgradable, IMainCurrencyIncreaser
    {
        #region Variables

        private float _baseEarningPerSec;

        [JsonProperty]
        private Multiplier<float, FloatOperator> _earningMultipliers;

        private EarningUpgradableBaseConfig _earningUpgradableConfig;

        #endregion

        #region Props

        public virtual float ProductionEarning => _baseEarningPerSec * Level * Time * _earningMultipliers.AllMults;

        public double IncreaseAmount => ProductionEarning;

        public float BaseEarningPerSec { get => _baseEarningPerSec; }

        public Multiplier<float, FloatOperator> EarningMultipliers => _earningMultipliers;

        #endregion

        #region Methods

        public override void Initialize(ScriptableObject so, bool isLoaded)
        {
            base.Initialize(so, isLoaded);

            _earningUpgradableConfig = (EarningUpgradableBaseConfig)so;
            _baseEarningPerSec = _earningUpgradableConfig.BaseEarningPerSec;

            if(isLoaded == false)
            {
                _earningMultipliers = new Multiplier<float, FloatOperator>();
            }
        }

        public override void OnManage()
        {
            base.OnManage();
            ShouldUpdate = true;

            Container.Currency.Increase(this);
        }

        protected override void OnUpgrade()
        {
            Container.Statistics.Ips += _baseEarningPerSec;
        }

        public override  void OnPrestige()
        {
            base.OnPrestige();

            _earningMultipliers.Reset();
        }

        #endregion
    }
}
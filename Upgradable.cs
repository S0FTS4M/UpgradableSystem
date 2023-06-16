using System;
using Newtonsoft.Json;
using Forge.Configs;
using Forge.Models.CurrencyManagement;
using Forge.Models.Constraint;
using Forge.Helpers;
using Forge.Models.ReferenceContainers;
using Forge.Helpers.Interfaces;
using UnityEngine;
using Forge.Controllers;
using System.Collections.Generic;
using Forge.Services.Localization;

namespace Forge.Models.Upgradables
{

    public delegate void UpgradeHandler(IUpgradable upgradable);

    public interface IUpgradable:
        IMainCurrencyDecreaser,
        IIdentifiable,
        IConstraintable,
        IParent,
        IContainerHolder,
        IInitializable,
        IInitialized
    {
        int Level { get; }
        float BaseCost { get; }
        double UpgradeCost { get; }
        int MaxLevel { get; }
        UpgradableConfig Config { get; }
        IConstraintBase UpgradeConstraint { get; }
        IConstraintBase LevelMaxedConstraint { get; }

        event UpgradeHandler Upgraded;

        void Upgrade();
        void UpgradeRewarded(bool sendEvent = true);

        string GetDisplayInfo();

        float GetProgressAmount();
    }

    [JsonObject(MemberSerialization.OptIn)]
    public abstract class Upgradable : IUpgradable
    {
        #region Variables

        /// <summary>
        /// Current level of the upgrader.
        /// </summary>
        [JsonProperty]
        private int _level;

        /// <summary>
        /// Cost to upgrade this upgrader to next level
        /// </summary>
        private double _upgradeCost;

        private float _baseCost;

        private float _costMultiplier;

        /// <summary>
        /// Maximum level that upgrader can reach
        /// </summary>
        protected int _maxLevel;

        private string _guid;

        protected IGameBase _gameBase;

        private IConstraintBase _upgradeConstraint;

        private IConstraintBase _levelMaxedConstraint;

        #endregion

        #region Props

        public int Level => _level;

        public float BaseCost => _baseCost;

        public double UpgradeCost => Verfices.Request<GameContainer>().CurrStageIndex == 0 ? _upgradeCost : Verfices.Request<GameContainer>().CurrStageIndex * Verfices.Request<GameContainer>().CostMultiplier * _upgradeCost;

        public int MaxLevel { get; set; }

        public UpgradableConfig Config { get; private set; }

        public IConstraintBase UpgradeConstraint => _upgradeConstraint;

        public IConstraintBase LevelMaxedConstraint => _levelMaxedConstraint;

        public double DecreaseAmount => UpgradeCost;

        public IParent Parent { get; private set; }

        public ReferenceContainerBase Container { get; private set; }

        #endregion

        #region Events

        public event UpgradeHandler Upgraded;

        #endregion

        #region Methods

        public virtual void Initialize(ScriptableObject so, bool isLoaded)
        {
            Config = (UpgradableConfig) so;

            if (isLoaded == false)
            {
                _level = Config.staticConfig.Level;
            }

            _gameBase       = Verfices.Request<IGameBase>();

            Parent         = Config.Parent;
            Container      = ((IContainerHolder) Parent).Container;

            MaxLevel       = Config.MaxLevel;
            _baseCost       = Config.BaseCost;
            _costMultiplier = Config.CostMultiplier;
            
            _upgradeCost    = CalcMultipliedUpgradeCost();
            _guid           = Guid.NewGuid().ToString();
        }

        public virtual void Initialized()
        {
            _levelMaxedConstraint = new UpgradableLevelMaxedConstraint(this);
            
            if(Container == null)
            {
                Debug.LogError("how did this happen?");
            }
            _upgradeConstraint = new MainCurrencyConstraint(this, Container.Currency);

            //if level is not maxed
            if (_levelMaxedConstraint.Satisfied == false)
            {
                Container.Currency.CurrencyChanged += OnMainCurrencyChanged;
            }
        }

        public virtual void Upgrade()
        {
            if (_levelMaxedConstraint.Satisfied || _upgradeConstraint.Satisfied == false)
                return;

            SendEvent(false);

            Container.Currency.Decrease(this);

            _level += 1;
            _upgradeCost = CalcMultipliedUpgradeCost();

            // NOTE: We check here cause we need to update upgradable state
            // immediately. levelMaxed and upgradable states should be ready
            // if player keep spaming the upgrade button 
            _upgradeConstraint.Check();
            _levelMaxedConstraint.Check();

            //Notify sub classes
            OnUpgrade();

            Upgraded?.Invoke(this);
        }

        public virtual void UpgradeRewarded(bool sendEvent = true)
        {
            if(sendEvent)
                SendEvent(true);

            _level += 1;
            _upgradeCost = CalcMultipliedUpgradeCost();

            // NOTE: We check here cause we need to update upgradable state
            // immediately. levelMaxed and upgradable states should be ready
            // if player keep spaming the upgrade button 
            _upgradeConstraint.Check();
            _levelMaxedConstraint.Check();

            //Notify sub classes
            OnUpgrade();

            Upgraded?.Invoke(this);
        }

        public string GetIdentifier()
        {
            return _guid;
        }

        protected abstract void OnUpgrade();

        #endregion

        #region Multiple Upgrade Methods

        private double CalcMultipliedUpgradeCost(int multiplier = 1)
        {
            return System.Math.Round(_baseCost * (Math.Pow(_costMultiplier, _level) *
                (Math.Pow(_costMultiplier, multiplier) - 1.0)) / (_costMultiplier - 1.0));
        }

        private int CalcMaxUpgradeMultiplier()
        {
            return (int)Math.Floor(Math.Log(
                (Container.Currency.Amount * (_costMultiplier - 1.0)) /
                (_baseCost * Math.Pow(_costMultiplier, _level)) + 1.0, _costMultiplier));
        }

        private void SendEvent(bool isRewarded)
        {
            var currParent = Parent;
            while (currParent != null && (currParent is IEntity) == false)
            {
                currParent = currParent.Parent;
            }

            var entityName = "";

            if (currParent != null)
            {
                var entity = (IEntity)currParent;
                entityName = entity.Name + "_";
            }

            AnalyticsController.SendEventWithParam(
                AnalyticsController.GD_Purchase,
                new KeyValuePair<string, object>(AnalyticsController.Cost, _upgradeCost),
                new KeyValuePair<string, object>(AnalyticsController.PurchasedItemType, entityName + Config.staticConfig.UpgradableName),
                new KeyValuePair<string, object>(AnalyticsController.PurchaseNo, Level),
                new KeyValuePair<string, object>(AnalyticsController.Rewarded, isRewarded)
            );
        }

        #endregion

        #region Callbacks

        protected void OnMainCurrencyChanged(object sender, EventArgs cArgs)
        {
            _upgradeConstraint.Check();
        }

        public virtual void OnPrestige()
        {
            //reset variables
            _level       = Config.staticConfig.Level;
            _baseCost    = Config.BaseCost;
            _upgradeCost = CalcMultipliedUpgradeCost();
            
            _levelMaxedConstraint.Check();
            _upgradeConstraint.Check();

            //RESET EVENTS
            Container.Currency.CurrencyChanged -= OnMainCurrencyChanged;
            Container.Currency.CurrencyChanged += OnMainCurrencyChanged;
        }

        public abstract string GetDisplayInfo();

        public virtual float GetProgressAmount()
        {
            return (float)Level / (float)MaxLevel;
        }

        #endregion
    }
}
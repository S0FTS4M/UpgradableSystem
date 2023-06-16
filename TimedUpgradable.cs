using System;
using Forge.Configs;
using Forge.Helpers.Updatable;
using Forge.Models.Operators;
using Newtonsoft.Json;
using UnityEngine;

namespace Forge.Models.Upgradables
{
    public interface ITimedUpgradable : IUpdatable, IManagable
    {
        float Time { get; }
        float CurrTime { get; }
        bool IsTimeUp { get; }

        event Action TimeUp;

        float GetTimeAsPercent();
    }

    public abstract class TimedUpgradable : Upgradable, ITimedUpgradable
    {
        #region Variables

        [JsonProperty]
        protected float time;

        [JsonProperty]
        private Multiplier<float, FloatOperator> timeMultipliers;

        [JsonProperty]
        private IManager manager;

        protected float currTime;

        protected float currStartTime;

        private bool isTimeUp;

        private TimedUpgradableConfig timedUpgradableConfig;

        #endregion

        #region Props

        public virtual float Time
        {
            get => time / timeMultipliers.AllMult;
            protected set => time = value;
        }

        public Multiplier<float, FloatOperator> TimeMultipliers => timeMultipliers;

        public float CurrTime => currTime;

        public bool ShouldUpdate { get; set; }

        public IManager Manager => manager;

        public bool IsTimeUp => isTimeUp;

        #endregion

        #region  Events

        public event Action TimeUp;

        #endregion

        #region Methods

        public override void Initialize(ScriptableObject so, bool isLoaded)
        {
            base.Initialize(so, isLoaded);

            this.timedUpgradableConfig = (TimedUpgradableConfig)so;

            // If upgradable is created
            if (isLoaded == false)
            {
                manager         = new Manager();
                time            = timedUpgradableConfig.Time;
                timeMultipliers = new Multiplier<float, FloatOperator>();
            }

            currTime = Time;
            currStartTime = Time;

            timedUpgradableConfig.ManagerConfig.Managable = this;

            gameBase.UpdatableManager.Add(this);
            manager.Initialize(timedUpgradableConfig.ManagerConfig, isLoaded);
        }

        public void Update(float deltaTime)
        {
            currTime -= deltaTime;
            if (currTime <= 0)
            {
                ShouldUpdate = manager.IsActive;

                // NOTE: This has to be fired after shouldupdate changed
                NotifyTimeUp();
                Manage();
            }
        }

        public void AssignManager(IManager manager)
        {
            this.manager = manager;
        }

        protected virtual void NotifyTimeUp()
        {
            isTimeUp = true;
            TimeUp?.Invoke();
        }

        protected virtual void Manage()
        {
            if (manager.IsActive)
            {
                manager.Manage();
            }
        }

        public virtual void OnManage()
        {
            currTime = Time;
            currStartTime = Time;
            isTimeUp = false;
        }

        public float GetTimeAsPercent()
        {
            return (currStartTime - CurrTime) / currStartTime;
        }

        public override void OnPrestige()
        {
            base.OnPrestige();

            timeMultipliers.Reset();

            time          = timedUpgradableConfig.Time;
            currTime      = Time;
            currStartTime = Time;
            isTimeUp      = false;
            ShouldUpdate  = false;
        }

        #endregion

        #region Callbacks

        public void OnHire()
        {
            ShouldUpdate = true;
            if (isTimeUp)
            {
                //this means we were waiting for user to manage but he/she did
                //not do it so it causes some problems
                manager.Manage();
            }
        }

        protected abstract override void OnUpgrade();

        #endregion
    }
}
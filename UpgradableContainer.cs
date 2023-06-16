using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using Forge.Configs;
using Forge.Helpers.Interfaces;

namespace Forge.Models.Upgradables
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UpgradableContainer : IUpgradableContainer
    {
        [JsonProperty]
        private Dictionary<string, IUpgradable> _upgradableDict;
        private IFactory<IUpgradable> _upgradableFactory;

        #region Props
        
        public List<IUpgradable> Upgradables => _upgradableDict.Values.ToList();
        
        #endregion

        /// <summary>
        /// Initilaizes the container.
        /// </summary>
        /// <param name="so">Gets #UpgradableContainerConfig as parameter</param>
        /// <param name="isLoaded"></param>
        public void Initialize(ScriptableObject so, bool isLoaded)
        {
            var config = (UpgradableContainerConfig)so;

            if(config.UpgradableFactory == null || config.Parent == null)
            {
                Debug.LogError("UpgradableContainer: UpgradableFactory or Parent is null");
                return;
            }

            _upgradableFactory = config.UpgradableFactory;

            if (isLoaded == false)
            {
                _upgradableDict = new Dictionary<string, IUpgradable>();
            }

            var upgradableConfigs = config.UpgradableConfigs;
            for (int i = 0; i < upgradableConfigs.Length; i++)
            {
                var upgradableType = upgradableConfigs[i].staticConfig.UpgradableType;
                bool isLoadSuccess = _upgradableDict.TryGetValue(upgradableType.ToString(), out IUpgradable upgradable);

                if (isLoadSuccess == false)
                {
                    upgradable = AddUpgradable(upgradableType);
                }

                upgradableConfigs[i].Parent = config.Parent;
                upgradable.Initialize(upgradableConfigs[i], isLoadSuccess);
            }

        }
        
        public void Initialized()
        {
            var upgradables = _upgradableDict.Values.ToArray();
            for (int i = 0; i < upgradables.Length; ++i)
            {
                upgradables[i].Initialized();
            }
        }
        
        public IUpgradable AddUpgradable(UpgradableType upgradableType)
        {
            var upgradable = _upgradableFactory.Create(upgradableType.ToString());
            _upgradableDict.Add(upgradableType.ToString(), upgradable);

            return upgradable;
        }

        public IUpgradable GetUpgradable(UpgradableType upgradableType)
        {
            bool success = _upgradableDict.TryGetValue(upgradableType.ToString(), out IUpgradable upgradable);

            if (success == false)
            {
                UnityEngine.Debug.LogError("There is no upgrader with type of " +
                    upgradableType);
            }

            return upgradable;
        }

        public virtual void Upgrade(UpgradableType upgradableType)
        {
            GetUpgradable(upgradableType).Upgrade();
        }

        public bool HasUpgradable(UpgradableType type)
        {
            return _upgradableDict.ContainsKey(type.ToString());
        }
    }
}
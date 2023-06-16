using Forge.Helpers.Interfaces;

namespace Forge.Models.Upgradables
{
    public interface IUpgradableContainer : IInitializable, IInitialized
    {
        IUpgradable AddUpgradable(UpgradableType type);
        IUpgradable GetUpgradable(UpgradableType type);

        bool HasUpgradable(UpgradableType type);
        void Upgrade(UpgradableType type);
    }
}
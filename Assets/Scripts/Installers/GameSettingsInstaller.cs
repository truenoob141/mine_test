using UnityEngine;
using Zenject;

namespace Mine
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Installers/GameSettingsInstaller")]
    public class GameSettingsInstaller : ScriptableObjectInstaller<GameSettingsInstaller>
    {
        public ProjectInstaller.Settings settings;

        public override void InstallBindings()
        {
            Container.BindInstance(this.settings);
        }
    }
}

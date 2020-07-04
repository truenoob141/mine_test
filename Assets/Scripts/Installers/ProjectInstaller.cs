using Mine.Game;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Mine
{
    public class ProjectInstaller : MonoInstaller
    {
        [System.Serializable]
        public class Settings
        {
            public GameObject statPrefab;
            [Space(10)]
            public int healthId;
            public int armorId;
            public int damageId;
            public int lifestealId;
        }

        [Inject] private Settings settings;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<EventManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<LoadManager>().AsSingle();

            Container.BindInterfacesAndSelfTo<PoolCleanupChecker>().AsSingle();
        }
    }
}

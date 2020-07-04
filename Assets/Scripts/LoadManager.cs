using UnityEngine;
using UnityEngine.Assertions;
using Zenject;

namespace Mine
{
    public class OnGameLoaded { }

    public class LoadManager : IInitializable
    {
        public Data GameData { get; private set; }

        [Inject] private EventManager eventManager;

        public void Initialize()
        {
            var request = Resources.LoadAsync<TextAsset>("data");
            request.completed += (op) =>
            {
                var config = (TextAsset)request.asset;
                Assert.IsNotNull(config, "Config not found");

                this.GameData = JsonUtility.FromJson<Data>(config.text);

                eventManager.Trigger<OnGameLoaded>();
            };
        }
    }
}

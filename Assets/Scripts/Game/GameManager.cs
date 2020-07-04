using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;

namespace Mine.Game
{
    public class OnGameStarted { }

    public class OnPlayerStatsChanged
    {
        public int PlayerId { get; private set; }

        public OnPlayerStatsChanged(int playerId)
        {
            this.PlayerId = playerId;
        }
    }

    public class GameManager : IInitializable, IDisposable
    {
        [Inject] private EventManager eventManager;
        [Inject] private LoadManager loadManager;
        [Inject] private ProjectInstaller.Settings settings;

        public bool IsValidGame { get; private set; }

        private List<Player> players;

        public void Initialize()
        {
            eventManager.Subscribe<OnGameLoaded>(OnGameLoaded);
        }

        public void Dispose()
        {
            eventManager.Unsubscribe<OnGameLoaded>(OnGameLoaded);
        }

        public void StartGame(bool withBuffs)
        {
            Debug.Log("Start game");
            Assert.IsFalse(IsValidGame, "Already game");

            var data = loadManager.GameData;

            // Create players
            this.players = new List<Player>(Constants.PLAYER_COUNT);
            for (int i = 0; i < Constants.PLAYER_COUNT; ++i)
            {
                var player = new Player(i + 1, settings);

                // Apply stats
                foreach (var stat in data.stats)
                {
                    player.ApplyStat(stat);
                }

                // Apply buffs
                if (withBuffs)
                {
                    var buffs = GetBuffs();
                    foreach (var buff in buffs)
                    {
                        player.ApplyBuff(buff);
                    }
                }

                this.players.Add(player);
            }

            // Done
            this.IsValidGame = true;

            eventManager.Trigger<OnGameStarted>();

            Debug.Log("Game started");
        }

        public void EndGame()
        {
            Debug.Log("End game");
            Assert.IsTrue(IsValidGame, "No game");

            this.IsValidGame = false;
        }

        public void DealDamage(int attackerId)
        {
            Assert.IsTrue(attackerId > 0, "Invalid player id");

            var victim = GetEnemy(attackerId);
            // Victim already dead
            if (!victim.IsAlive())
                return;

            var attacker = GetPlayer(attackerId);

            // Deal damage
            float damage = attacker.GetDamage();
            if (damage <= 0)
                return;

            damage = victim.TakeDamage(damage);

            // Lifesteal
            float lifesteal = attacker.GetLifesteal();
            if (lifesteal > 0)
                attacker.Heal(lifesteal * damage);

            // Events
            eventManager.Trigger(new OnPlayerStatsChanged(victim.PlayerId));
            eventManager.Trigger(new OnPlayerStatsChanged(attacker.PlayerId));

            // Game over
            bool isAlive = victim.IsAlive();
            if (!isAlive)
            {
                EndGame();
            }
        }

        public Player GetPlayer(int playerId)
        {
            // TODO I can use dictionary, but no
            return this.players.FirstOrDefault(p => p.PlayerId == playerId);
        }

        private void OnGameLoaded()
        {
            // Auto start
            StartGame(false);
        }

        private Player GetEnemy(int playerId)
        {
            return this.players.FirstOrDefault(p => p.PlayerId != playerId);
        }

        private IEnumerable<Buff> GetBuffs()
        {
            var data = loadManager.GameData;
            var settings = data.settings;
            Assert.IsTrue(settings.buffCountMin <= settings.buffCountMax, "Incorrect buff count");

            int count = UnityEngine.Random.Range(settings.buffCountMin, settings.buffCountMax + 1);

            IEnumerable<Buff> buffs;
            if (settings.allowDuplicateBuffs)
                buffs = Enumerable.Repeat(0, count).Select(_ => data.buffs.RandomValue());
            else
                buffs = data.buffs.OrderBy(_ => UnityEngine.Random.value).Take(count);

            return buffs;
        }
    }
}

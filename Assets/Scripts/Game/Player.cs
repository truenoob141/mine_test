using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Mine.Game
{
    public class Player
    {
        public int PlayerId { get; private set; }

        public event Action onStatsChanged;

        private ProjectInstaller.Settings settings;

        private Dictionary<int, Stat> stats = new Dictionary<int, Stat>();
        private Dictionary<int, Buff> buffs = new Dictionary<int, Buff>();

        public Player(int playerId, ProjectInstaller.Settings settings)
        {
            this.PlayerId = playerId;
            this.settings = settings;
        }

        /// <summary>
        /// Set player stat
        /// </summary>
        public void ApplyStat(Stat stat)
        {
            Assert.IsNotNull(stat);

            Stat playerStat;
            if (!stats.TryGetValue(stat.id, out playerStat))
            {
                playerStat = new Stat
                {
                    id = stat.id,
                    icon = stat.icon,
                    title = stat.title
                };

                stats.Add(stat.id, playerStat);
            }

            playerStat.value = stat.value;
        }

        /// <summary>
        /// Add or set buff
        /// </summary>
        public void ApplyBuff(Buff buff)
        {
            Assert.IsNotNull(buff);

            // Get/Store
            Buff playerBuff;
            if (!buffs.TryGetValue(buff.id, out playerBuff))
            {
                // TODO Clone
                playerBuff = new Buff
                {
                    id = buff.id,
                    icon = buff.icon,
                    title = buff.title,
                    stats = new BuffStat[buff.stats.Length]
                };

                for (int i = 0; i < playerBuff.stats.Length; ++i)
                {
                    playerBuff.stats[i] = new BuffStat { statId = buff.stats[i].statId };
                }

                buffs.Add(buff.id, playerBuff);
            }
            else
            {
                // Just debug
                Debug.Log($"Player {PlayerId} has a duplicate buff {buff.title} ({buff.id})");
            }

            // Mutable buffs or duplicate buff id
            Assert.AreEqual(playerBuff.stats.Length, buff.stats.Length);

            for (int i = 0; i < buff.stats.Length; ++i)
            {
                var buffStat = buff.stats[i];

                Assert.IsTrue(stats.ContainsKey(buffStat.statId));

                // Apply
                // All buffs is sum (no multiply mode)
                this.stats[buffStat.statId].value += buffStat.value;

                // Remember
                playerBuff.stats[i].value += buffStat.value;
            }
        }

        public Stat GetStat(int statId)
        {
            return this.stats[statId];
        }

        public Stat[] GetStats()
        {
            return this.stats.Values.ToArray();
        }

        public Buff[] GetBuffs()
        {
            return this.buffs.Values.ToArray();
        }

        public float GetHealth()
        {
            return stats[settings.healthId].value;
        }

        public float GetDamage()
        {
            return stats[settings.damageId].value;
        }

        public float GetLifesteal()
        {
            return stats[settings.lifestealId].value * 0.01f;
        }

        /// <returns>Final damage amount</returns>
        public float TakeDamage(float amount)
        {
            Assert.IsTrue(amount > 0);

            // Apply armor
            var armor = stats[settings.armorId].value;
            Assert.IsTrue(armor >= 0);
            float damage = amount * Mathf.Clamp01(1 - armor * 0.01f);

            // Done
            var health = stats[settings.healthId];
            health.value = Mathf.Max(0, health.value - damage);

            Debug.Log($"Player {PlayerId} take {damage} damage (health {health.value})");

            // Event
            onStatsChanged?.Invoke();

            return damage;
        }

        public void Heal(float amount)
        {
            // Ignore max health ?

            Assert.IsTrue(amount > 0);

            var health = stats[settings.healthId];
            health.value += amount;

            Debug.Log($"Heal player {PlayerId} to {amount} (health {health.value})");

            // Event
            onStatsChanged?.Invoke();
        }

        public bool IsAlive()
        {
            var health = GetHealth();
            return health > 0;
        }
    }
}

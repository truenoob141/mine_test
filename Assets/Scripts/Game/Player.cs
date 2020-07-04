using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Mine.Game
{
    public class Player : IVictim, IAttacker
    {
        public int EntityId { get; private set; }

        private ProjectInstaller.Settings settings;

        private Dictionary<int, Stat> stats = new Dictionary<int, Stat>();
        private Dictionary<int, Buff> buffs = new Dictionary<int, Buff>();

        public Player(int playerId, ProjectInstaller.Settings settings)
        {
            this.EntityId = playerId;
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
                Debug.Log($"Player {EntityId} has a duplicate buff {buff.title} ({buff.id})");
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

        #region IVictim
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

            Debug.Log($"Player {EntityId} take {damage} damage (health {health.value})");

            return damage;
        }

        public bool IsAlive()
        {
            var health = GetHealth();
            return health > 0;
        }
        #endregion

        #region IAttacker
        public float GetDamage()
        {
            return stats[settings.damageId].value;
        }

        public void DealDamage(float damage)
        {
            float lifesteal = stats[settings.lifestealId].value * 0.01f;
            if (lifesteal > 0)
                Heal(lifesteal * damage);
        }
        #endregion

        private void Heal(float amount)
        {
            // Ignore max health ?

            Assert.IsTrue(amount > 0);

            var health = stats[settings.healthId];
            health.value += amount;

            Debug.Log($"Heal player {EntityId} to {amount} (health {health.value})");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Zenject;

namespace Mine.Game
{
    [RequireComponent(typeof(PlayerPanelHierarchy))]
    public class PlayerController : MonoBehaviour
    {
        private class StatView
        {
            public GameObject gameObject;
            public Image image;
            public Text label;

            public bool isBuff;
            public int id;
        }

        [SerializeField]
        private int playerId = -1;

        [Inject] private EventManager eventManager;
        [Inject] private GameManager gameManager;
        [Inject] private ProjectInstaller.Settings settings;

        private PlayerPanelHierarchy panel;

        private Player currentPlayer;
        private List<StatView> statViews = new List<StatView>();

        private void Awake()
        {
            this.panel = GetComponent<PlayerPanelHierarchy>();

            eventManager.Subscribe<OnGameStarted>(OnGameStarted);

            this.panel.attackButton.onClick.AddListener(OnAttackClick);
        }

        private void OnDestroy()
        {
            eventManager.Unsubscribe<OnGameStarted>(OnGameStarted);

            this.panel.attackButton.onClick.RemoveListener(OnAttackClick);

            // Why not?
            ResetCurrentPlayer();
        }

        private void OnGameStarted()
        {
            Assert.IsTrue(this.playerId > 0, "Incorrect player id");

            // Clear old stats
            ResetCurrentPlayer();
            ClearStatViews();

            // Game is not valid (it's ok)
            if (!gameManager.IsValidGame)
                return;

            var player = gameManager.GetPlayer(this.playerId);
            if (player == null)
            {
                Debug.LogError($"Player {this.playerId} not found");
                return;
            }

            this.currentPlayer = player;

            // Apply stats
            var stats = player.GetStats();
            foreach (var stat in stats)
            {
                var view = GetOrCreateStatView();

                view.gameObject.name = "stat_" + stat.id;
                view.label.text = stat.value.ToString("0.##");
                view.isBuff = false;
                view.id = stat.id;

                var sprite = Resources.Load<Sprite>("icons/" + stat.icon);
                if (sprite == null)
                {
                    Debug.LogError("Sprite not found: " + stat.icon);
                    continue;
                }

                view.image.sprite = sprite;
            }

            // Apply buffs
            var buffs = player.GetBuffs();
            foreach (var buff in buffs)
            {
                var view = GetOrCreateStatView();

                view.gameObject.name = "buff_" + buff.id;
                view.label.text = buff.title;
                view.isBuff = true;
                view.id = buff.id;

                var sprite = Resources.Load<Sprite>("icons/" + buff.icon);
                if (sprite == null)
                {
                    Debug.LogError("Sprite not found: " + buff.icon);
                    continue;
                }

                view.image.sprite = sprite;
            }

            // Apply health to animator
            this.panel.character.SetInteger("Health", Mathf.CeilToInt(player.GetHealth()));

            // Subscribe
            player.onStatsChanged += OnStatsChanged;
        }

        private void OnAttackClick()
        {
            if (!currentPlayer.IsAlive())
                return;

            this.panel.character.SetTrigger("Attack");

            gameManager.DealDamage(this.playerId);
        }

        private void OnStatsChanged()
        {
            var player = this.currentPlayer;
            var stats = player.GetStats();
            foreach (var view in this.statViews)
            {
                if (!view.gameObject.activeSelf)
                    continue;

                if (view.isBuff)
                    continue;

                var stat = player.GetStat(view.id);
                view.label.text = stat.value.ToString("0.##");
            }

            this.panel.character.SetInteger("Health", Mathf.CeilToInt(player.GetHealth()));
        }

        private StatView GetOrCreateStatView()
        {
            var view = this.statViews.FirstOrDefault(s => !s.gameObject.activeSelf);
            if (view == null)
            {
                var go = settings.statPrefab.Clone(panel.statsPanel);

                // TODO
                // I could create a component for more flexibility,
                // but I don't want to modify the source prefab
                view = new StatView
                {
                    gameObject = go,
                    image = go.transform.Find("Icon").GetComponent<Image>(),
                    label = go.transform.Find("Text").GetComponent<Text>()
                };

                this.statViews.Add(view);
            }

            view.gameObject.SetActive(true);
            return view;
        }

        private void ClearStatViews()
        {
            this.statViews.ForEach(v => v.gameObject.SetActive(false));
        }

        private void ResetCurrentPlayer()
        {
            if (currentPlayer != null)
            {
                currentPlayer.onStatsChanged -= OnStatsChanged;
                currentPlayer = null;
            }
        }
    }
}

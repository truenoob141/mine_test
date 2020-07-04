using Mine.Game;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Mine
{
    [RequireComponent(typeof(Button))]
    public class StartGameButton : MonoBehaviour
    {
        [SerializeField]
        private bool withBuffs;

        [Inject] private GameManager gameManager;

        private Button button;

        private void Awake()
        {
            this.button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            this.button.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            this.button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            if (gameManager.IsValidGame)
                gameManager.EndGame();

            gameManager.StartGame(this.withBuffs);
        }
    }
}

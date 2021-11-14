using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Player))]
public class PlayerBankClient : MonoBehaviour
{
    public BankService Bank { get; private set; }

    private void Awake()
    {
        GetComponent<Player>().OnPlayerWillSpawn += gameState =>
        {
            Bank = gameState.Services.BankService;
        };
    }
}

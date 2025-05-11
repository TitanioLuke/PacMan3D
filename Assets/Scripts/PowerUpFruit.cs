// Fruit.cs
using UnityEngine;

public class Fruit : MonoBehaviour
{
    public float boostDuration = 5f;
    public float boostedSpeed  = 10f;
    public int   scoreValue    = 50;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // chamadas de método, sem atribuição
            GameManager.Instance.ActivateSpeedBoost(boostDuration, boostedSpeed);
            GameManager.Instance.AddScore(scoreValue);
            Destroy(gameObject);
        }
    }
}
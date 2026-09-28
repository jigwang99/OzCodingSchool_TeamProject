using System.Collections.Generic;
using UnityEngine;

namespace PixelRestaurant.Gacha
{
    public class GachaInventoryObjectPool : MonoBehaviour
    {
        [Header("Inventory Card Pool")]
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private int initialPoolSize = 20;
        [SerializeField] private Transform poolParent;

        private readonly Queue<GameObject> availableObjects =
            new Queue<GameObject>();

        private readonly HashSet<GameObject> pooledObjects =
            new HashSet<GameObject>();

        private readonly HashSet<GameObject> allObjects =
            new HashSet<GameObject>();

        private void Awake()
        {
            InitializePool();
        }

        private void InitializePool()
        {
            if (cardPrefab == null)
                return;

            if (poolParent == null)
                poolParent = transform;

            int count = Mathf.Max(0, initialPoolSize);

            for (int i = 0; i < count; i++)
            {
                GameObject card = CreateCard();

                if (card != null)
                    AddToPool(card);
            }
        }

        private GameObject CreateCard()
        {
            if (cardPrefab == null)
                return null;

            GameObject card = Instantiate(
                cardPrefab,
                poolParent,
                false
            );

            card.SetActive(false);

            allObjects.Add(card);

            return card;
        }

        private void AddToPool(GameObject card)
        {
            if (card == null)
                return;

            if (pooledObjects.Contains(card))
                return;

            card.SetActive(false);

            card.transform.SetParent(
                poolParent,
                false
            );

            ResetTransform(card.transform);

            pooledObjects.Add(card);
            availableObjects.Enqueue(card);
        }

        public GameObject GetObject(Transform parent)
        {
            GameObject card = null;

            while (availableObjects.Count > 0)
            {
                GameObject candidate =
                    availableObjects.Dequeue();

                if (candidate == null)
                    continue;

                if (!pooledObjects.Remove(candidate))
                    continue;

                card = candidate;
                break;
            }

            if (card == null)
            {
                card = CreateCard();

                if (card == null)
                    return null;
            }

            if (parent != null)
            {
                card.transform.SetParent(
                    parent,
                    false
                );
            }

            ResetTransform(card.transform);

            card.SetActive(true);

            return card;
        }

        public void ReturnObject(GameObject card)
        {
            if (card == null)
                return;

            if (!allObjects.Contains(card))
                return;

            AddToPool(card);
        }

        public void ReturnAll(Transform inventoryGrid)
        {
            if (inventoryGrid == null)
                return;

            for (int i = inventoryGrid.childCount - 1;
                 i >= 0;
                 i--)
            {
                GameObject card =
                    inventoryGrid.GetChild(i).gameObject;

                if (allObjects.Contains(card))
                    ReturnObject(card);
            }
        }

        private void ResetTransform(Transform target)
        {
            target.localPosition = Vector3.zero;
            target.localRotation = Quaternion.identity;
            target.localScale = Vector3.one;
        }
    }
}
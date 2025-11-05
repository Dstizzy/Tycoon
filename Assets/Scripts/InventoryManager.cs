using System;

using UnityEngine;

public class InventoryManager : MonoBehaviour {

    public static InventoryManager Instance { get; private set; }

    const int MIN_PEARL_COUNT = 0;
    const int MAX_PEARL_COUNT = 1000;
    const int MIN_CRYSTAL_COUNT = MIN_PEARL_COUNT;
    const int MAX_CRYSTAL_COUNT = MAX_PEARL_COUNT;
    const int MIN_CRUDE_COUNT = MIN_PEARL_COUNT;
    const int MAX_CRUDE_COUNT = MAX_PEARL_COUNT;
    const int MIN_REINFORCED_COUNT = MIN_PEARL_COUNT;
    const int MAX_REINFORCED_COUNT = MAX_PEARL_COUNT;
    const int MIN_ARTIFACT_COUNT = MIN_PEARL_COUNT;
    const int MAX_ARTIFACT_COUNT = MAX_PEARL_COUNT;

    public int pearlCount;
    public int crystalCount;
    public int oreCount;
    public int crudeItemCount;
    public int reinforcedItemCount;
    public int artifactItemCount;

    private void Awake() {

        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
        } else {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        pearlCount = MIN_PEARL_COUNT;
        crystalCount = MIN_CRYSTAL_COUNT;
        crudeItemCount = MIN_CRUDE_COUNT;
        reinforcedItemCount = MIN_REINFORCED_COUNT;
        artifactItemCount = MIN_ARTIFACT_COUNT;
    }

    public void TryAddPearl(int pearlAmount) {
        if (pearlCount > MAX_PEARL_COUNT) {
            Debug.LogError("Pearl count is at maximum!");
            return;
        } else if ((pearlCount + pearlAmount) > MAX_PEARL_COUNT) {
            Debug.LogError("Pearl count is at maximum!");
        } else
            pearlCount += pearlAmount;

        OnPearlCountChanged?.Invoke(pearlCount);

        return;
    }
    public void TrySpendPearl(int pearlAmount) {
        if (pearlCount < MIN_PEARL_COUNT) {
            Debug.LogError("Pearl count is at minimum!");
            return;
        } else if (pearlCount < pearlAmount) {
            Debug.LogError("Not enough pearls to spend!");
            return;
        } else
            pearlCount -= pearlAmount;

        OnPearlCountChanged?.Invoke(pearlCount);

        return;
    }
    public void TryAddCrystal(int pearlAmount) {
        if (pearlCount > MAX_PEARL_COUNT) {
            Debug.LogError("Pearl count is at maximum!");
            return;
        } else if ((pearlCount + pearlAmount) > MAX_PEARL_COUNT) {
            Debug.LogError("Pearl count is at maximum!");
        } else
            pearlCount += pearlAmount;

        OnCrystalCountChanged?.Invoke(crystalCount);

        return;
    }
    public void TrySpendCrystal(int pearlAmount) {
        if (pearlCount < MIN_PEARL_COUNT) {
            Debug.LogError("Pearl count is at minimum!");
            return;
        } else if (pearlCount < pearlAmount) {
            Debug.LogError("Not enough pearls to spend!");
            return;
        } else
            pearlCount -= pearlAmount;

        OnCrystalCountChanged?.Invoke(crystalCount);

        return;
    }

    /* Add an amount of certain items from the inventory                                      */
    public void TryAddItem(string itemType, int itemAmount)
    {
        switch (itemType)
        {
            case "Crude Tool":
                if ((crudeItemCount + itemAmount) > MAX_CRUDE_COUNT)
                {
                    crudeItemCount = MAX_CRUDE_COUNT;
                    Debug.LogError("Crude Tool count is at maximum!");
                }
                else
                    crudeItemCount += itemAmount;
                OnCrudeItemCountChanged?.Invoke(crudeItemCount);
                break;
            case "Reinforced Tool":
                if ((reinforcedItemCount + itemAmount) > MAX_REINFORCED_COUNT)
                {
                    reinforcedItemCount = MAX_CRUDE_COUNT;
                    Debug.LogError("Reinforced Item count is at maximum!");
                }
                else
                    crudeItemCount += itemAmount;
                OnReinforcedItemCountChanged?.Invoke(reinforcedItemCount);
                break;
            case "Artifact":
                if ((artifactItemCount + itemAmount) > MAX_ARTIFACT_COUNT)
                {
                    artifactItemCount = MAX_CRUDE_COUNT;
                    Debug.LogError("Artifact count is at maximum!");
                }
                else
                    artifactItemCount += itemAmount;
                OnArtifactItemCountChanged?.Invoke(artifactItemCount);
                break;
            default:
                Debug.LogError("Invalid item type!");
                break;
        };

        return;

    }

    /* Remove an amount of certain items from the inventory                                      */
    public void TrySellItem(string itemType, int itemAmount)
    {
        switch (itemType)
        {
            case "Crude Tool":
                if ((crudeItemCount + itemAmount) > MAX_CRUDE_COUNT)
                {
                    crudeItemCount = MAX_CRUDE_COUNT;
                    Debug.LogError("Crude Tool count is at maximum!");
                }
                else
                    crudeItemCount += itemAmount;
                OnCrudeItemCountChanged?.Invoke(crudeItemCount);
                break;
            case "Reinforced Tool":
                if ((reinforcedItemCount + itemAmount) > MAX_REINFORCED_COUNT)
                {
                    reinforcedItemCount = MAX_CRUDE_COUNT;
                    Debug.LogError("Crude Tool count is at maximum!");
                }
                else
                    crudeItemCount += itemAmount;
                OnReinforcedItemCountChanged?.Invoke(reinforcedItemCount);
                break;
            case "Artifact":
                if ((artifactItemCount + itemAmount) > MAX_ARTIFACT_COUNT)
                {
                    artifactItemCount = MAX_CRUDE_COUNT;
                    Debug.LogError("Crude Tool count is at maximum!");
                }
                else
                    artifactItemCount += itemAmount;
                OnArtifactItemCountChanged?.Invoke(artifactItemCount);
                break;
            default:
                Debug.LogError("Invalid item type!");
                break;
        };

        return;

    }

    public Action<int> OnPearlCountChanged;
    public Action<int> OnCrystalCountChanged;
    public Action<int> OnOreCountChanged;
    public Action<int> OnCrudeItemCountChanged;
    public Action<int> OnReinforcedItemCountChanged;
    public Action<int> OnArtifactItemCountChanged;
}

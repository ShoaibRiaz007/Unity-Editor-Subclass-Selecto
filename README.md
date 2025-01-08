# Unity Editor Subclass Selector

A custom Unity Editor tool that simplifies subclass assignment for fields marked with `[SerializeReference]`. The `IsSubClass` attribute and its associated property drawer enable developers to dynamically select and assign subclasses through a dropdown interface in the Inspector.

## Features

- **Dynamic Subclass Assignment**: Easily assign any subclass of a base type to a `[SerializeReference]` field.
- **Editor Integration**: Utilizes Unity's custom property drawers to provide an intuitive UI.
- **Caching for Performance**: Implements caching to quickly fetch available subclasses.

## Usage

### Setup

1. Add the `IsSubClass` attribute to any `[SerializeReference]` field in your script:
    ```csharp
    [SerializeReference, IsSubClass]
    private BaseClass mySubclassInstance;
    ```

2. In the Unity Inspector, a dropdown will appear, listing all available subclasses of the base type (`BaseClass` in this case).

### Prerequisites

- Unity Editor 2021.2 or later (for `VisualElement` and `UIElements` support).
- Proper subclass hierarchy set up for the `[SerializeReference]` field.

### Example

```csharp
using UnityEngine;

public interface IRandom
{
    object GetRandom();
}

[System.Serializable]
public class RandomInt : IRandom
{
    [SerializeField] private int from, to;

    public object GetRandom()
    {
        return UnityEngine.Random.Range(from, to);
    }
}
[System.Serializable]
public class RandomFloat : IRandom
{
    [SerializeField] private float from, to;

    public object GetRandom()
    {
        return UnityEngine.Random.Range(from, to);
    }
}
[System.Serializable]
public class Item
{
    public int id;
    public string name;
}

[System.Serializable]
public class RandomItem : IRandom
{
    [SerializeField] private Item[] items;

    public object GetRandom()
    {
        if (items == null || items.Length == 0)
        {
            throw new System.NullReferenceException("At least one item is required in the items array.");
        }
        return items[UnityEngine.Random.Range(0, items.Length)];
    }
}

public class SubclassExample : MonoBehaviour
{
    [SerializeReference, IsSubClass] private IRandom _chestReward;
    private void OnEnable()
    {
        OpenChest();
    }
    private void OpenChest()
    {
        if (_chestReward == null)
        {
            Debug.LogWarning("No random generator assigned to the chest!");
            return;
        }

        var reward = _chestReward.GetRandom();
        Debug.Log($"Reward generated: {reward}");
    }
}


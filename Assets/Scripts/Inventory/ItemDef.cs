using UnityEngine;

[CreateAssetMenu(menuName="Game/Item", fileName="Item_")]
public class ItemDef : ScriptableObject
{
    public string id = "fuel";        // 내부 키 (예: "fuel")
    public string displayName = "연료통";
    public Sprite icon;
    public bool stackable = true;
    public int maxStack = 99;
}

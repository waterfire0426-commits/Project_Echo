using UnityEngine;
using System;

[DefaultExecutionOrder(-100)]
public class Hotbar : MonoBehaviour
{
    [Serializable] public class Slot { public ItemDef def; public int count; public bool Empty => def == null || count <= 0; }

    public int size = 8;
    public Slot[] slots;
    public int selected = 0;
    public Action OnChanged;

    void Awake() {
        if (slots == null || slots.Length != size) {
            slots = new Slot[size];
            for (int i = 0; i < size; i++) slots[i] = new Slot();
        }
        Changed();
    }

    // 선택/휠
    public void Select(int idx) { selected = Mathf.Clamp(idx, 0, size - 1); Changed(); }
    public void Cycle(int dir)
    {
        if (size <= 0) return; // 추가
        selected = (selected + (dir > 0 ? 1 : -1) + size) % size;
        Changed();
    }

    // 추가
    public bool Add(ItemDef def, int amount = 1) {
        if (!def || amount <= 0) return false;

        if (def.stackable) {
            for (int i = 0; i < size && amount > 0; i++) {
                var s = slots[i];
                if (s.def == def && s.count < def.maxStack) {
                    int add = Mathf.Min(amount, def.maxStack - s.count);
                    s.count += add; amount -= add;
                }
            }
        }
        for (int i = 0; i < size && amount > 0; i++) {
            var s = slots[i];
            if (s.Empty) {
                s.def = def;
                s.count = def.stackable ? Mathf.Min(amount, def.maxStack) : 1;
                amount -= s.count;
            }
        }
        Changed(); return amount == 0;
    }

    // 선택 슬롯이 특정 id인지
    public bool SelectedIs(string id, int min = 1) {
        if (string.IsNullOrEmpty(id)) return false;
        var s = slots[selected];
        return s.def && s.def.id == id && s.count >= min;
    }

    // 선택 슬롯에서 개수 차감
    public bool RemoveFromSelected(int amount = 1) {
        var s = slots[selected];
        if (s.def == null || s.count < amount) return false;
        s.count -= amount;
        if (s.count <= 0) { s.def = null; s.count = 0; }
        OnChanged?.Invoke();
        return true;
    }

    // 인벤토리 어딘가에 해당 id가 있는지
    public bool Has(string id, int min = 1) {
        if (string.IsNullOrEmpty(id)) return false;
        for (int i = 0; i < size; i++) {
            var s = slots[i];
            if (s.def && s.def.id == id && s.count >= min) return true;
        }
        return false;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha1)) Select(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) Select(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) Select(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) Select(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) Select(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) Select(5);
        if (Input.GetKeyDown(KeyCode.Alpha7)) Select(6);
        if (Input.GetKeyDown(KeyCode.Alpha8)) Select(7);

        float w = Input.mouseScrollDelta.y;
        if (Mathf.Abs(w) > 0.01f) Cycle(w > 0 ? 1 : -1);
    }

    void Changed() => OnChanged?.Invoke();
}

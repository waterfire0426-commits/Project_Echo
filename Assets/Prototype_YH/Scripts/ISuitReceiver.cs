using UnityEngine;

public interface ISuitReceiver {
    bool IsSuited { get; }
    void ApplySuit(bool suited);
}


using UnityEngine;

public interface IInteractable
{
    void OnFocus();                       // 조준 시작
    void OnUnfocus();                     // 조준 해제
    void Interact(GameObject interactor); // E키 눌렀을 때 실행
}

using UnityEngine;

[CreateAssetMenu(fileName = "New ArtPiece", menuName = "Heist/Art Piece Data")]
public class ArtPieceData : ScriptableObject
{
    [Header("Basic Info")]
    public string artName;      // 작품 이름 (예: 모나리자)
    public int price;           // 장물 가격 (예: 10000$)

    [Header("Visuals")]
    public Sprite uiSprite;     // 노트북/로딩화면에 띄울 아이콘 (Sprite)

    // ★ [핵심] 실제 게임에서 액자에 끼워 넣을 그림 원본
    // (그림 그리기 미니게임에서도 이걸 보고 베낍니다)
    public Texture2D targetTexture;
}
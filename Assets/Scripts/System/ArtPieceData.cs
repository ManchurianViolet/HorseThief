using UnityEngine;

[CreateAssetMenu(fileName = "New ArtPiece", menuName = "Heist/Art Piece Data")]
public class ArtPieceData : ScriptableObject
{
    [Header("Basic Info")]
    public string artName;     
    public int price;          

    [Header("Visuals")]
    public Sprite uiSprite;     
    public Texture2D targetTexture;
}
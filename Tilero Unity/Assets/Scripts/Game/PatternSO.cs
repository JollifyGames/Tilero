using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Pattern", menuName = "Game/Pattern")]
public class PatternSO : ScriptableObject
{
    [Header("Pattern Info")]
    [SerializeField] private string patternName;
    [TextArea(2, 4)]
    [SerializeField] private string description;
    
    [Header("Pattern Steps")]
    [SerializeField] private List<PatternStep> steps = new List<PatternStep>();
    
    public string PatternName => patternName;
    public string Description => description;
    public List<PatternStep> Steps => steps;
    
    [System.Serializable]
    public class PatternStep
    {
        public Vector2Int position;
        public PieceType pieceType;
        
        public PatternStep(Vector2Int pos, PieceType type)
        {
            position = pos;
            pieceType = type;
        }
    }
    
    // MovementPattern'e dönüştürme metodu (geriye uyumluluk için)
    public MovementPattern ToMovementPattern()
    {
        MovementPattern pattern = new MovementPattern(patternName);
        foreach (var step in steps)
        {
            pattern.Steps.Add(step.position);
        }
        return pattern;
    }
}
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MovementPattern
{
    [SerializeField] private string patternName;
    [SerializeField] private List<Vector2Int> steps = new List<Vector2Int>();
    
    public string PatternName => patternName;
    public List<Vector2Int> Steps => steps;
    
    public MovementPattern(string name)
    {
        patternName = name;
        steps = new List<Vector2Int>();
    }
    
    public MovementPattern(string name, List<Vector2Int> patternSteps)
    {
        patternName = name;
        steps = new List<Vector2Int>(patternSteps);
    }
    
    public List<Vector2Int> GetAbsoluteSteps(Vector2Int currentPosition, Direction currentDirection)
    {
        List<Vector2Int> absoluteSteps = new List<Vector2Int>();
        
        // Her step, (0,0)'dan itibaren relatif pozisyonu gösteriyor
        // (0,0) = player'ın şu anki pozisyonu
        // (1,0) = player'dan 1 sağ
        // (2,1) = player'dan 2 sağ, 1 yukarı
        foreach (var step in steps)
        {
            // Step'i player'ın baktığı yöne göre döndür
            Vector2Int rotatedStep = RotateStepByDirection(step, currentDirection);
            // Player'ın mevcut pozisyonuna ekle
            Vector2Int absolutePosition = currentPosition + rotatedStep;
            absoluteSteps.Add(absolutePosition);
        }
        
        return absoluteSteps;
    }
    
    private Vector2Int RotateStepByDirection(Vector2Int step, Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return step;
            case Direction.Down:
                return new Vector2Int(-step.x, -step.y);
            case Direction.Left:
                return new Vector2Int(-step.y, step.x);
            case Direction.Right:
                return new Vector2Int(step.y, -step.x);
            default:
                return step;
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

// Runtime'da oluşturulan rotate edilmiş pattern
[System.Serializable]
public class RotatedPatternSO : PatternSO
{
    private PatternSO originalPattern;
    private int rotationAngle; // 0, 90, 180, 270
    
    public static RotatedPatternSO CreateRotatedPattern(PatternSO original, int angle)
    {
        RotatedPatternSO rotated = ScriptableObject.CreateInstance<RotatedPatternSO>();
        rotated.originalPattern = original;
        rotated.rotationAngle = angle;
        rotated.name = $"{original.PatternName}_{angle}";
        
        return rotated;
    }
    
    // Steps property'sini override ederek runtime'da rotate edilmiş step'leri döndür
    private List<PatternStep> rotatedSteps;
    public override List<PatternStep> Steps
    {
        get
        {
            if (rotatedSteps == null || rotatedSteps.Count == 0)
            {
                rotatedSteps = new List<PatternStep>();
                if (originalPattern != null)
                {
                    foreach (var step in originalPattern.Steps)
                    {
                        Vector2Int rotatedPosition = RotateVector(step.position, rotationAngle);
                        rotatedSteps.Add(new PatternStep(rotatedPosition, step.pieceType));
                    }
                }
            }
            return rotatedSteps;
        }
    }
    
    private static Vector2Int RotateVector(Vector2Int pos, int angle)
    {
        switch (angle)
        {
            case 0:
                return pos;
            case 90:
                return new Vector2Int(-pos.y, pos.x);
            case 180:
                return new Vector2Int(-pos.x, -pos.y);
            case 270:
                return new Vector2Int(pos.y, -pos.x);
            default:
                return pos;
        }
    }
    
    public override string PatternName => $"{originalPattern?.PatternName ?? "Unknown"} ({rotationAngle}°)";
    public override string Description => originalPattern?.Description ?? "";
}
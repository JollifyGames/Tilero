using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }
    [Header("Movement Settings")]
    [SerializeField] private float moveAnimationDuration = 0.3f;
    [SerializeField] private float jumpHeightX = 0.5f;
    [SerializeField] private float jumpHeightY = 0.5f;
    [SerializeField] private AnimationCurve jumpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Visual Settings")]
    [SerializeField] private Transform visualTransform;
    [SerializeField] private float rotationDuration = 0.2f;
    
    private BoardManager boardManager;
    private bool canMove = true;
    private bool isMoving = false;
    private Vector2Int lastMoveDirection = Vector2Int.zero;
    private Direction currentDirection = Direction.Down;
    
    public bool IsMoving => isMoving;
    public Direction CurrentDirection => currentDirection;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[PlayerController] Another instance exists, keeping the existing one");
            Destroy(this);
            return;
        }
        Instance = this;
    }
    
    public void Initialize(BoardManager boardManager)
    {
        if (this == null) return;
        
        this.boardManager = boardManager;
        
        if (visualTransform == null)
        {
            visualTransform = transform;
        }
        
        UpdateVisualDirection();
        Debug.Log("[PlayerController] Initialized");
    }
    
    private void Start()
    {
        if (boardManager == null && BoardManager.Instance != null)
        {
            boardManager = BoardManager.Instance;
        }
    }
    
    public void TestMovement()
    {
        if (!canMove || boardManager == null || isMoving) return;
        
        Vector2Int moveDirection = Vector2Int.zero;
        
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            moveDirection = Vector2Int.up;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            moveDirection = Vector2Int.down;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            moveDirection = Vector2Int.left;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            moveDirection = Vector2Int.right;
        }
        
        if (moveDirection != Vector2Int.zero)
        {
            lastMoveDirection = moveDirection;
            UpdateDirection(moveDirection);
            bool moved = boardManager.TryMovePlayer(moveDirection);
            if (moved)
            {
                Debug.Log($"[PlayerController] Moving {moveDirection}");
            }
        }
    }
    
    public void MoveToPosition(Vector3 targetPosition)
    {
        if (isMoving) return;
        
        isMoving = true;
        boardManager.SetMovingState(true);
        
        Vector3 startPos = transform.position;
        Vector3 midPoint = (startPos + targetPosition) / 2f;
        
        // X ekseninde hareket ediyorsa jumpHeightX, Y ekseninde hareket ediyorsa jumpHeightY kullan
        float currentJumpHeight = (lastMoveDirection.x != 0) ? jumpHeightX : jumpHeightY;
        midPoint.y += currentJumpHeight;
        
        Sequence moveSequence = DOTween.Sequence();
        
        moveSequence.Append(transform.DOMove(midPoint, moveAnimationDuration * 0.5f)
            .SetEase(jumpCurve));
        
        moveSequence.Append(transform.DOMove(targetPosition, moveAnimationDuration * 0.5f)
            .SetEase(jumpCurve));
        
        moveSequence.OnComplete(() =>
        {
            isMoving = false;
            boardManager.SetMovingState(false);
            Debug.Log("[PlayerController] Move completed");
        });
        
        moveSequence.Play();
    }
    
    private void UpdateDirection(Vector2Int moveDirection)
    {
        Direction newDirection = currentDirection;
        
        if (moveDirection == Vector2Int.up)
            newDirection = Direction.Up;
        else if (moveDirection == Vector2Int.down)
            newDirection = Direction.Down;
        else if (moveDirection == Vector2Int.left)
            newDirection = Direction.Left;
        else if (moveDirection == Vector2Int.right)
            newDirection = Direction.Right;
        
        if (newDirection != currentDirection)
        {
            currentDirection = newDirection;
            UpdateVisualDirection();
        }
    }
    
    private void UpdateVisualDirection()
    {
        if (visualTransform == null) return;
        
        float targetRotation = 0f;
        
        switch (currentDirection)
        {
            case Direction.Up:
                targetRotation = 0f;
                break;
            case Direction.Down:
                targetRotation = 180f;
                break;
            case Direction.Left:
                targetRotation = 90f;
                break;
            case Direction.Right:
                targetRotation = -90f;
                break;
        }
        
        visualTransform.DORotate(new Vector3(0, 0, targetRotation), rotationDuration)
            .SetEase(Ease.OutQuad);
    }
    
    public void SetDirection(Direction direction)
    {
        currentDirection = direction;
        UpdateVisualDirection();
    }
    
    public void ExecuteMovementPattern(PatternSO pattern, List<Vector2Int> absoluteSteps)
    {
        if (isMoving || pattern == null || absoluteSteps.Count == 0) return;
        
        StartCoroutine(ExecutePatternSequence(pattern, absoluteSteps));
    }
    
    private IEnumerator ExecutePatternSequence(PatternSO pattern, List<Vector2Int> absoluteSteps)
    {
        canMove = false;
        // boardManager.SetMovingState(true); // Bu satır TryMoveObject'i blokluyor
        
        Debug.Log($"[PlayerController] Executing pattern: {pattern.PatternName} with {absoluteSteps.Count} steps");
        
        Vector2Int startCell = boardManager.GetPlayerCell();
        Vector2Int currentCell = startCell;
        PieceType currentPieceType = PieceType.Basic; // Default
        
        int stepIndex = 0;
        foreach (var targetCell in absoluteSteps)
        {
            Vector2Int moveDirection = targetCell - currentCell;
            
            if (moveDirection.x != 0 || moveDirection.y != 0)
            {
                UpdateDirection(moveDirection);
                
                // isMoving'i pattern sequence kontrol etmemeli, MoveToPosition kontrol etmeli
                bool canMoveToTarget = boardManager.TryMoveObject(
                    gameObject, 
                    currentCell, 
                    targetCell,
                    (targetPos) => MoveToPositionForPattern(targetPos)
                );
                
                if (canMoveToTarget)
                {
                    // MoveToPositionForPattern tamamlanana kadar bekle
                    yield return new WaitUntil(() => !isMoving);
                    currentCell = targetCell;
                    
                    // Player'ın durduğu pozisyonun PieceType'ını bul
                    if (stepIndex < pattern.Steps.Count)
                    {
                        currentPieceType = pattern.Steps[stepIndex].pieceType;
                        Debug.Log($"[PlayerController] Moved to step {stepIndex}, PieceType: {currentPieceType}");
                    }
                }
                else
                {
                    Debug.Log($"[PlayerController] Cannot move to {targetCell}, stopping pattern execution");
                    break;
                }
            }
            stepIndex++;
        }
        
        canMove = true;
        // boardManager.SetMovingState(false);
        Debug.Log($"[PlayerController] Pattern execution completed at position with PieceType: {currentPieceType}");
        
        // Defense PieceType kontrolü
        if (currentPieceType == PieceType.Defense)
        {
            PlayerCharacter playerChar = GetComponent<PlayerCharacter>();
            if (playerChar != null)
            {
                playerChar.ApplyDefenseBuff();
                Debug.Log("[PlayerController] Defense buff applied!");
            }
        }
        
        // Pattern tamamlandıktan sonra enemy kontrol et ve attack yap
        yield return CheckAndAttackEnemy(currentPieceType);
        
        // Attack işlemi bittikten sonra turn'ü enemy'e geç
        if (WorldManager.Instance != null)
        {
            WorldManager.Instance.OnPlayerActionComplete();
        }
    }
    
    private IEnumerator CheckAndAttackEnemy(PieceType pieceType)
    {
        // Player ve Defense piece type'ları attack yapmaz
        if (pieceType == PieceType.Player || pieceType == PieceType.Defense)
        {
            Debug.Log($"[PlayerController] PieceType {pieceType} does not trigger attack");
            yield break;
        }
        
        Vector2Int currentPosition = boardManager.GetPlayerCell();
        EnemyCharacter enemy = boardManager.FindEnemyInDirection(currentPosition, currentDirection);
        
        if (enemy != null)
        {
            // Enemy bulundu, enemy'e doğru dön
            Vector2Int enemyPosition = GridManager.Instance.GetCellFromWorldPosition(enemy.transform.position).GridPosition;
            Direction directionToEnemy = boardManager.GetDirectionToTarget(currentPosition, enemyPosition);
            
            if (directionToEnemy != currentDirection)
            {
                SetDirection(directionToEnemy);
                yield return new WaitForSeconds(rotationDuration); // Dönüş animasyonu bitene kadar bekle
            }
            
            // Attack uygula (sadece Basic, Attack, Special için)
            Attack(enemy, pieceType);
        }
    }
    
    private void Attack(EnemyCharacter target, PieceType pieceType)
    {
        Debug.Log($"[PlayerController] === ATTACK === Attacking {target.gameObject.name} with PieceType: {pieceType}!");
        
        // Get player damage from model
        PlayerCharacter playerChar = GetComponent<PlayerCharacter>();
        if (playerChar == null || playerChar.Model == null)
        {
            Debug.LogError("[PlayerController] PlayerCharacter or Model not found!");
            return;
        }
        
        // Calculate damage using multiplier from PlayerCharacter
        float multiplier = playerChar.GetDamageMultiplier(pieceType);
        int baseDamage = playerChar.Model.Damage;
        int damageAmount = Mathf.RoundToInt(baseDamage * multiplier);
        
        Debug.Log($"[PlayerController] {pieceType} attack - Base: {baseDamage} x {multiplier} = {damageAmount} damage");
        
        // Apply damage to enemy
        target.TakeDamage(damageAmount);
    }
    
    private void MoveToPositionForPattern(Vector3 targetPosition)
    {
        isMoving = true;
        
        Vector3 startPos = transform.position;
        Vector3 midPoint = (startPos + targetPosition) / 2f;
        
        float currentJumpHeight = (lastMoveDirection.x != 0) ? jumpHeightX : jumpHeightY;
        midPoint.y += currentJumpHeight;
        
        Sequence moveSequence = DOTween.Sequence();
        
        moveSequence.Append(transform.DOMove(midPoint, moveAnimationDuration * 0.5f)
            .SetEase(jumpCurve));
        
        moveSequence.Append(transform.DOMove(targetPosition, moveAnimationDuration * 0.5f)
            .SetEase(jumpCurve));
        
        moveSequence.OnComplete(() =>
        {
            isMoving = false;
            Debug.Log("[PlayerController] Move completed");
        });
        
        moveSequence.Play();
    }
}
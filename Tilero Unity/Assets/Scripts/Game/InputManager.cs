using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    
    private PlayerController playerController;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }
    
    public void SetPlayerController(PlayerController controller)
    {
        playerController = controller;
    }
    
    private void Update()
    {
        TestInput();
    }
    
    private void TestInput()
    {
        if (playerController != null)
        {
            playerController.TestMovement();
        }
    }
}
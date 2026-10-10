using UnityEngine;
using UnityEngine.InputSystem;

public class CoinMovement : MonoBehaviour
{

    private Vector3 startPosition;
    private Vector3 endPosition;

    private GameBoard board;
    private Coin coin;

    private bool isDragging = false;

    void Start(){
        board = FindAnyObjectByType<GameBoard>();
        coin = GetComponent<Coin>();
    }

    void OnMouseDown(){
        if(Mouse.current == null || Camera.main == null){
            return;
        }
        isDragging = true;
        startPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        startPosition.z = 0;
        
       
    }

    void OnMouseUp(){
        if(!isDragging){
            return;
        }
        isDragging = false;
        if (Mouse.current == null || Camera.main == null || board == null || coin == null){
            return;
        }
       Vector3 mousePosition = Mouse.current.position.ReadValue();
       endPosition = Camera.main.ScreenToWorldPoint(mousePosition);
       endPosition.z = 0;
       
       Vector3 swipe = endPosition - startPosition;
        if(swipe.magnitude < 0.3f){
            return;
            }

            Vector2Int direction;
            if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
    {
        direction = swipe.x > 0
            ? Vector2Int.right
            : Vector2Int.left;
    }
    else
    {
        direction = swipe.y > 0
            ? Vector2Int.up
            : Vector2Int.down;
    }

    Debug.Log("Selected coin: (" + coin.x + ", " + coin.y +
              "), direction: " + direction);

    board.HandleSwipe(coin, direction);
}}
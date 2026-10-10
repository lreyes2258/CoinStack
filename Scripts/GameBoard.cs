using UnityEngine;
using TMPro;

public class GameBoard : MonoBehaviour
{
    //number of columns on the board
    public int width = 8;
    //number of rows on the board
    public int height = 8;
    //player's score
    public int score = 0;

    //number of levels
    public int level = 1;
    public int pointsPerLevel = 1000;
    public TMP_Text levelText;

    //coin prefabs
    public Coin pennyPrefab;
    public Coin nickelPrefab;
    public Coin dimePrefab;
    public Coin quarterPrefab;
    public TMP_Text scoreText;

    //audio sounds
    public AudioClip mergeSound;
    public AudioClip scoreSound;

    private AudioSource audioSource;

    //2D array for coin tracking
    Coin[,] coins;
    bool checking = false;

    //adds points to player's score
    void AddPoints(int points){
        score += points;

        //checks if reached next level
        UpdateLevel();

        //update score
        if(scoreText != null){
        scoreText.text = "Score: " + score;
        }
        if(points > 0 && audioSource != null && scoreSound != null){
            audioSource.PlayOneShot(scoreSound);
        }

    }

    //starts the game board
    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if(scoreText != null){
            scoreText.text = "Score: " + score;
        }
        //creates 8 x 8 game board
        coins = new Coin[width, height];

        //loop through all the columns
        for (int x = 0; x < width; x++)
        {
            //loop through all the rows
            for (int y = 0; y < height; y++)
            {
                //creates a coin at (x,y)
                CreateCoin(x, y, true);
            }
        }
        scoreText.text = "Score: " + score;
        UpdateLevel();
    }

    //updates player level
    void UpdateLevel(){

        //every level is worth 1000 points
        level = (score/pointsPerLevel) + 1;

        if(levelText != null){
            levelText.text = "Level: " + level;
        }

        Debug.Log("Current Level: " + level);
    }

    //creates a coin at a specific board position
    void CreateCoin(int x, int y, bool avoidMatches = false)
    {
        //all four possible coin choices
        Coin[] prefabs = {
            pennyPrefab, nickelPrefab, dimePrefab, quarterPrefab
        };

        Coin prefab = prefabs[Random.Range(0, prefabs.Length)];

        //this tries up to 30 times to find a non matching coin
        if (avoidMatches)
        {
            for (int tries = 0; tries < 30; tries++)
            {
                if (!WouldMatch(x, y, prefab.coinValue))
                    break;

                prefab = prefabs[Random.Range(0, prefabs.Length)];
            }
        }

        //creates the coin
        Coin coin = Instantiate(
            prefab, new Vector3(x, y, 0), Quaternion.identity
        );

        //saves coordinates and stores in board
        coin.x = x;
        coin.y = y;
        coins[x, y] = coin;
    }

    //checks for matches right away of 3 or more matching coins
    bool WouldMatch(int x, int y, int value)
    {
        //checks horizontally
        if (x >= 2 && coins[x - 1, y] != null &&
            coins[x - 2, y] != null &&
            coins[x - 1, y].coinValue == value &&
            coins[x - 2, y].coinValue == value)
            return true;
            
        //checks vertically
        if (y >= 2 && coins[x, y - 1] != null &&
            coins[x, y - 2] != null &&
            coins[x, y - 1].coinValue == value &&
            coins[x, y - 2].coinValue == value)
            return true;

        return false;
    }

    //player movement
    public void HandleSwipe(Coin coin, Vector2Int direction)
    {
        if (coin == null || checking) return;

        int x = coin.x + direction.x;
        int y = coin.y + direction.y;

        if (x < 0 || x >= width || y < 0 || y >= height)
            return;

            //finds the coin where it is moved to
        Coin other = coins[x, y];
        if (other == null) {
            return;
        }

        //saves old coin position
        int oldX = coin.x;
        int oldY = coin.y;

        //swaps the coins in the board
        coins[oldX, oldY] = other;
        coins[x, y] = coin;

        //updates coin coordinates
        coin.x = x;
        coin.y = y;
        other.x = oldX;
        other.y = oldY;

        coin.transform.position = new Vector3(x, y, 0);
        other.transform.position = new Vector3(oldX, oldY, 0);

        //merge if coins are the same
        if (coin.coinValue == other.coinValue)
        {
            MergeCoins(coin, other);
            return;
        }

        //checks for match
        if (CountMatches(coin) >= 3)
            RemoveMatches(coin);
        else if (CountMatches(other) >= 3)
            RemoveMatches(other);
    }

    //counts how many matched coins only vertically or horizontally
    int CountMatches(Coin coin)
    {
        if (coin == null) return 0;

        int horizontal = 1;
        int vertical = 1;

        //checks the left side
        for (int x = coin.x - 1; x >= 0; x--)
        {
            if (coins[x, coin.y] == null ||
                coins[x, coin.y].coinValue != coin.coinValue) break;
            horizontal++;
        }
        
        
        //checks the right side
        for (int x = coin.x + 1; x < width; x++)
        {
            if (coins[x, coin.y] == null ||
                coins[x, coin.y].coinValue != coin.coinValue) break;
            horizontal++;
        }

        //checks down
        for (int y = coin.y - 1; y >= 0; y--)
        {
            if (coins[coin.x, y] == null ||
                coins[coin.x, y].coinValue != coin.coinValue) break;
            vertical++;
        }

        //checks up
        for (int y = coin.y + 1; y < height; y++)
        {
            if (coins[coin.x, y] == null ||
                coins[coin.x, y].coinValue != coin.coinValue) break;
            vertical++;
        }

        return Mathf.Max(horizontal, vertical);
    }

    //removes matching coins off board
    void RemoveMatches(Coin coin)
    {
        if (coin == null || checking) return;
        checking = true;

        int x = coin.x;
        int y = coin.y;
        int value = coin.coinValue;

        bool[,] remove = new bool[width, height];

        int left = x;
        int right = x;
        int down = y;
        int up = y;

        //finds matches to the left
        while (left > 0 && coins[left - 1, y] != null &&
               coins[left - 1, y].coinValue == value) {
                left--;
               }

        //finds matches to the right
        while (right < width - 1 && coins[right + 1, y] != null &&
               coins[right + 1, y].coinValue == value) {
                right++;
               }
        
        //finds matches below
        while (down > 0 && coins[x, down - 1] != null &&
               coins[x, down - 1].coinValue == value) {
                down--;
               }

        //finds matches above
        while (up < height - 1 && coins[x, up + 1] != null &&
               coins[x, up + 1].coinValue == value) {
                up++;
               }

        if (right - left + 1 >= 3)
            for (int i = left; i <= right; i++)
                remove[i, y] = true;

        if (up - down + 1 >= 3)
            for (int i = down; i <= up; i++)
                remove[x, i] = true;

        int removed = 0;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (remove[i, j] && coins[i, j] != null)
                {
                    Destroy(coins[i, j].gameObject);
                    coins[i, j] = null;
                    removed++;
                }
            }
        }

       int pointsPerCoin = 0;


        // penny = 1 point.
        if (value == 1)
        {
            pointsPerCoin = 1;
        }

        // nickel = 5 points.
        else if (value == 5)
        {
            pointsPerCoin = 5;
        }

        // dime = 10 points.
        else if (value == 10)
        {
            pointsPerCoin = 10;
        }

        // quarter = 25 points.
        else if (value == 25)
        {
            pointsPerCoin = 25;
        }
        int points = removed * pointsPerCoin;

        AddPoints(points);
        Debug.Log("Score: " + score);

        //refills empty spaces
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (coins[i, j] == null)
                    CreateCoin(i, j);
            }
        }

        checking = false;

        //looks for new matches created by refilling
        CheckBoard();
    }

    //searches entire board for matches
    void CheckBoard()
    {
        if (checking) return;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                //checks for 3 or more
                if (coins[x, y] != null && CountMatches(coins[x, y]) >= 3)
                {
                    RemoveMatches(coins[x, y]);
                    return;
                }
            }
        }
    }

    //combines two matching coins into next highest coin
    void MergeCoins(Coin coin, Coin other)
    {
        GameObject next = coin.transformedCoin;

        if (next == null){
            next = other.transformedCoin;
        }

        if (next == null) {return;
        }
        if (audioSource != null && mergeSound != null)
        {
            audioSource.PlayOneShot(mergeSound);
            }

        //saves both coin positions
        int x = coin.x;
        int y = coin.y;
        int otherX = other.x;
        int otherY = other.y;

        //destroys the two coins
        Destroy(coin.gameObject);
        Destroy(other.gameObject);

        //removes coins from board
        coins[x, y] = null;
        coins[otherX, otherY] = null;

        //creates new coin
        Coin newCoin = Instantiate(
            next, new Vector3(x, y, 0), Quaternion.identity
        ).GetComponent<Coin>();

        newCoin.x = x;
        newCoin.y = y;
        coins[x, y] = newCoin;

        CreateCoin(otherX, otherY);
        CheckBoard();
    }
}
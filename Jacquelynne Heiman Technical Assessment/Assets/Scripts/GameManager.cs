using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Components")]
    [SerializeField] private MazeGenerator mazeGenerator;
    [HideInInspector] public MazeSolver solver;

    [Header("Maze Info")]
    public bool debugMode;
    [SerializeField] private string[] mazePath;

    public GameObject[,] maze;
    [SerializeField] int height, width;

    [Header("Level Info")]
    private int levelIndex;

    [Header("Character")]
    public GameObject characterPrefab;
    private SpriteMover character;

    [Header("UI")]
    public GameObject startMenu;
    public GameObject endMenu;
    public GameObject pauseMenu;
    public GameObject gameOverMenu;

    [Header("Audio - Music")]
    public AudioSource audioSource;
    public AudioClip[] levelMusic;
    public AudioClip menuMusic;
    public AudioClip endStage;

    [Header("Audio - SFX")]
    public AudioClip runningSFX;
    public AudioClip buttonHover;
    public AudioClip buttonClick;

    bool isGameStarted;

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }

        //set our current level to 0
        levelIndex = 0;
    }

    public void StartGame()
    {
        //remove the start screen
        startMenu.SetActive(false);

        InitializeMap();

        InitializeCharacter();

        //Tell the character their goal position
        character.SetTargetPosition(solver.endCoordinates);

        isGameStarted = true;
    
    }

    void InitializeMap()
    {
        //generate the maze
        mazeGenerator.ReadMaze(mazePath[levelIndex]);

        //set the height and width of the maze
        height = mazeGenerator.height;
        width = mazeGenerator.width;

        //create a new maze tile list
        maze = new GameObject[width, height];

        //begin find the solution for the maze
        solver.StartSolution(mazeGenerator.maze, height, width);
    }

    void InitializeCharacter()
    {
        //spawn the character and set them at the starting tile position

        GameObject newCharacter = Instantiate(characterPrefab);

        Vector3 startPosition = newCharacter.transform.position;
        startPosition = solver.startCoordinates;
        newCharacter.transform.position = startPosition;

        //get access to the character's mover script
        character = newCharacter.GetComponent<SpriteMover>();
    }

    void NextLevel()
    {
        //set our level to the next one
        levelIndex++;

        //destroy the character
        Destroy(character.gameObject);
        character = null;

        //delete all the tiles from the world
        foreach(Tile tile in mazeGenerator.GetComponentsInChildren<Tile>())
        {
            Destroy(tile.gameObject);
        }

        //create the next maze and character
        InitializeMap();
        InitializeCharacter();

        //set the correct music
        audioSource.clip = levelMusic[levelIndex];
    }

    public void PlayNextLevel()
    {
        //turn off our end stage UI
        endMenu.SetActive(false);

        //play the level music
        audioSource.Play();

        //set the character's target position
        character.SetTargetPosition(solver.endCoordinates);

        //start the game
        isGameStarted = true;
    }

    void EndGame()
    {
        //reset level

        levelIndex = 0;

        //Destroy the current character if there is one
        if (character)
        {
            Destroy(character.gameObject);
            character = null; 
        }

        //Destroy all the maze tiles
        foreach (Tile tile in mazeGenerator.GetComponentsInChildren<Tile>())
        {
            Destroy(tile.gameObject);
        }

    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void MainMenu()
    {
        //turn off any UI that isn't the main menu if there is one on
        if(endMenu.activeSelf)
        {
            endMenu.SetActive(false);
        }

        if(gameOverMenu.activeSelf)
        {
            gameOverMenu.SetActive(false);
        }
        
        if(pauseMenu.activeSelf)
        {
            pauseMenu.SetActive(false);
        }

        startMenu.SetActive(true);

        //reset the game
        EndGame();

        //if the game is paused
        if(Time.timeScale == 0)
        {
            //unpause it
            Time.timeScale = 1;
        }
        
        //play the menu music
        audioSource.clip = menuMusic;
        audioSource.Play();
    }

    public void PauseGame()
    {
        //pause the game
        Time.timeScale = 0;

        //bring up the pause menu
        pauseMenu.SetActive(true);
    }

    public void UnpauseGame()
    {
        //turn off the pause menu
        pauseMenu.SetActive(false);

        //unpause the game
        Time.timeScale = 1;
    }

    void YouWin()
    {
        //when the player finishes the last maze
        gameOverMenu.SetActive(true);

        //play menu music
        audioSource.clip = menuMusic;
        audioSource.Play();
    }

    private void Update()
    {
        //if the game is started and there is a character
        if(isGameStarted && character)
        {
            //move the character
            character.HandleMovement();
        }

        //if there is a character
        if(character != null)
        {
            //and they are at the goal and we haven't yet opened our end stage UI
            if (character.isAtGoal && !endMenu.activeSelf)
            {
                
                isGameStarted = false;

                //activate our end stage UI
                endMenu.SetActive(true);
               
                //if we are level 0, 1, 2 or 3
                if(levelIndex < 4)
                {
                    //go to the next level
                    NextLevel();
                }
                else
                {
                    //end the game, you win!
                    YouWin();
                }
                
            }
        }
        
    }


}

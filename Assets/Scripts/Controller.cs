using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    //GameObjects
    public GameObject board;
    public GameObject[] cops = new GameObject[2];
    public GameObject robber;
    public Text rounds;
    public Text finalMessage;
    public Button playAgainButton;

    //Otras variables
    Tile[] tiles = new Tile[Constants.NumTiles];
    private int roundCount = 0;
    private int state;
    private int clickedTile = -1;
    private int clickedCop = 0;
    private int[] level = new int[64];
    private bool[] vis = new bool[64];
    void Start()
    {        
        InitTiles();
        InitAdjacencyLists();
        state = Constants.Init;
      
    }
        
    //Rellenamos el array de casillas y posicionamos las fichas
    void InitTiles()
    {
        for (int fil = 0; fil < Constants.TilesPerRow; fil++)
        {
            GameObject rowchild = board.transform.GetChild(fil).gameObject;            

            for (int col = 0; col < Constants.TilesPerRow; col++)
            {
                GameObject tilechild = rowchild.transform.GetChild(col).gameObject;                
                tiles[fil * Constants.TilesPerRow + col] = tilechild.GetComponent<Tile>();                         
            }
        }
                
        cops[0].GetComponent<CopMove>().currentTile=Constants.InitialCop0;
        cops[1].GetComponent<CopMove>().currentTile=Constants.InitialCop1;
        robber.GetComponent<RobberMove>().currentTile=Constants.InitialRobber;           
    }

    public void InitAdjacencyLists()
    {
        //Matriz de adyacencia
        int[,] matrix= new int[Constants.NumTiles, Constants.NumTiles];

        //TODO: Inicializar matriz a 0's
        for(int c=0; c < Constants.NumTiles; c++)
        {
            for(int f=0; f< Constants.NumTiles; f++)
            {
                matrix[c, f] = 0;
            }
        }
        /* int cont = 0;
         for (int c = 0; c < Constants.NumTiles; c++)
         {
             for (int f = 0; f < Constants.NumTiles; f++)
             {
                 Debug.Log(matrix[c, f]);
                 cont++;
             }
         }
         Debug.Log(cont);*/

        //TODO: Para cada posición, rellenar con 1's las casillas adyacentes (arriba, abajo, izquierda y derecha)
        for (int c = 0; c < Constants.NumTiles; c++)
        {
            int down = c - 8;
            int up = c + 8;
            if (down >= 0)
            {
                matrix[c, down] = 1;
            }
            if (up <= 63)
            {
                matrix[c, up] = 1;
            }
            if ( c % 8 == 7)
            {                           
                matrix[c, c - 1] = 1;
            }else if (c % 8 == 0)
            {                          
                matrix[c, c + 1] = 1;
            }
            else
            {           
                matrix[c, c + 1] = 1;
                matrix[c, c - 1] = 1;
            }
        }

      /*  int cont = 0;
        for (int c = 0; c < Constants.NumTiles; c++)
        {
            for (int f = 0; f < Constants.NumTiles; f++)
            {
                Debug.Log("Columna "+ c+"Fila "+f+"= "+matrix[c, f]);
                cont++;
            }
        }
        Debug.Log(cont);*/
        //TODO: Rellenar la lista "adjacency" de cada casilla con los índices de sus casillas adyacentes
        for (int c = 0; c < Constants.NumTiles; c++)
        {
            for (int f = 0; f < Constants.NumTiles; f++)
            {
                if(matrix[c, f]==1)
                tiles[c].adjacency.Add(tiles[f].numTile);
            }
        }


        /* for (int c = 0; c < Constants.NumTiles; c++)
         {
             for (int f = 0; f < tiles[c].adjacency.Count; f++)
             {
                 Debug.Log("Casilla " + c + "Adjacency " + f + "= " + tiles[c].adjacency[f]);

             }
         }*/
    }

    //Reseteamos cada casilla: color, padre, distancia y visitada
    public void ResetTiles()
    {        
        foreach (Tile tile in tiles)
        {
            tile.Reset();
        }
    }

    public void ClickOnCop(int cop_id)
    {
        switch (state)
        {
            case Constants.Init:
            case Constants.CopSelected:                
                clickedCop = cop_id;
                clickedTile = cops[cop_id].GetComponent<CopMove>().currentTile;
                tiles[clickedTile].current = true;

                ResetTiles();
                FindSelectableTiles(true);

                state = Constants.CopSelected;                
                break;            
        }
    }

    public void ClickOnTile(int t)
    {                     
        clickedTile = t;

        switch (state)
        {            
            case Constants.CopSelected:
                //Si es una casilla roja, nos movemos
                if (tiles[clickedTile].selectable)
                {                  
                    cops[clickedCop].GetComponent<CopMove>().MoveToTile(tiles[clickedTile]);
                    cops[clickedCop].GetComponent<CopMove>().currentTile=tiles[clickedTile].numTile;
                    tiles[clickedTile].current = true;   
                    
                    state = Constants.TileSelected;
                }                
                break;
            case Constants.TileSelected:
                state = Constants.Init;
                break;
            case Constants.RobberTurn:
                state = Constants.Init;
                break;
        }
    }

    public void FinishTurn()
    {
        switch (state)
        {            
            case Constants.TileSelected:
                ResetTiles();

                state = Constants.RobberTurn;
                RobberTurn();
                break;
            case Constants.RobberTurn:                
                ResetTiles();
                IncreaseRoundCount();
                if (roundCount <= Constants.MaxRounds)
                    state = Constants.Init;
                else
                    EndGame(false);
                break;
        }

    }

    public void RobberTurn()
    {
        clickedTile = robber.GetComponent<RobberMove>().currentTile;
        tiles[clickedTile].current = true;
        FindSelectableTiles(false);

        /*TODO: Cambia el código de abajo para hacer lo siguiente
        - Elegimos una casilla aleatoria entre las seleccionables que puede ir el caco
        - Movemos al caco a esa casilla
        - Actualizamos la variable currentTile del caco a la nueva casilla
        */
        int distanceAux = 0;
        List<Tile> lista = new List<Tile>();
        for(int i =0; i<Constants.NumTiles; i++)
        {
            if(tiles[i].selectable && tiles[i].numTile != cops[0].GetComponent<CopMove>().currentTile && tiles[i].numTile != cops[1].GetComponent<CopMove>().currentTile && tiles[i].numTile != clickedTile)
            {
                //distanceAux= tiles[i].distance
                lista.Add(tiles[i]);

            }
        }
        Debug.Log("Dfs: " + BFS(clickedTile, cops[0].GetComponent<CopMove>().currentTile));
        Debug.Log("Dfs2: " + BFS(clickedTile, cops[1].GetComponent<CopMove>().currentTile));
        Tile there = new Tile();
        double maxDis =0;
        if (BFS(robber.GetComponent<RobberMove>().currentTile, cops[0].GetComponent<CopMove>().currentTile) < BFS(robber.GetComponent<RobberMove>().currentTile, cops[1].GetComponent<CopMove>().currentTile))
        {
            foreach (Tile c in lista)
            {

                if (BFS(c.numTile, cops[0].GetComponent<CopMove>().currentTile) > maxDis)
                {
                    maxDis = BFS(c.numTile, cops[0].GetComponent<CopMove>().currentTile);
                    there = c;
                }
            }
        }
        else if (BFS(robber.GetComponent<RobberMove>().currentTile, cops[0].GetComponent<CopMove>().currentTile) > BFS(robber.GetComponent<RobberMove>().currentTile, cops[1].GetComponent<CopMove>().currentTile))
        {
            foreach (Tile c in lista)
            {

                if (BFS(c.numTile, cops[1].GetComponent<CopMove>().currentTile) > maxDis)
                {
                    maxDis = BFS(c.numTile, cops[1].GetComponent<CopMove>().currentTile);
                    there = c;
                }
            }
        }
        else
        {
            foreach (Tile c in lista)
            {
              
                if ((BFS(c.numTile, cops[0].GetComponent<CopMove>().currentTile) + BFS(c.numTile, cops[1].GetComponent<CopMove>().currentTile)) / 2 > maxDis)
                {
                    maxDis = BFS(c.numTile, cops[0].GetComponent<CopMove>().currentTile);
                    there = c;
                }
              

            }
        }
       
        robber.GetComponent<RobberMove>().MoveToTile(there);
        robber.GetComponent<RobberMove>().currentTile = there.numTile;

        Debug.Log("Dfs: " + BFS(clickedTile, cops[0].GetComponent<CopMove>().currentTile));
        Debug.Log("Dfs2: " + BFS(clickedTile, cops[1].GetComponent<CopMove>().currentTile));
        
    }

  
  
   
    int BFS(int s, int b)
    {
       Queue<int> q = new Queue<int>();
        q.Enqueue(s);
        level[s] = 0;  
        vis[s] = true;
        while (q.Count!=0)
        {
            int p = q.Dequeue();
           
           foreach(int d in tiles[p].adjacency)
            {
               // Debug.Log("llega aqui");
                if (vis[d] == false)
                {
                    
                    level[d] = level[p] + 1;
                    q.Enqueue(d);
                    vis[d] = true;
                    if (d == b)
                    {
                        int aux = level[d];
                        for(int i =0; i < vis.Length; i++)
                        {
                            vis[i] = false;
                        }
                        for (int i = 0; i < level.Length; i++)
                        {
                            level[i] = 0;
                        }
                        return aux;
                    }
                   
                   
                }
            }
        }
        return -1;
    }
   

    public void EndGame(bool end)
    {
        if(end)
            finalMessage.text = "You Win!";
        else
            finalMessage.text = "You Lose!";
        playAgainButton.interactable = true;
        state = Constants.End;
    }

    public void PlayAgain()
    {
        cops[0].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop0]);
        cops[1].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop1]);
        robber.GetComponent<RobberMove>().Restart(tiles[Constants.InitialRobber]);
                
        ResetTiles();

        playAgainButton.interactable = false;
        finalMessage.text = "";
        roundCount = 0;
        rounds.text = "Rounds: ";

        state = Constants.Restarting;
    }

    public void InitGame()
    {
        state = Constants.Init;
         
    }

    public void IncreaseRoundCount()
    {
        roundCount++;
        rounds.text = "Rounds: " + roundCount;
    }

    public void FindSelectableTiles(bool cop)
    {
       
        int indexcurrentTile;

        if (cop == true)
            indexcurrentTile = cops[clickedCop].GetComponent<CopMove>().currentTile;
        else
            indexcurrentTile = robber.GetComponent<RobberMove>().currentTile;

        //La ponemos rosa porque acabamos de hacer un reset
        tiles[indexcurrentTile].current = true;
       
        //Cola para el BFS
        Queue<Tile> nodes = new Queue<Tile>();

        //TODO: Implementar BFS. Los nodos seleccionables los ponemos como selectable=true
        //Tendrás que cambiar este código por el BFS
      

        //--------------------------------------------
         foreach (int casilla in tiles[indexcurrentTile].adjacency)
         {
             if (tiles[casilla].numTile != cops[0].GetComponent<CopMove>().currentTile && tiles[casilla].numTile != cops[1].GetComponent<CopMove>().currentTile)
             {
                 nodes.Enqueue(tiles[casilla]);
                 tiles[casilla].selectable = true;
                if (!cop)
                {
                   
                }
                 foreach (int adj in tiles[casilla].adjacency)
                 {
                     if (!nodes.Contains(tiles[adj]) && tiles[adj].numTile != cops[0].GetComponent<CopMove>().currentTile && tiles[adj].numTile != cops[1].GetComponent<CopMove>().currentTile)

                        nodes.Enqueue(tiles[adj]);
                    tiles[adj].selectable = true;
                     }

                 }
             }

        tiles[indexcurrentTile].selectable = false;
        Debug.Log( nodes.Count);
        
      
    }
   
    public void calculateDistance(Tile a, Tile b)
    {

    }
}

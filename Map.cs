using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Gamerator
{
    public class Map
    {
        // map config
        public float difficulty; // safety - value >= 1f // time(if printing all) - value <= 5f 
        public int seed;
        public int min_gridsize;
        public int max_gridsize;
        public int min_nrooms;
        public int max_nrooms;
        public int min_roomheight;
        public int max_roomheight;
        public int min_roomwidth;
        public int max_roomwidth;

        // consts that hold the costs of the map population
        public const int cost_ground = 1;
        public const int cost_box = -2;
        public const int cost_enemy = 30;
        public const int cost_spike = 15;
        public int gold_Time = 0;

        // multiplier of the manhathan distance calc
        // increasing it will result in more distant tokens
        private const int heuristicMultiplier = 100;

        // vars that will hold the start and end point generated
        private int start_i = -1, start_j = -1, end_i = -1, end_j = -1;

        // vars that will be used to aid the randomization of numbers
        // note: we can use random.range only in the main thread
        private static System.Random getrandom;
        private static readonly object syncLock = new object();

        // map tileset
        public Texture2D tileset;
        // map tileset2
        public Texture2D tileset_2;
        // tree textures
        public Texture2D treesheet;
        // map tile size
        public int tilesize;
        // random tile offset matrix
        int[,] randtile;
        // random tile max offsets
        int ground_maxrand_x = 9;
        int mountain_maxrand_x = 8;
        int wall_maxrand_x = 3;
        int box_maxrand_x = 2;
        int spike_maxrand_x = 6;
        int enemy_maxrand_id = 24;
        int start_maxrand_x = 1;
        // entity data info
        // reference to the list of chests
        public List<Chest> chests;
        // reference to the list of enemies
        public List<Enemy> enemies;
        // reference to the list of tiles
        public List<Tile> tiles;
        public int[,] spikes;
    
        // player initial position
        public Vector2 initialPosition;
        // player initial position indexes
        public Vector2 initialPosIdx;
        private Camera camera;
        private ContentManager content;

        // reference to gameController
        GameController gameController;

        // colliders
        internal List<Collider> wall_colliders;

        // items
        /// sprite framing ///
        /// foods
        public int food_idx_x = 0;
        public int food_idx_y = 0;
        public int food_off_x = 13;
        public int food_off_y = 1;
        /// keys
        public int key_idx_x = 7;
        public int key_idx_y = 10;
        public int key_off_x = 4;
        public int key_off_y = 0;
        /// potions
        public int potion_idx_x = 0;
        public int potion_idx_y = 2;
        public int potion_off_x = 13;
        public int potion_off_y = 0;
        /// shields
        public int shield_idx_x = 0;
        public int shield_idx_y = 7;
        public int shield_off_x = 13;
        public int shield_off_y = 0;
        /// weapons
        public int weapon_idx_x = 0;
        public int weapon_idx_y = 3;
        public int weapon_off_x = 9;
        public int weapon_off_y = 3;
        /// armors
        public int armor_idx_x = 0;
        public int armor_idx_y = 10;
        public int armor_off_x = 4;
        public int armor_off_y = 1;
        /// treasures
        public int treasure_idx_x = 0;
        public int treasure_idx_y = 8;
        public int treasure_off_x = 9;
        public int treasure_off_y = 0;
        /// /// /// /// /// ///

        public void Initialize(Texture2D tileset, int tilesize, List<Chest> chests, List<Enemy> enemies, List<Tile> tiles)
        {
            this.tileset = tileset;
            this.tilesize = tilesize;
            this.chests = chests;
            this.enemies = enemies;
            this.tiles = tiles;

            // initialize list of wall colliders
            wall_colliders = new List<Collider>();
        }

        private void InitMap(float level, int seed)
        {
            this.difficulty = 1f + (level * 0.01f); // safety - value >= 1f // time(if printing all) - value <= 5f 
            //clamp 
            if (this.difficulty > 5f)
                this.difficulty = 5f;

            this.seed = seed;
            min_gridsize = (int)(40 * difficulty);
            max_gridsize = (int)(50 * difficulty);
            min_roomheight = min_gridsize / 10;
            max_roomheight = max_gridsize / 5;
            min_roomwidth = min_gridsize / 10;
            max_roomwidth = max_gridsize / 5;
            min_nrooms = min_gridsize / min_roomheight;
            max_nrooms = max_gridsize / min_roomheight;
        }

        // (thread-safe) method that returns a random number
        // between a min and max parameter
        public static int GetRandomNumber(int min, int max)
        {
            lock (syncLock)
            { // synchronize
                return getrandom.Next(min, max);
            }
        }

        public struct sala
        {
            public int X;
            public int Y;
            public int altura;
            public int largura;
            public bool acessivel;
        }

        // class that defines a ground with its cost
        public class Ground
        {
            public int i;
            public int j;
            public int cost;
            public int heuristic; // will calculated as heuri(Ini->Mid)+heuri(Mid->End)

            public Ground(int i, int j, int cost, int heuristic)
            {
                this.i = i;
                this.j = j;
                this.cost = cost;
                this.heuristic = heuristic;
            }
        }

        // class that defines how the comparison between
        // Ground elements is done to order an arraylist
        public class GroundComparer : IComparer
        {

            // Calls CaseInsensitiveComparer.Compare with the parameters reversed.
            int IComparer.Compare(object x, object y)
            {
                Ground firstGround = (Ground)x;
                Ground secondGround = (Ground)y;

                int fGroundCost = (firstGround.cost * 5) + firstGround.heuristic;
                int sGroundCost = (secondGround.cost * 5) + secondGround.heuristic;

                if (fGroundCost > sGroundCost)
                    return 1;
                else if (fGroundCost < sGroundCost)
                    return -1;
                else
                    return 0;
            }

        }

        public char[,] GridGenerator()
        {
            int cols;
            int lines;
            char[,] matrix;

            cols = GetRandomNumber(min_gridsize, max_gridsize);
            lines = cols;

            matrix = new char[lines, cols];

            for (int i = 0; i < lines; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    matrix[i, j] = '_';
                }
            }

            return matrix;
        }

        public void SpawnPointGenerator(sala[] RoomVector, char[,] matrix)
        {
            int spawnRoom = 0;
            int finalRoom = 0;

            while (spawnRoom == finalRoom)
            {
                spawnRoom = GetRandomNumber(0, RoomVector.Length);
                finalRoom = GetRandomNumber(0, RoomVector.Length);
            }
            Vector2 spawnPoint = new Vector2(RoomVector[spawnRoom].X + GetRandomNumber(0, RoomVector[spawnRoom].altura), RoomVector[spawnRoom].Y + GetRandomNumber(0, RoomVector[spawnRoom].largura));
            Vector2 finalPoint = new Vector2(RoomVector[finalRoom].X + GetRandomNumber(0, RoomVector[finalRoom].altura), RoomVector[finalRoom].Y + GetRandomNumber(0, RoomVector[finalRoom].largura));
            while (spawnPoint.X == finalPoint.X && spawnPoint.Y == finalPoint.Y)
            {
                spawnPoint = new Vector2(RoomVector[spawnRoom].X + GetRandomNumber(0, RoomVector[spawnRoom].altura), RoomVector[spawnRoom].Y + GetRandomNumber(0, RoomVector[spawnRoom].largura));
                finalPoint = new Vector2(RoomVector[finalRoom].X + GetRandomNumber(0, RoomVector[finalRoom].altura), RoomVector[finalRoom].Y + GetRandomNumber(0, RoomVector[finalRoom].largura));
            }
            int s_x = (int)spawnPoint.X;
            int s_y = (int)spawnPoint.Y;
            int f_x = (int)finalPoint.X;
            int f_y = (int)finalPoint.Y;

            start_i = s_x;
            start_j = s_y;
            end_i = f_x;
            end_j = f_y;

            matrix[f_x, f_y] = 'F';
            matrix[s_x, s_y] = 'I';
        }

        public sala[] RoomVectorGenerator(char[,] matrix)
        {
            sala[] RoomVector;
            int n_salas;
            int line = matrix.GetUpperBound(0) + 1;
            int col = matrix.GetUpperBound(1) + 1;

            n_salas = GetRandomNumber(min_nrooms, max_nrooms);

            RoomVector = new sala[n_salas];

            for (int i = 0; i < n_salas; i++)
            {
                // clamp values
                if (max_roomheight >= col)
                    max_roomheight = col - min_roomheight;
                if (max_roomwidth >= line)
                    max_roomwidth = line - min_roomwidth;

                RoomVector[i].X = GetRandomNumber(2, line - (max_roomwidth + 1));
                RoomVector[i].Y = GetRandomNumber(2, col - (max_roomheight + 1));
                RoomVector[i].altura = GetRandomNumber(min_roomheight, max_roomheight);
                RoomVector[i].largura = GetRandomNumber(min_roomwidth, max_roomwidth);
            }

            return RoomVector;
        }

        public void RoomPlacer(char[,] matrix, sala[] RoomVector)
        {
            for (int i = 0; i < RoomVector.Length; i++)
            {
                for (int j = 0; j < RoomVector[i].altura; j++)
                {
                    for (int k = 0; k < RoomVector[i].largura; k++)
                    {
                        matrix[RoomVector[i].X + j, RoomVector[i].Y + k] = ' ';
                    }
                }
            }
        }

        public bool RoadGenerator(Vector2 InicialPoint, Vector2 FinalPoint, char[,] matrix)
        {
            int i = (int)InicialPoint.X;
            int j = (int)InicialPoint.Y;

            while (i != (int)FinalPoint.X || j != (int)FinalPoint.Y)
            {
                if (i != FinalPoint.X && j != FinalPoint.Y)
                {
                    if (GetRandomNumber(0, 2) == 1)
                    {
                        if (i < FinalPoint.X)
                            i++;
                        else
                            i--;
                        matrix[i, j] = ' ';
                    }
                    else
                    {
                        if (j < FinalPoint.Y)
                            j++;
                        else
                            j--;
                        matrix[i, j] = ' ';
                    }
                }
                if (i != FinalPoint.X && j == FinalPoint.Y)
                {
                    if (i < FinalPoint.X)
                        i++;
                    else
                        i--;
                    matrix[i, j] = ' ';
                }
                if (i == FinalPoint.X && j != FinalPoint.Y)
                {
                    if (j < FinalPoint.Y)
                        j++;
                    else
                        j--;
                    matrix[i, j] = ' ';
                }
            }
            return true;
        }

        public void MakeAllRoomAvaliable(char[,] matrix, sala[] RoomVector)
        {
            int randN = GetRandomNumber(0, RoomVector.Length);

            RoomVector[randN].acessivel = true;

            for (int i = 0; i < RoomVector.Length; i++)
            {
                if (!RoomVector[i].acessivel)
                {
                    int j = 0;

                    while (RoomVector[j].acessivel == false)
                        j = GetRandomNumber(0, RoomVector.Length);
                    RoomVector[i].acessivel = RoadGenerator(new Vector2(RoomVector[i].X + GetRandomNumber(0, RoomVector[i].altura), RoomVector[i].Y + GetRandomNumber(0, RoomVector[i].largura))
                                                            , new Vector2(RoomVector[j].X + GetRandomNumber(0, RoomVector[j].altura), RoomVector[j].Y + GetRandomNumber(0, RoomVector[j].largura))
                                                            , matrix);
                }
            }
        }

        public void WallGenerator(char[,] matrix, ArrayList grounds)
        {
            bool up_down_wall = false;
            bool left_right_wall = false;
            int line = matrix.GetUpperBound(0) + 1;
            int col = matrix.GetUpperBound(1) + 1;

            ///TODO - fix for condicion
            for (int i = 0; i < grounds.Count; i++)
            {
                Vector2 v = (Vector2)grounds[i];
                int x = (int)v.X;
                int y = (int)v.Y;

                if ((x + 1) <= line)
                    if (matrix[x + 1, y] == '_' || matrix[x + 1, y] == '■')
                    { matrix[x + 1, y] = '■'; up_down_wall = true; }

                if ((x - 1) >= 0)
                    if (matrix[x - 1, y] == '_' || matrix[x - 1, y] == '■')
                    { matrix[x - 1, y] = '■'; up_down_wall = true; }

                if ((y + 1) <= col)
                    if (matrix[x, y + 1] == '_' || matrix[x, y + 1] == '■')
                    { matrix[x, y + 1] = '■'; left_right_wall = true; }

                if ((y - 1) >= 0)
                    if (matrix[x, y - 1] == '_' || matrix[x, y - 1] == '■')
                    { matrix[x, y - 1] = '■'; left_right_wall = true; }

                if (up_down_wall && left_right_wall)
                {
                    if (matrix[x + 1, y + 1] == '_')
                        matrix[x + 1, y + 1] = '■';

                    if (matrix[x + 1, y - 1] == '_')
                        matrix[x + 1, y - 1] = '■';

                    if (matrix[x - 1, y + 1] == '_')
                        matrix[x - 1, y + 1] = '■';

                    if (matrix[x - 1, y - 1] == '_')
                        matrix[x - 1, y - 1] = '■';
                }
                up_down_wall = false;
                left_right_wall = false;
            }
        }

        public ArrayList RetrieveGrounds(char[,] matrix)
        {
            int line = matrix.GetUpperBound(0) + 1;
            int col = matrix.GetUpperBound(1) + 1;
            ArrayList grounds = new ArrayList();

            for (int i = 0; i < line; i++)
                for (int j = 0; j < col; j++)
                    if (matrix[i, j] == ' ') // ground
                        grounds.Add(new Vector2(i, j));

            return grounds;
        }

        // enemy and items generation

        public void MapPopulator(char[,] matrix, ArrayList grounds)
        {
            int enemyQtd = grounds.Count / 55;
            int boxQtd = (grounds.Count / 30);
            int spikeQtd = grounds.Count / 45;

            //Debug.Log("level size" + grounds.Count + " - enemys" + enemyQtd + " - box" + boxQtd + " - spikes" + spikeQtd + " - grounds" + ((boxQtd + spikeQtd + enemyQtd) - grounds.Count));

            while (enemyQtd + boxQtd + spikeQtd != 0)
            {
                int indice = GetRandomNumber(0, grounds.Count);
                Vector2 v = (Vector2)grounds[indice];
                int x = (int)v.X;
                int y = (int)v.Y;
                int value = GetRandomNumber(0, 3);

                if (value == 0 && enemyQtd > 0)
                {
                    matrix[x, y] = '☺';
                    enemyQtd--;
                }
                if (value == 1 && boxQtd > 0)
                {
                    matrix[x, y] = '□';
                    boxQtd--;
                }
                if (value == 2 && spikeQtd > 0)
                {
                    matrix[x, y] = '۩';
                    spikeQtd--;
                }

                if (matrix[x, y] != ' ')
                    grounds.RemoveAt(indice);

            }

        }

        // prints the map on a file
        public void PrintMapOnFile(char[,] grid, string path)
        {
            int line = grid.GetUpperBound(0) + 1;
            int col = grid.GetUpperBound(1) + 1;
            string bigString = "";

            for (int i = 0; i < line; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    bigString += grid[i, j] + " ";
                }
                bigString += "\n";
            }

            File.WriteAllText(path, bigString);
        }

        // method that calculates costs of the ground.
        // calculation takes account of NxN square(nSquare*2+1 x nSquare*2+1:
        // X X X X X
        // X X X X X
        // X X G X X
        // X X X X X
        // X X X X X
        //|---|
        //nSquare 
        // size
        /// <summary>
        /// Calculates the costs of the grounds
        /// </summary>
        /// <returns>An ArrayList of Grounds with all costs calculated.</returns>
        /// <param name="map">The matrix that contains the map data</param> 
        /// <param name="grounds">ArrayList of vector2 grounds.</param>
        /// <param name="nSquare">The size of the square, from center to frontier
        /// to calculate the ground cost. Square 5x5 would have nSquare 2</param>
        public ArrayList GroundCostCalculator(char[,] map, ArrayList grounds, int nSquare)
        {
            // arraylist that will store all calculated cost grounds
            ArrayList calculatedGrounds = new ArrayList();

            // iterators
            int i, j;

            // iterates the arraylist that contains only grounds
            for (int k = 0; k < grounds.Count; k++)
            {
                // gets the i coord of the ground
                int v_i = (int)((Vector2)grounds[k]).X;
                // gets the j coord of te ground
                int v_j = (int)((Vector2)grounds[k]).Y;
                // gets the n of lines in the map grid
                int lines = map.GetUpperBound(0) + 1;
                // gets then n of cols in the map grid
                int cols = map.GetUpperBound(1) + 1;

                // caculates the initial and final pos
                // of the matrix taken account for the cost
                int ini_i = (v_i - nSquare);
                int fim_i = (v_i + nSquare);
                int ini_j = (v_j - nSquare);
                int fim_j = (v_j + nSquare);

                // 'normalize' the initial and final pos
                // within the bounds of the map matrix
                if (ini_i < 0) ini_i = 0;
                if (fim_i >= lines) fim_i = lines;
                if (ini_j < 0) ini_j = 0;
                if (fim_j >= cols) fim_j = cols;

                // var that will hold the current calculated cost
                int cost = 0;

                // iterates in the sub-matrix formed with the 
                // initial and final pos calculated
                for (i = ini_i; i <= fim_i; i++)
                {
                    for (j = ini_j; j <= fim_j; j++)
                    {
                        // the ground that cost is being calculated should be ignored?
                        //Debug.Log("i: "+i+" | lines: "+lines);
                        //Debug.Log("j: "+j+" | cols: "+cols);
                        // current cell of the map
                        char cell = map[i, j];
                        //print(cell + "indeice x" + (i+1) + "indice y" + (j+1));

                        // switch of cell types
                        switch (cell)
                        {
                            // ground
                            case ' ':
                                cost += cost_ground;
                                //print("Ground, cost = " + cost);
                                break;
                            // box
                            case '□':
                                cost += cost_box;
                                //print("Box, cost = " + cost);
                                break;
                            // enemy
                            case '☺':
                                cost += cost_enemy;
                                //print("Enemy, cost = " + cost);
                                break;
                            // spike
                            case '۩':
                                cost += cost_spike;
                                //print("Spike, cost = " + cost);
                                break;
                            // wall & blank - ignored
                            default:
                                break;
                        }
                    }
                }

                // now we have the cost of the ground calculated!
                // to aid the tokens positioning, we'll be calculating
                // the heuristic(man.distance) of each ground
                // but only if field is possible for token positioning
                // and we're going to store these grounds in the return data

                int heuristic = -1;

                if (map[v_i, v_j] == ' ')
                {
                    heuristic = (int)(Math.Sqrt((manhattanDistance(start_i, start_j, v_i, v_j) *
                        manhattanDistance(v_i, v_j, end_i, end_j))));
                    heuristic *= heuristicMultiplier;

                    // stores in the proper data structure
                    calculatedGrounds.Add(new Ground(v_i, v_j, cost, heuristic));
                }

                // for debug
                /*if(cost > nSquare * 35)
                    map[v_i,v_j] = 'H';
                else if(cost > nSquare * 10)
                    map[v_i,v_j] = 'M';
                else
                    map[v_i,v_j] = 'L';*/

            }
            return calculatedGrounds;
        }

        // populates the map with tokens taking into account 
        // positioning costs and token types
        private void TokenPopulator(char[,] map, ArrayList grounds)
        {
            int size = grounds.Count - 1;
            grounds.Sort(new GroundComparer());
            int silver = (int)Math.Floor(size / 1.45), bronze = (int)Math.Floor(size / 4.35);

            map[((Ground)grounds[size]).i, ((Ground)grounds[size]).j] = 'G';
            map[((Ground)grounds[silver]).i, ((Ground)grounds[silver]).j] = 'S';
            map[((Ground)grounds[bronze]).i, ((Ground)grounds[bronze]).j] = 'B';

            //debug
            int b = ((Ground)grounds[bronze]).cost + ((Ground)grounds[bronze]).heuristic;
            int s = ((Ground)grounds[silver]).cost + ((Ground)grounds[silver]).heuristic;
            int g = ((Ground)grounds[size]).cost + ((Ground)grounds[size]).heuristic;
            //Debug.Log("G: " + g + " | S: " + s + " | B: " + b);
            gold_Time = g;
        }

        // heuristic used to position tokens along the map
        // depending on the cost and the type of token
        private int manhattanDistance(int x1, int y1, int x2, int y2)
        {
            return (int)(Math.Abs(x2 - x1) + Math.Abs(y2 - y1));
        }

        private int FindGround(ArrayList grounds, Vector2 indice)
        {
            int indice_x = (int)indice.X;
            int indice_y = (int)indice.Y;

            for (int i = 0; i < grounds.Count; i++)
            {
                if (((Ground)grounds[i]).i == indice_x && ((Ground)grounds[i]).j == indice_y)
                {
                    return i;
                }
            }
            return -1;
        }

        public char[,] GenerateMap(float difficulty, int seed)
        {
            char[,] Grid;
            sala[] RoomVector;

            InitMap(difficulty, seed);

            getrandom = new System.Random(seed);
           
            Grid = GridGenerator();
            RoomVector = RoomVectorGenerator(Grid);
            RoomPlacer(Grid, RoomVector);
            MakeAllRoomAvaliable(Grid, RoomVector);
            ArrayList grounds = RetrieveGrounds(Grid);
            WallGenerator(Grid, grounds);
            MapPopulator(Grid, grounds);
            SpawnPointGenerator(RoomVector, Grid);
            ArrayList tokenGrounds = GroundCostCalculator(Grid, grounds, 2);
            TokenPopulator(Grid, tokenGrounds);
            
            // debug
            string path = "Map.txt";
            PrintMapOnFile(Grid, path);

            return Grid;
        }

        /// <summary>
        /// Initial configuration from generated grid
        /// includes colliders subscription
        /// </summary>
        /// <param name="map"></param>
        public void ConfigureTiles(char[,] map)
        {
            int line = map.GetUpperBound(0) + 1;
            int col = map.GetUpperBound(1) + 1;

            randtile = new int[line, col];
            spikes = new int[line, col];

            for (int i = 0; i < col; i++)
            {
                for (int j = 0; j < line; j++)
                {
                    switch (map[i, j])
                    {
                        // ground
                        case ' ':
                            randtile[i, j] = GetRandomNumber(0, ground_maxrand_x);
                            break;
                        // box
                        case '□':
                            randtile[i, j] = GetRandomNumber(0, box_maxrand_x);
                           // boxes[i, j] = GetRandomNumber(1, box_max_hits);
                            break;
                        // spike
                        case '۩':
                            randtile[i, j] = GetRandomNumber(0, spike_maxrand_x);
                            spikes[i, j] = randtile[i, j] + 1;
                            break;
                        // initial pos of the player
                        case 'I':
                            randtile[i, j] = GetRandomNumber(0, start_maxrand_x);
                            initialPosIdx = new Vector2(j, i);
                            break;
                        // blank - unused fields
                        case '_':
                            randtile[i, j] = GetRandomNumber(0, mountain_maxrand_x);
                            break;
                        default:
                            randtile[i, j] = GetRandomNumber(0, 1);
                            break;
                    }
                }
            }
        }

        public Vector2 GetPointFromIJ(int i, int j)
        {
            /*** camera framing ****/
            float tilezoomed = camera.zoom * tilesize;
            int startCol = (int)(Math.Floor(camera.x / tilezoomed));
            int endCol = (int)(Math.Floor(startCol + (camera.width / tilezoomed)));
            int startRow = (int)(Math.Floor(camera.y / tilezoomed));
            int endRow = (int)(Math.Floor(startRow + (camera.height / tilezoomed)));
            Double offsetX = (-camera.x + startCol * tilezoomed);
            Double offsetY = (-camera.y + startRow * tilezoomed);

            var target_x = (j - (Math.Floor(camera.x / tilezoomed))) * tilezoomed + offsetX;
            var target_y = (i - (Math.Floor(camera.y / tilezoomed))) * tilezoomed + offsetY;

            return new Vector2((float)Math.Round(target_x), (float)Math.Round(target_y));
        }

        public void SetInitialConfig(char[,] map, Camera camera, ContentManager content, GameController gameController)
        {
            // applies the camera zoom to the tiles
            int tilezoomed = (int)(tilesize * camera.zoom);

            this.camera = camera;
            this.content = content;
            this.gameController = gameController;

            // set textures
            tileset_2 = content.Load<Texture2D>("tileset");
            treesheet = content.Load<Texture2D>("tree");

            // Initial configuration from generated map
            ConfigureTiles(map);

            // Initial Player Position Point
            initialPosition.X = (initialPosIdx.X * tilezoomed) + tilezoomed/2 - 1;
            initialPosition.Y = (initialPosIdx.Y * tilezoomed) + tilezoomed / 1.5f + 1 ;
        }

        public void InitialDraw(SpriteBatch spriteBatch, char[,] map, Camera camera, float global_light, GameController gameController)
        {
            int line = map.GetUpperBound(0) + 1;
            int col = map.GetUpperBound(1) + 1;

            Rectangle source;
            Vector2 destination;

            // applies the camera zoom to the tiles
            float tilezoomed = tilesize * camera.zoom;

            /**grounds - egiptian 1*/
            // var ground_ind_x = 48;
            // var ground_ind_y = 16;
            // var ground_rand_x = 7
            /***********************/
            /**grounds - egiptian 2*/
            int ground_ind_x = 12;
            int ground_ind_y = 17;
            /***********************/
            /** mountains- egiptian*/
            int mountain_ind_x = 43;
            int mountain_ind_y = 17;
            /***********************/
            /** walls - stone egpt**/
            int wall_ind_x = 39;
            int wall_ind_y = 17;
            /***********************/
            /**boxes - chests ******/
            int box_ind_x = 43;
            int box_ind_y = 45;
            int box_offset_y = -2;
            /***********************/
            /**spikes - hole&spear**/
            int spike_ind_x = 32;
            int spike_ind_y = 11;
            /***********************/
            /**enemies - group 1 ***/
            // var enemy_ind_x = 20;
            // var enemy_ind_y = 4;
            // var enemy_rand_x = 41;
            // var enemy_rand_y = 1;
            /***********************/
            /**enemies - group 2 ***/
            int enemy_ind_x = 8;
            int enemy_ind_y = 44;
            /***********************/
            /**start - doors 1 *****/
            int start_ind_x = 32;
            int start_ind_y = 15;
            /***********************/
            /**final - totem 1 *****/
            int final_ind_x = 35;
            int final_ind_y = 7;
            /***********************/
            /** bronze - pendant ***/
            int bronze_ind_x = 37;
            int bronze_ind_y = 20;
            /***********************/
            /** silver - pendant ***/
            int silver_ind_x = 43;
            int silver_ind_y = 20;
            /***********************/
            /** gold - pendant ***/
            int gold_ind_x = 46;
            int gold_ind_y = 20;
            /***********************/

            /*** camera framing ****/
            int startCol = (int)(Math.Floor(camera.x / tilezoomed));
            int endCol = (int)(Math.Floor(startCol + (camera.width / tilezoomed)));
            int startRow = (int)(Math.Floor(camera.y / tilezoomed));
            int endRow = (int)(Math.Floor(startRow + (camera.height / tilezoomed)));
            Double offsetX = -camera.x + startCol * tilezoomed;
            Double offsetY = -camera.y + startRow * tilezoomed;

            // clamp max values
            if (endRow + 1 > col-1)
                endRow = col-2;
            if (endCol + 1 > line-1)
                endCol = line-2;

            // tile generic class instance
            Tile tile;
            // random offset for aleatory tile
            int random_tile;

            // array of string for names generating
            string[] chest_names = { "CHEST, BOX, TREASURY, CARTON, BASKET, CASE, WOODY, TOOL, container, pack, receptacle, trunk" };

            string[] enemy_names = { "Abysmal Knight, Alarm, Alicel, Anubis, Argos, Baby Hatii, Baby Desert Wolf, Bloody Knight, "
                                     +"Baphomet, Bongun, Coco, Dark Lord, Poring, Golem, Goblin, Gargoyle, Garm, Gryphon, Myst, " 
                                     + "Orc, Rotworm, Succubus, Incubus, Viola, Skeleton, Demon, Amon, Amarth, Minorous, Minotaur, "
                                     + "Maya, Matyr, Nine Tail, Peco, Jiraya, Khalitzburg, Kobold, Joker, Dokebi, Deviling, Drake, "
                                     + "Eddga, Willow, Hydra, Mimic, Megalodon, Nacht Sieger, Sting, Archer, Dragon, Dragon Lord"};

            // name generators
            NameGenerator chest_name_gen = new NameGenerator(chest_names, 0, 3, seed);
            NameGenerator enemy_name_gen = new NameGenerator(enemy_names, 0, 3, seed);

            // Console.WriteLine(camera.x + "/" + tilezoomed + ",");
            /// loop through matrix map
            for (int i = 0; i < col; i++)
            {
                for (int j = 0; j < line; j++)
                {
                    var target_x = (j - (Math.Floor(camera.x / tilezoomed))) * tilezoomed + offsetX;
                    var target_y = (i - (Math.Floor(camera.y / tilezoomed))) * tilezoomed + offsetY;
                    
                    destination = new Vector2((float)Math.Round(target_x), (float)Math.Round(target_y));

                    switch (map[i, j])
                    {
                        // wall
                        case '■':
                            // create tile instance of a random ground
                            destination.X += camera.x;
                            destination.Y += camera.y;
                            // create ground
                            random_tile = GetRandomNumber(0, ground_maxrand_x);
                            tile = new Tile();
                            tile.Initialize(Tile.Type.Ground, 0, 0, 0, 0, 0,
                                            false, false, 0f, destination, 32, camera, map, content, gameController);
                            tiles.Add(tile);
                            // create wall
                            random_tile = GetRandomNumber(0, wall_maxrand_x);
                            tile = new Tile();

                            tile.Initialize(Tile.Type.Wall, random_tile, 0, 0,0,0,
                                            true, true, 0.1f, destination, 40, camera, map, content, gameController);
                            tiles.Add(tile);
                            break;
                        // ground
                        case ' ':
                            random_tile = GetRandomNumber(0, ground_maxrand_x);
                            tile = new Tile();
                            destination.X += camera.x;
                            destination.Y += camera.y;
                            tile.Initialize(Tile.Type.Ground, 0, 0, 0, 0, 0,
                                            false, false, 0f, destination, 32, camera, map, content, gameController);
                            tiles.Add(tile);
                            break;
                            // box
                        case '□':
                            destination.X += camera.x;
                            destination.Y += camera.y;
                            // create tile instance of a random ground
                            random_tile = GetRandomNumber(0, ground_maxrand_x);
                            tile = new Tile();
                            tile.Initialize(Tile.Type.Ground, 0, 0, 0, 0, 0,
                                            false, false, 0f, destination, 32, camera, map, content, gameController);
                            tiles.Add(tile);
                            // create a chest instance of random id
                            int random_chest = GetRandomNumber(0, box_maxrand_x);
                            int hitpoints = GetRandomNumber(5, gameController.max_chest_hits);
                            Chest chest = new Chest();
                            chest.Initialize(random_chest, chest_name_gen.NextName, hitpoints, generateItem(destination, camera, content, gameController), 
                                             destination, tilesize, camera, map, content, gameController);
                            chests.Add(chest);
                            break;
                        // enemy (moving entity, enemy class will control the draw)
                        case '☺':
                            destination.X += camera.x;
                            destination.Y += camera.y;
                            // create tile instance of a random ground
                            random_tile = GetRandomNumber(0, ground_maxrand_x);
                            tile = new Tile();
                            tile.Initialize(Tile.Type.Ground, 0, 0, 0, 0, 0,
                                            false, false, 0f, destination, 32, camera, map, content, gameController);
                            tiles.Add(tile);
                            // create enemy instance of random id
                            int random_enemy = GetRandomNumber(0, enemy_maxrand_id);
                            Enemy enemy = new Enemy();
                            int health = GetRandomNumber(5, gameController.max_chest_hits);
                            float[] personality = BuildPersonality();
                            enemy.Initialize(random_enemy, enemy_name_gen.NextName, personality, health, destination, tilesize, camera, map, content, gameController);
                            enemies.Add(enemy);
                            break;
                        // spike
                        case '۩':
                            destination.X += camera.x;
                            destination.Y += camera.y;
                            // create tile instance of a random ground
                            random_tile = GetRandomNumber(0, ground_maxrand_x);
                            tile = new Tile();
                            tile.Initialize(Tile.Type.Ground, 0, 0, 0, 0, 0,
                                            false, false, 0f, destination, 32, camera, map, content, gameController);
                            tiles.Add(tile);
                            // create spike
                            random_tile = GetRandomNumber(0, spike_maxrand_x);
                            tile = new Tile();
                            destination.X += 3f * camera.zoom;
                            tile.Initialize(Tile.Type.Spike, random_tile, spike_ind_x, spike_ind_y, 0, 0,
                                            false, false, 0.01f, destination, 32, camera, map, content, gameController);
                            tiles.Add(tile);
                            break;
                        // golden|silver|bronze token
                          case 'G':
                            destination.X += camera.x;
                            destination.Y += camera.y;
                            // create tile instance of a random ground
                            random_tile = GetRandomNumber(0, ground_maxrand_x);
                            tile = new Tile();
                            tile.Initialize(Tile.Type.Ground, 0, 0, 0, 0, 0,
                                            false, false, 0f, destination, 32, camera, map, content, gameController);
                            tiles.Add(tile);
                            // create token
                            Token gold = new Token();
                            gold.Initialize(Token.TokenType.Gold, gold_ind_x, gold_ind_y, 0, 0, destination, tilesize, camera, content, gameController);
                            gameController.tokens.Add(gold);
                            break;
                        case 'S':
                            destination.X += camera.x;
                            destination.Y += camera.y;
                            // create tile instance of a random ground
                            random_tile = GetRandomNumber(0, ground_maxrand_x);
                            tile = new Tile();
                            tile.Initialize(Tile.Type.Ground, 0, 0, 0, 0, 0,
                                            false, false, 0f, destination, 32, camera, map, content, gameController);
                            tiles.Add(tile);
                            // create token
                            Token silver = new Token();
                            silver.Initialize(Token.TokenType.Silver, silver_ind_x, silver_ind_y, 0, 0, destination, tilesize, camera, content, gameController);
                            gameController.tokens.Add(silver);
                            break;
                        case 'B':
                            destination.X += camera.x;
                            destination.Y += camera.y;
                            // create tile instance of a random ground
                            random_tile = GetRandomNumber(0, ground_maxrand_x);
                            tile = new Tile();
                            tile.Initialize(Tile.Type.Ground, 0, 0, 0, 0, 0,
                                            false, false, 0f, destination, 32, camera, map, content, gameController);
                            tiles.Add(tile);
                            // create token
                            Token bronze = new Token();
                            bronze.Initialize(Token.TokenType.Bronze, bronze_ind_x, bronze_ind_y, 0, 0, destination, tilesize, camera, content, gameController);
                            gameController.tokens.Add(bronze);
                            break;
                        // initial pos of the player
                        case 'I':
                            destination.X += camera.x;
                            destination.Y += camera.y;

                            // create tile instance of a random ground
                            random_tile = GetRandomNumber(0, ground_maxrand_x);
                            tile = new Tile();
                            tile.Initialize(Tile.Type.Ground, 0, 0, 0, 0, 0,
                                            false, false, 0f, destination, 32, camera, map, content, gameController);
                            tiles.Add(tile);

                            // create tile instance of a random start
                            random_tile = GetRandomNumber(0, start_maxrand_x);
                            tile = new Tile();
                            tile.Initialize(Tile.Type.Start, random_tile, start_ind_x, start_ind_y, 0, 0,
                                            false, true, 0.1f, destination, tilesize, camera, map, content, gameController);
                            tiles.Add(tile);

                            // initial player pos index
                            initialPosIdx = new Vector2(j, i);
                            break;
                        // totem
                        case 'F':
                            destination.X += camera.x;
                            destination.Y += camera.y;
                            // create tile instance of a random ground
                            random_tile = GetRandomNumber(0, ground_maxrand_x);
                            tile = new Tile();
                            tile.Initialize(Tile.Type.Ground, 0, 0, 0, 0, 0,
                                            false, false, 0f, destination, 32, camera, map, content, gameController);
                            tiles.Add(tile);
                            // create totem
                            Totem totem = new Totem();
                            totem.Initialize(final_ind_x, final_ind_y, 0, 0, destination, tilesize, camera, content, gameController);
                            gameController.totems.Add(totem);
                            break;
                        // blank - unused fields
                        case '_':
                            destination.X += camera.x;
                            destination.Y += camera.y;

                            // create ground
                            random_tile = GetRandomNumber(0, ground_maxrand_x);
                            tile = new Tile();
                            tile.Initialize(Tile.Type.Ground, 0, 0, 0, 0, 0,
                                            false, false, 0f, destination, 32, camera, map, content, gameController);
                            tiles.Add(tile);
                            // create wall
                            random_tile = GetRandomNumber(0, wall_maxrand_x);
                            tile = new Tile();

                            tile.Initialize(Tile.Type.Wall, random_tile, 0, 0, 0, 0,
                                            false, true, 0.1f, destination, 40, camera, map, content, gameController);
                            tiles.Add(tile);
                            break;
                        default:
                            // create tile instance of a random ground
                            random_tile = GetRandomNumber(0, ground_maxrand_x);
                            tile = new Tile();
                            destination.X += camera.x;
                            destination.Y += camera.y;
                            tile.Initialize(Tile.Type.Ground, 0, 0, 0, 0, 0,
                                            false, false, 0f, destination, 32, camera, map, content, gameController);
                            tiles.Add(tile);
                            break;
                    }
                }
            }

        }

        // generates enemy personality (random personality)
        private float[] BuildPersonality()
        {
            float[] personality = new float[5];
            int precision_factor = 1000; // 10, 100, 1000...

            for (int i = 0; i < personality.Length; i++)
                personality[i] = GetRandomNumber(0, precision_factor) / (float)precision_factor;

            return personality;
        }

        // generates a random item for chests
        private Item generateItem(Vector2 position, Camera camera, ContentManager content, GameController gameController)
        {
            Item item = new Item();

            int lucky;
            int variation_x, variation_y, x, y, item_id;

            // tries to generate from most rare to least rare
            lucky = gameController.random_noseed.Next(0, 100);
            if (lucky < gameController.chest_treasure_chance)
            {
                // calculates item id (14 equals to number of sprites per line in spritesheet)
                item_id = CalculateItemID(treasure_idx_x, treasure_idx_y, treasure_off_x, treasure_off_y);
                // generate item
                item.Initialize(item_id, Item.ItemType.Treasure, false, position, tilesize, camera, content, gameController);
                return item;
            }
            lucky = gameController.random_noseed.Next(0, 100);
            if (lucky < gameController.chest_armor_chance)
            {
                // calculates item id (14 equals to number of sprites per line in spritesheet)
                item_id = CalculateItemID(armor_idx_x, armor_idx_y, armor_off_x, armor_off_y);
                // generate item
                item.Initialize(item_id, Item.ItemType.Armor, false, position, tilesize, camera, content, gameController);
                return item;
            }
            lucky = gameController.random_noseed.Next(0, 100);
            if (lucky < gameController.chest_shield_chance)
            {
                // calculates item id (14 equals to number of sprites per line in spritesheet)
                item_id = CalculateItemID(shield_idx_x, shield_idx_y, shield_off_x, shield_off_y);
                // generate item
                item.Initialize(item_id, Item.ItemType.Shield, false, position, tilesize, camera, content, gameController);
                return item;
            }
            lucky = gameController.random_noseed.Next(0, 100);
            if (lucky < gameController.chest_weapon_chance)
            {
                // calculates item id (14 equals to number of sprites per line in spritesheet)
                item_id = CalculateItemID(weapon_idx_x, weapon_idx_y, weapon_off_x, weapon_off_y);
                // generate item
                item.Initialize(item_id, Item.ItemType.Weapon, false, position, tilesize, camera, content, gameController);
                return item;
            }
            lucky = gameController.random_noseed.Next(0, 100);
            if (lucky < gameController.chest_potion_chance)
            {
                // calculates item id (14 equals to number of sprites per line in spritesheet)
                item_id = CalculateItemID(potion_idx_x, potion_idx_y, potion_off_x, potion_off_y);
                // generate item
                item.Initialize(item_id, Item.ItemType.Potion, false, position, tilesize, camera, content, gameController);
                return item;
            }
            lucky = gameController.random_noseed.Next(0, 100);
            if (lucky < gameController.chest_key_chance)
            {
                // calculates item id (14 equals to number of sprites per line in spritesheet)
                item_id = CalculateItemID(key_idx_x, key_idx_y, key_off_x, key_off_y);
                // generate item
                item.Initialize(item_id, Item.ItemType.Key, false, position, tilesize, camera, content, gameController);
                return item;
            }
            lucky = gameController.random_noseed.Next(0, 100);
            if (lucky < gameController.chest_food_chance)
            {
                // calculates item id (14 equals to number of sprites per line in spritesheet)
                item_id = CalculateItemID(food_idx_x, food_idx_y, food_off_x, food_off_y);
                // generate item
                item.Initialize(item_id, Item.ItemType.Food, false, position, tilesize, camera, content, gameController);
                return item;
            }

            // bad lucky, no item
            item.Initialize(-1, Item.ItemType.None, false, position, tilesize, camera, content, gameController);
            return item;         
        }

        public int CalculateItemID(int idx_x, int idx_y, int off_x, int off_y)
        {
            // randomize
            int variation_x = gameController.random_noseed.Next(0, off_x);
            int variation_y = gameController.random_noseed.Next(0, off_y);
            // get indexes with randomization
            int x = idx_x + variation_x;
            int y = idx_y + variation_y;
            // return item id calculated
            return (x + (14 * y));
        }
    }
}

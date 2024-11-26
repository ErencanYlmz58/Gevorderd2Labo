using System;
using System.Collections.Generic;

namespace ConsoleAppSquareMaster
{
    public class WorldConquer
    {
        /* world indicates whether the grid cell on coordinate x,y is part of the world or not */
        private bool[,] world;
        /* the values in worldempires are -1 if not part of the world, 0 if part of the world but not conquered by any empire, any other positive value indicates the empire (id) the grid cell belongs to */
        private int[,] worldempires;
        private int maxx, maxy;
        private Random random = new Random(1);

        public WorldConquer(bool[,] world)
        {
            this.world = world;
            maxx = world.GetLength(0);
            maxy = world.GetLength(1);
            worldempires = new int[maxx, maxy];
            for (int i = 0; i < maxx; i++)
            {
                for (int j = 0; j < maxy; j++)
                {
                    if (world[i, j]) worldempires[i, j] = 0; // Beschikbaar voor verovering
                    else worldempires[i, j] = -1; // Geen deel van de wereld
                }
            }
        }

        /*
         * Conquer method that allows different empires to use different algorithms
         * empireAlgorithmMapping: a dictionary mapping each empire to an algorithm (1, 2, or 3)
         * turns: number of turns to run the conquest
         */
        public int[,] ConquerWithDifferentAlgorithms(Dictionary<int, int> empireAlgorithmMapping, int turns)
        {
            // Key is the empire id, value is the list of cells (x,y) the empire controls
            Dictionary<int, List<(int, int)>> empires = new();

            // Search random start positions for each empire
            // Start positions must be located on the world and each empire requires a different start position
            int x, y;
            foreach (var empire in empireAlgorithmMapping.Keys)
            {
                bool ok = false;
                while (!ok)
                {
                    x = random.Next(maxx);
                    y = random.Next(maxy);
                    if (world[x, y] && worldempires[x, y] == 0)
                    {
                        ok = true;
                        worldempires[x, y] = empire;
                        empires.Add(empire, new List<(int, int)>() { (x, y) });
                    }
                }
            }

            // Perform the conquest for the specified number of turns
            for (int i = 0; i < turns; i++)
            {
                foreach (var empire in empires.Keys)
                {
                    int algorithm = empireAlgorithmMapping[empire];
                    switch (algorithm)
                    {
                        case 1:
                            ExecuteConquer1(empire, empires);
                            break;
                        case 2:
                            ExecuteConquer2(empire, empires);
                            break;
                        case 3:
                            ExecuteConquer3(empire, empires);
                            break;
                        default:
                            Console.WriteLine($"Onbekend algoritme voor empire {empire}");
                            break;
                    }
                }
            }

            // Generate images for each algorithm
            BitmapWriter bmw = new BitmapWriter();
            foreach (var empire in empireAlgorithmMapping.Keys)
            {
                int algorithm = empireAlgorithmMapping[empire];
                bmw.DrawWorld(worldempires, $"Conquer{algorithm}_FinalResult");
            }

            return worldempires;
        }

        /*
         * ExecuteConquer1: Empire expands by selecting a random location and expanding to a random adjacent free location
         */
        private void ExecuteConquer1(int empire, Dictionary<int, List<(int, int)>> empires)
        {
            if (empires[empire].Count == 0) return;

            int index = random.Next(empires[empire].Count);
            int direction = random.Next(4);
            int x = empires[empire][index].Item1;
            int y = empires[empire][index].Item2;

            switch (direction)
            {
                case 0:
                    if (x < maxx - 1 && worldempires[x + 1, y] == 0)
                    {
                        worldempires[x + 1, y] = empire;
                        empires[empire].Add((x + 1, y));
                    }
                    break;
                case 1:
                    if (x > 0 && worldempires[x - 1, y] == 0)
                    {
                        worldempires[x - 1, y] = empire;
                        empires[empire].Add((x - 1, y));
                    }
                    break;
                case 2:
                    if (y < maxy - 1 && worldempires[x, y + 1] == 0)
                    {
                        worldempires[x, y + 1] = empire;
                        empires[empire].Add((x, y + 1));
                    }
                    break;
                case 3:
                    if (y > 0 && worldempires[x, y - 1] == 0)
                    {
                        worldempires[x, y - 1] = empire;
                        empires[empire].Add((x, y - 1));
                    }
                    break;
            }
        }

        /*
         * ExecuteConquer2: Empire expands by finding the location with the most free adjacent cells
         */
        private void ExecuteConquer2(int empire, Dictionary<int, List<(int, int)>> empires)
        {
            if (empires[empire].Count == 0) return;

            int index = FindWithMostEmptyNeighbours(empires[empire]);
            int direction = random.Next(4);
            int x = empires[empire][index].Item1;
            int y = empires[empire][index].Item2;

            switch (direction)
            {
                case 0:
                    if (x < maxx - 1 && worldempires[x + 1, y] == 0)
                    {
                        worldempires[x + 1, y] = empire;
                        empires[empire].Add((x + 1, y));
                    }
                    break;
                case 1:
                    if (x > 0 && worldempires[x - 1, y] == 0)
                    {
                        worldempires[x - 1, y] = empire;
                        empires[empire].Add((x - 1, y));
                    }
                    break;
                case 2:
                    if (y < maxy - 1 && worldempires[x, y + 1] == 0)
                    {
                        worldempires[x, y + 1] = empire;
                        empires[empire].Add((x, y + 1));
                    }
                    break;
                case 3:
                    if (y > 0 && worldempires[x, y - 1] == 0)
                    {
                        worldempires[x, y - 1] = empire;
                        empires[empire].Add((x, y - 1));
                    }
                    break;
            }
        }

        /*
         * ExecuteConquer3: Empire expands by picking a random location and searching for an empty adjacent cell
         */
        private void ExecuteConquer3(int empire, Dictionary<int, List<(int, int)>> empires)
        {
            if (empires[empire].Count == 0) return;

            int index = random.Next(empires[empire].Count);
            PickEmpty(empires[empire], index, empire);
        }

        // Toegevoegde methode om de grootte en het percentage van elk empire te berekenen
        public Dictionary<int, (int size, double percentage)> CalculateEmpireSizes()
        {
            Dictionary<int, int> empireSizes = new Dictionary<int, int>();
            int totalWorldSize = 0;

            for (int x = 0; x < maxx; x++)
            {
                for (int y = 0; y < maxy; y++)
                {
                    if (world[x, y])
                    {
                        totalWorldSize++;
                        int empireId = worldempires[x, y];
                        if (empireId > 0) // Enkel tellen als het veroverd is door een empire
                        {
                            if (!empireSizes.ContainsKey(empireId))
                            {
                                empireSizes[empireId] = 0;
                            }
                            empireSizes[empireId]++;
                        }
                    }
                }
            }

            // Omzetten naar een dictionary die ook het percentage bevat
            Dictionary<int, (int size, double percentage)> result = new Dictionary<int, (int size, double percentage)>();
            foreach (var entry in empireSizes)
            {
                int empireId = entry.Key;
                int size = entry.Value;
                double percentage = (double)size / totalWorldSize * 100;
                result[empireId] = (size, percentage);
            }

            return result;
        }

        /* Helper method to find the cell with the most empty neighbours */
        private int FindWithMostEmptyNeighbours(List<(int, int)> empire)
        {
            List<int> indexes = new List<int>();
            int maxEmpty = 0;
            int calcEmpty;
            for (int i = 0; i < empire.Count; i++)
            {
                calcEmpty = EmptyNeighbours(empire[i].Item1, empire[i].Item2);
                if (calcEmpty >= maxEmpty)
                {
                    if (calcEmpty > maxEmpty)
                    {
                        indexes.Clear();
                        maxEmpty = calcEmpty;
                    }
                    indexes.Add(i);
                }
            }
            return indexes[random.Next(indexes.Count)];
        }

        /* Helper method to find and add an empty adjacent cell */
        private void PickEmpty(List<(int, int)> empire, int index, int e)
        {
            List<(int, int)> neighbours = new List<(int, int)>();
            if (IsValidPosition(empire[index].Item1 - 1, empire[index].Item2) && worldempires[empire[index].Item1 - 1, empire[index].Item2] == 0)
                neighbours.Add((empire[index].Item1 - 1, empire[index].Item2));
            if (IsValidPosition(empire[index].Item1 + 1, empire[index].Item2) && worldempires[empire[index].Item1 + 1, empire[index].Item2] == 0)
                neighbours.Add((empire[index].Item1 + 1, empire[index].Item2));
            if (IsValidPosition(empire[index].Item1, empire[index].Item2 - 1) && worldempires[empire[index].Item1, empire[index].Item2 - 1] == 0)
                neighbours.Add((empire[index].Item1, empire[index].Item2 - 1));
            if (IsValidPosition(empire[index].Item1, empire[index].Item2 + 1) && worldempires[empire[index].Item1, empire[index].Item2 + 1] == 0)
                neighbours.Add((empire[index].Item1, empire[index].Item2 + 1));

            if (neighbours.Count > 0)
            {
                var chosen = neighbours[random.Next(neighbours.Count)];
                empire.Add(chosen);
                worldempires[chosen.Item1, chosen.Item2] = e;
            }
        }

        /* Helper method to count empty neighbours */
        private int EmptyNeighbours(int x, int y)
        {
            int emptyCount = 0;
            if (IsValidPosition(x - 1, y) && worldempires[x - 1, y] == 0) emptyCount++;
            if (IsValidPosition(x + 1, y) && worldempires[x + 1, y] == 0) emptyCount++;
            if (IsValidPosition(x, y - 1) && worldempires[x, y - 1] == 0) emptyCount++;
            if (IsValidPosition(x, y + 1) && worldempires[x, y + 1] == 0) emptyCount++;
            return emptyCount;
        }

        /* Helper method to check if the position is valid */
        private bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < maxx && y >= 0 && y < maxy;
        }

        public int[,] GetWorldEmpires()
        {
            return worldempires;
        }
    }
}

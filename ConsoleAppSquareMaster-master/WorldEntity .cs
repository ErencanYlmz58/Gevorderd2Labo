using System;

namespace ConsoleAppSquareMaster
{
    public class WorldEntity
    {
        public string Naam { get; set; }
        public string AlgoritmeType { get; set; }
        public int MaxX { get; set; }
        public int MaxY { get; set; }
        public double Coverage { get; set; }  // Dekkingsgraad tussen 0 en 1 (bijv. 0.6 betekent 60%)
    }
}

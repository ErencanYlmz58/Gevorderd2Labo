using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppSquareMaster
{
    public class World
    {
        private Random random = new Random(10);
        private int maxRandom = 10;
        private int chanceExtra = 6;
        private int chanceLess = 3;
        /* column based algorithm
           * for the first column a y-begin and y-end value is generated at random
           * for all the upcoming columns the algorithm determines at random if the column should extend or decrease at the top and at the bottom
           * the chance of extending is determined by the value chanceExtra and is expressed as a possibility between 0 and maxRandom
           * the chance of decreasing the column is expressed by the value chanceLess
           */
        public bool[,] BuildWorld1(int maxy,int maxx)
        {          
            int dx=maxx;
            int dy=maxy;
            bool[,] squares=new bool[dx,dy];
            int y1 = random.Next(dy);
            int y2 = random.Next(dy);
            int yb = Math.Min(y1,y2);
            int ye = Math.Max(y1,y2);
            for (int i = 0; i < dx; i++)
            {
                for(int j=yb;j<ye;j++) squares[i,j] = true;
                switch (build())
                {
                    case 1: if (yb>0) yb--; break;
                    case -1: if (yb<maxy) yb++; break;
                }
                switch (build())
                {
                    case 1: if (ye<maxy) ye++; break;
                    case -1: if (ye>0) ye--; break;
                }
                if (ye < yb) break;
            }
            return squares;
        }
        /* seeds/coverage based algorithm
             * the goal is to cover a certain amount of a rectangular map, this amount is expressed as a number between 0 and 1. A vaulue of 0.7 means a 70 % coverage of the rectangle
             * the algorithm starts with a number of seeds which are randomly chosen in the map and added to a list
             * from this list an element is picked randomly and a new element is chosen by selecting an adjacent element 
             * the adjacent element is determined by randomly picking a direction (right, left, up or under) relative to the first element
             * if the new element is within the boundary of the map and not already chosen then it is added to the list
             * this procedure will be repeated until the required coverage is reached
             */
        public bool[,] BuildWorld2(int maxy,int maxx,double coverage)
        {
            
            bool[,] squares = new bool[maxx, maxy];
            int seeds = 5;
            int coverageRequired =(int)(coverage*maxx*maxy);//procent
            int currentCoverage = 0;
            int x, y;
            List<(int, int)> list = new();
            for(int i = 0;i<seeds;i++)
            {
                x=random.Next(maxx); y=random.Next(maxy);
                if (!list.Contains((x, y))) { list.Add((x, y)); currentCoverage++; squares[x, y] = true; }

            }
            int index;
            int direction;//0-right,1-left,2-top,3-bottom
            while(currentCoverage < coverageRequired)
            {
                index=random.Next(list.Count);
                direction = random.Next(4);
                switch (direction)
                {
                    case 0:
                        if ((list[index].Item1 < maxx - 1) && !squares[list[index].Item1 + 1, list[index].Item2]) 
                        {
                            squares[list[index].Item1 + 1, list[index].Item2] = true;
                            list.Add((list[index].Item1 + 1, list[index].Item2));
                            currentCoverage++; 
                        }
                        break;
                    case 1:
                        if ((list[index].Item1 > 0) && !squares[list[index].Item1 - 1, list[index].Item2])
                        {
                            squares[list[index].Item1 - 1, list[index].Item2] = true;
                            list.Add((list[index].Item1 - 1, list[index].Item2));
                            currentCoverage++;
                        }
                        break;
                    case 2:
                        if ((list[index].Item2 < maxy - 1) && !squares[list[index].Item1, list[index].Item2 + 1])
                        {
                            squares[list[index].Item1, list[index].Item2 + 1]=true;
                            list.Add((list[index].Item1, list[index].Item2 + 1));
                            currentCoverage++;
                        }
                        break;
                    case 3:
                        if ((list[index].Item2 > 0) && !squares[list[index].Item1, list[index].Item2 - 1])
                        {
                            squares[list[index].Item1, list[index].Item2 - 1] = true;
                            list.Add((list[index].Item1, list[index].Item2 - 1));
                            currentCoverage++;
                        }
                        break;
                }
            }            
            return squares;
        }
        /* e.g. with a value of 10 for maxRandom an a value of 6 for chanceExtra and 3 for chanceLess
         * the probability of an extension is 3/10 - values 7,8 and 9
         * the probability of a decrease is 3/10 - values 0,1 and 2
         * the probability of no change is 4/10 - values 3,4,5 and 6
         */
        private int build()
        {
            int x = random.Next(maxRandom);
            if (x > chanceExtra) return 1;
            if (x < chanceLess) return -1;
            return 0;
        }
    }
}

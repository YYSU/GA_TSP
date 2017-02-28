using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestC_sharp
{
    class Program
    {
        double[,] distance;
        int[] fitness;
        int[] BestLine; //最佳路線
        int[] WorstLine; //最差路線
        int city = 29;
        int RunTime = 30;
        int PopulationSize;
        int Maxgenerations;
        int Best = 0, GBest;
        int Worst = 0, GWorst;

        //交配,突變
        double PC, PM;
        Random rand = new Random();

        List<int[]> list = new List<int[]>();
        List<int[]> newlist = new List<int[]>();
        static double DisplayStatisticInfo(int N, double Sum, double SumOfSQR)
        {
            double STDDev = Math.Sqrt((N * SumOfSQR - Math.Pow(Sum, 2)) / Math.Pow(N, 2));       //Caculate Standard Deviation
            Console.WriteLine("=========================================");
            Console.WriteLine("           Average = {0}", Sum / N);
            Console.WriteLine("Standart Deviation = {0}", STDDev);
            return STDDev;
        }
        static void Main(string[] args)
        {
            Program ga = new Program();
            ga.init_bays29();
            ga.Init(1000, 100, 0.8, 0.3);

            double Sum = 0.0, SumOfSQR = 0.0;
            for (int i = 0; i < ga.RunTime; i++)
            {
                Console.WriteLine("=========================================");
                Console.WriteLine("Exam : {0}th time", i + 1);
                for (int j = 0; j < ga.Maxgenerations; j++)
                {
                    ga.Tournament_Selection();
                    ga.OX();
                }
                ga.print(ga.list);
                Sum += ga.GBest;
                SumOfSQR += Math.Pow(ga.GBest, 2);
            }
            double SSD = DisplayStatisticInfo(ga.RunTime, Sum, SumOfSQR);

            StreamWriter fileWriter = new StreamWriter("GA-29TSP.csv");
            fileWriter.WriteLine("BS, WS, SA, SSD");
            fileWriter.WriteLine(ga.Best + "," + ga.Worst + "," + Sum / ga.RunTime + "," + SSD);
            fileWriter.Close();
            int[] arr = { 14, 17, 16, 13, 21, 10, 24, 6, 22, 26, 7, 23, 15, 18, 3, 19, 9, 12, 0, 20, 1, 2, 28, 25, 8, 4, 5, 11, 27, 14 };
            //Console.WriteLine("{0} ", ga.SUM_Line(arr));
            Console.ReadLine();
        }
        public int SUM_Line(int[] sum)
        {
            int total = 0;
            for (int i = 1; i < sum.Length; i++)
            {
                total += (int)distance[sum[i - 1], sum[i]];
            }
            return total;
        }
        public int[] find_BestandWorst()
        {
            //              min        , max       ,SD
            int[] array = { fitness[0], fitness[0], 0 };
            for (int i = 0; i < fitness.Length; i++)
            {
                if (fitness[i] < array[0])
                {
                    array[0] = fitness[i];
                    BestLine = list[i];
                }
                if (fitness[i] > array[1])
                {
                    array[1] = fitness[i];
                    WorstLine = list[i];
                }
            }
            return array;
        }
        public void OX()
        {
            for (int i = 0; i < list.Count; i++)
            {
                int choose1 = rand.Next(0, PopulationSize);
                int choose2 = rand.Next(0, PopulationSize);
                int[] newArray = new int[list[choose1].Length];

                double Mate = rand.Next(0, 1);
                if (Mate < PC)
                {
                    //交配
                    int start = rand.Next(0, list[choose1].Length - 1);
                    int end = rand.Next(0, list[choose1].Length - 1);
                    if (start > end)
                    {
                        int temp = start;
                        start = end;
                        end = temp;
                    }

                    HashSet<int> set = new HashSet<int>();
                    for (int j = start; j < end; j++)
                    {
                        newArray[j] = list[choose1][j];
                        set.Add(list[choose1][j]);
                    }


                    //Parent 1: 8 4 7 (3 6 2 5 1) 9 0
                    //Parent 2: 0 1 2 3 4 5 6 7 8 9

                    //Child 1:  0 4 7 (3 6 2 5 1) 8 9

                    int indexCheck = 0;
                    int index = 0;

                    while (indexCheck < 29)
                    {
                        if (index == start)
                        {
                            index = end;
                        }
                        if (set.Contains(list[choose2][indexCheck]))
                        {
                            indexCheck++;
                        }
                        else
                        {
                            newArray[index++] = list[choose2][indexCheck++];
                        }
                    }
                    newArray[29] = newArray[0];
                }
                else
                {
                    //不交配,取fitness低的當作新的         
                    newArray = fitness[choose1] < fitness[choose2] ? list[choose1] : list[choose2];
                }

                double MutateRate = rand.Next(0, 1);
                if (MutateRate < PM)
                {
                    newArray = Mutate(newArray);
                }
                newlist.Add(newArray);
            }
            Translate();
        }
        public void Cross_partial_change()
        {
            for (int i = 0; i < list.Count; i++)
            {
                int choose1 = rand.Next(0, PopulationSize);
                int choose2 = rand.Next(0, PopulationSize);
                int[] newArray = new int[list[choose1].Length];

                double Mate = rand.Next(0, 1);
                if (Mate < PC)
                {
                    //交配
                    int start = rand.Next(0, list[choose1].Length - 1);
                    int end = rand.Next(0, list[choose1].Length - 1);
                    if (start > end)
                    {
                        int temp = start;
                        start = end;
                        end = temp;
                    }

                    int index = 0;
                    HashSet<int> set = new HashSet<int>();
                    for (int j = start; j < end; j++)
                    {
                        newArray[index] = list[choose2][j];
                        set.Add(list[choose2][j]);
                        index++;
                    }

                    //補足後面剩下的
                    //choose1 : 1 5 7 6 8 4 2 3 10 24 14
                    //choose2 : 28 9 8 5 3 4 7 1 2 15 16
                    //ex start=3 , end=10
                    //newArray = 8 5 3 4 7 1 2 15
                    //
                    //
                    int indexCheck = 0;

                    for (int j = (end - start); j < 29; j++)
                    {
                        if (indexCheck == 30)
                        {

                            break;
                        }
                        if (set.Contains(list[choose1][indexCheck]))
                        {
                            j--;
                        }
                        else
                        {
                            newArray[j] = list[choose1][indexCheck];
                        }
                        indexCheck++;
                    }
                    newArray[29] = newArray[0];
                }
                else
                {
                    //不交配,取fitness低的當作新的              
                    newArray = fitness[choose1] < fitness[choose2] ? list[choose1] : list[choose2];
                }
                //突變
                double MutateRate = rand.Next(0, 1);
                if (MutateRate < PM)
                {
                    newArray = Mutate(newArray);
                }
                newlist.Add(newArray);
            }
            Translate();
        }
        public int[] Mutate(int[] newArray)
        {
            int position = rand.Next(0, 28);
            if (position == 0)
            {
                newArray[0] = newArray[1];
                newArray[1] = newArray[29];
                newArray[29] = newArray[0];
            }
            else
            {
                int temp = newArray[position];
                newArray[position] = newArray[position + 1];
                newArray[position + 1] = temp;
            }
            return newArray;
        }
        public int[] Inversion(int[] newArray)
        {
            int point1 = rand.Next(1, 28);
            int point2 = rand.Next(1, 28);
            if (point2 < point1)
            {
                int temp = point1;
                point1 = point2;
                point2 = temp;
            }
            else if (point1 == point2)
            {
                return newArray;
            }
            //point1 = 10 ,  point2 = 15
            int[] reverseArray = new int[(point2 - point1) + 1];
            int index = point1;
            for (int i = 0; i < (point2 - point1 + 1); i++)
            {
                reverseArray[i] = newArray[index++];
            }
            Array.Reverse(reverseArray);
            index = 0;
            for (int i = point1; i < point2 + 1; i++)
            {
                newArray[i] = reverseArray[index++];
            }
            return newArray;
        }

        public void Tournament_Selection()
        {
            //隨便取兩個來比
            for (int i = 0; i < list.Count; i++)
            {
                int choose1 = rand.Next(0, PopulationSize);
                int choose2 = rand.Next(0, PopulationSize);

                if (fitness[choose1] < fitness[choose2])
                {
                    newlist.Add(list[choose1]);
                }
                else
                {
                    newlist.Add(list[choose2]);
                }
            }
            Translate();
        }
        public void Translate()
        {
            list = newlist.ToList<int[]>();
            fitness_array();
            newlist.Clear();
        }
        public void print(List<int[]> list)
        {
            /*
            double average = 0.0;   //平均
            double SD = 0.0;  //標準差
            for (int i = 0; i < list.Count; i++)
            {
                average += (double)fitness[i];
            }
            average /= list.Count;

            for (int i = 0; i < fitness.Length; i++)
            {
                //SD += Math.Pow(fitness[i] - average, 2);
                SD += (fitness[i] - average) * (fitness[i] - average);
            }
            SD = Math.Sqrt(SD /fitness.Length);
            */
            GBest = find_BestandWorst()[0];
            GWorst = find_BestandWorst()[1];

            if (GBest > Best)
            {
                Best = GBest;
            }
            if (GWorst > Worst)
            {
                Worst = GWorst;
            }

            Console.WriteLine("GBEST FIT = {0}", GBest);
            Console.WriteLine("GWORST FIT = {0}", GWorst);
            /*
            Console.WriteLine("GWORST FIT = {0}", GWorst);
            Console.WriteLine("Average = {0}", average);
            Console.WriteLine("標準差:{0}",  SD);
            */

            Console.Write("最佳路線:");
            for (int i = 0; i < BestLine.Length; i++)
            {
                Console.Write("{0} ", BestLine[i]);
            }
            Console.WriteLine();

            Console.Write("最差路線:");
            for (int i = 0; i < WorstLine.Length; i++)
            {
                Console.Write("{0} ", WorstLine[i]);
            }
            Console.WriteLine();

            Console.WriteLine();
        }

        public void Init(int PopulationSize, int generations, double PC, double PM)
        {
            this.PopulationSize = PopulationSize;
            this.Maxgenerations = generations;
            this.PC = PC;
            this.PM = PM;
            fitness = new int[PopulationSize];
            for (int j = 0; j < PopulationSize; j++)
            {
                int[] array = new int[30];
                for (int i = 0; i < 29; i++)
                {
                    array[i] = i;
                }
                Shuffle(array);
                array[29] = array[0];
                for (int k = 0; k < array.Length; k++)
                {
                    //Console.Write("{0}", array[k]+1);
                }
                //Console.WriteLine();
                list.Add(array);
            }
            fitness_array();
        }

        public void fitness_array()
        {
            for (int i = 0; i < list.Count; i++)
            {
                fitness[i] = fitness_one(list[i]);
            }
        }

        public int fitness_one(int[] array)
        {
            int sum = 0;
            for (int i = 0; i < array.Length - 1; i++)
            {
                //點 i ~ 點 i + 1 的距離
                sum += (int)distance[array[i], array[i + 1]];
                //Console.WriteLine("點:{0} 到 點:{1}   距離:{2}", array[i] + 1, array[i + 1] + 1, distance[array[i], array[i + 1]]);
            }
            //Console.WriteLine("Total Distance:{0}", sum);
            return sum;
        }

        public void Shuffle(int[] cards)
        {
            int randIndex;
            int tempNum;
            for (int i = 0; i < cards.Length - 1; ++i)
            {
                randIndex = rand.Next(0, cards.Length - 1);
                tempNum = cards[randIndex];
                cards[randIndex] = cards[i];
                cards[i] = tempNum;
            }
        }

        //Distance 矩陣讀檔
        public void init_bays29()
        {
            distance = new double[city, city];
            StreamReader str = new StreamReader("tsp_29.txt");
            string all = str.ReadToEnd();
            string[] c = all.Split(new char[] { ' ', '\n' });
            int index = 0;
            for (int i = 0; i < 29; i++)
            {
                for (int j = 0; j < 29; j++)
                {
                    for (int k = index; k < c.Length; k++)
                    {
                        if (c[k] != "")
                        {
                            index = k + 1;
                            break;
                        }
                    }
                    distance[i, j] = Double.Parse(c[index - 1]);
                    //Visibility[i, j] = 1 / distance[i][j];
                }
            }
        }
    }
}

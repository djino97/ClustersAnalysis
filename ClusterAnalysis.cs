using System;
using System.Collections.Generic;
using System.Linq;

class ClusterAnalysis
{
    public double[,] setForm;

    public Dictionary<int, List<int>> clusterEuclid;
    public Dictionary<int, List<int>> clusterDominance;

    private Dictionary<int, List<int>> cluster;

    private double[] weight;

    private int[] indexCenterCluster;

    private double[,] centerCluster;
    private bool methodCalculationDistance = false;

    public void ClusterAnalysisMain(int countCluster, double[] weight, bool normalization)
    {
        ClusterAnalysis clusterAnalysis = new ClusterAnalysis();

        double[,] setForm = new double[30, 3] // Входное множество образов для кластеризации  
        {
                {15, 6, 15},
                {1, 19, 0},
                {-8, -1, 4},
                {13, 19, 15},
                {15, 17, -14},
                {-3, 9, -35},
                {12, 4, 16},
                {8, 14, 9},
                {-6, 0, 5},
                {11, 17, 10},
                {12, 17, -10},
                {-1,10,-25},
                {18,17,-11},
                {-4,9,-31},
                {19, 4,13},
                {8,14,10},
                {-6,-5,1},
                {20,20,20},
                {7,16,-17},
                {-1,7,-26},
                {15,1,10},
                {0,11,8},
                {-8,-1,5},
                {10,10,10},
                {12,15,-10},
                {-4,5,-27},
                {-7,-1,4},
                {3,17,11},
                {0,1,3},
                {7,2,0}
        };

        // Применение нормализации
        if (normalization)
            setForm = GetNormineSetForm(setForm);

        ClusterCalculationByTwoMethods(setForm, countCluster, weight);

    }

    // Нормализация входных векторов
    static double[,] GetNormineSetForm(double[,] inputSetForm)
    {
        List<double> masX, masY, masZ;
        List<double> CmasX, CmasY, CmasZ;
        double curX, maxX, minX;
        double curY, maxY, minY;
        double curZ, maxZ, minZ;

        masX = new List<double>();
        masY = new List<double>();
        masZ = new List<double>();

        for (int i = 0; i < inputSetForm.GetLength(0); i++)
        {

            masX.Add(inputSetForm[i, 0]);
            masY.Add(inputSetForm[i, 1]);
            masZ.Add(inputSetForm[i, 2]);

        }


        CmasX = new List<double>();
        CmasY = new List<double>();
        CmasZ = new List<double>();
        CmasX.AddRange(masX);
        CmasY.AddRange(masY);
        CmasZ.AddRange(masZ);

        for (int i = 0; i < CmasX.Count; i++)
        {
            curX = CmasX[i];
            maxX = masX.Max();
            minX = masX.Min();

            inputSetForm[i, 0] = Math.Round((curX - minX) / (maxX - minX), 1);

            curY = CmasY[i];
            maxY = masY.Max();
            minY = masY.Min();

            inputSetForm[i, 1] = Math.Round((curY - minY) / (maxY - minY), 1);

            curZ = CmasZ[i];
            maxZ = masZ.Max();
            minZ = masZ.Min();

            inputSetForm[i, 2] = Math.Round((curZ - minZ) / (maxZ - minZ), 1);

        }

        return inputSetForm;
    }

    //Вычисление кластеров по двум методам нахождения расстояния между центроидом и образом 
    private void ClusterCalculationByTwoMethods(double[,] setForm, int countCluster, double[] weight)
    {
        bool calculationClusters = false;
        
        this.setForm = setForm;
        this.weight = weight;

        indexCenterCluster = new int[countCluster];
        indexCenterCluster = GetInitialValuesCenterCluster(countCluster);
        centerCluster = new double[indexCenterCluster.Length, setForm.GetLength(1)];

        while (!calculationClusters)
        {
            for (int i = 0; i < indexCenterCluster.Length; i++)
            {
                for (int j = 0; j < setForm.GetLength(1); j++)
                {
                    centerCluster[i, j] = setForm[indexCenterCluster[i] - 1, j];
                }

            }

            double[,] tmpCenterCluster = new double[centerCluster.GetLength(0), centerCluster.GetLength(1)];
            CalculationClusters(tmpCenterCluster);

            if (methodCalculationDistance)
            {
                calculationClusters = true;
                clusterDominance = cluster;
            }
            else
                clusterEuclid = cluster;

            methodCalculationDistance = true;
        }

    }


    // Инициализируются начальные центроиды для заданных кластеров
    private int[] GetInitialValuesCenterCluster(int countCluster)
    {
        Random rnd = new Random();

        int rndCenter = 0;
        bool checkIntRandom = false;

        for (int i = 0; i < countCluster; i++)
        {
            while (!checkIntRandom)
            {
                rndCenter = rnd.Next(0, setForm.GetLength(0)) + 1;

                if (i != 0)
                {
                    for (int j = 0; j < indexCenterCluster.Length; j++)
                    {
                        if (indexCenterCluster[j] == rndCenter)
                        {
                            checkIntRandom = false;
                            break;
                        }
                        else if (!checkIntRandom)
                            checkIntRandom = true;
                    }
                }
                else
                    break;

            }

            indexCenterCluster[i] = rndCenter;
            checkIntRandom = false;
        }

        return indexCenterCluster;
    }


    //Вычисляет кластеры с помощью метода по k-средним
    private void CalculationClusters(double[,] tmpCenterCluster)
    {
        bool stabilization = false;
        int countIter = 0;

        while (!stabilization)
        {
            CalculationClustersForEachPoint(indexCenterCluster);

            for (int i = 0; i < centerCluster.GetLength(0); i++)
            {
                for (int j = 0; j < centerCluster.GetLength(1); j++)
                {
                    tmpCenterCluster[i, j] = centerCluster[i, j];
                }
            }

            RecalculationCenterCluster();

            stabilization = CheckStabilizationCenter(tmpCenterCluster);
            countIter++;
        }
    }


    // Поиск подходящего кластера для каждого рассматриваемого образа 
    private void CalculationClustersForEachPoint(int[] indexCenterCluster)
    {
        Dictionary<int, double> pointsDistance;
        cluster = new Dictionary<int, List<int>>();

        int numberCluster;

        for (int i = 0; i < setForm.GetLength(0); i++)
        {
            pointsDistance = new Dictionary<int, double>();

            for (int k = 0; k < indexCenterCluster.Length; k++)
            {
                pointsDistance.Add(k, SelectMethodCalculationDistance(centerCluster[k, 0], centerCluster[k, 1],
                centerCluster[k, 2], setForm[i, 0], setForm[i, 1], setForm[i, 2], methodCalculationDistance));
            }

            numberCluster = GetCenterClusterWithMinDistanceBetweenPointAndCenterCluster(pointsDistance);


            if (!cluster.ContainsKey(numberCluster))
                cluster.Add(numberCluster, new List<int>());


            cluster[numberCluster].Add(i);

        }
    }


    // Выбор метода определения расстояния между центроидами и образами
    private double SelectMethodCalculationDistance(double x1, double x2,
    double x3, double y1, double y2, double y3, bool methodCalculationDistance)
    {
        if (!methodCalculationDistance)
            return CalculationDistanceBetweenPointAndCenterClusterEuclid(x1, x2, x3, y1, y2, y3);
        else
            return CalculationDistanceBetweenPointAndCenterClusterDominance(x1, x2, x3, y1, y2, y3);
    }


    // Вычисление Евклидово расстояния между образами
    private double CalculationDistanceBetweenPointAndCenterClusterDominance(double x1, double x2,
    double x3, double y1, double y2, double y3)
    {
        List<double> dominanceValue;
        dominanceValue = new List<double>(){Math.Abs(weight[0] * (x1 - y1)), Math.Abs(weight[1] * (x2 - y2)),
                                               Math.Abs(weight[2] * (x3 - y3))};
        return dominanceValue.Max();
    }

    // Вычисление расстояния доминирования между образами
    private double CalculationDistanceBetweenPointAndCenterClusterEuclid(double x1, double x2,
    double x3, double y1, double y2, double y3)
    {
        return Math.Pow(weight[0] * (x1 - y1), 2) + Math.Pow(weight[1] * (x2 - y2), 2)
               + Math.Pow(weight[2] * (x3 - y3), 2);
    }


    private int GetCenterClusterWithMinDistanceBetweenPointAndCenterCluster(Dictionary<int, double> pointsDistance)
    {
        pointsDistance = pointsDistance.OrderBy(dict => dict.Value).ToDictionary(dict => dict.Key, dict => dict.Value);

        return pointsDistance.ElementAt(0).Key;
    }

    //Перерасчет координат центроидов
    private void RecalculationCenterCluster()
    {
        double[] summPoint;
        List<int> clusterPoints;

        for (int i = 0; i < centerCluster.GetLength(0); i++)
        {
            summPoint = new double[centerCluster.GetLength(1)];

            try
            {
                clusterPoints = cluster.ElementAt(i).Value;

                foreach (int indexPoint in clusterPoints)
                {
                    for (int k = 0; k < centerCluster.GetLength(1); k++)
                    {
                        summPoint[k] += setForm[indexPoint, k];
                    }
                }

                for (int k = 0; k < centerCluster.GetLength(1); k++)
                {
                    centerCluster[cluster.ElementAt(i).Key, k] = summPoint[k] / (double)clusterPoints.Count;
                }
            }
            catch
            {
                cluster.Add(i + centerCluster.GetLength(0), new List<int>());
            }

        }
    }


    // Проверка стабилизации центров кластеров
    private bool CheckStabilizationCenter(double[,] tmpCenterCluster)
    {
        bool stabilization = false;

        for (int i = 0; i < centerCluster.GetLength(0); i++)
        {
            for (int j = 0; j < centerCluster.GetLength(1); j++)
            {
                if (tmpCenterCluster[i, j] - centerCluster[i, j] == 0)
                    stabilization = true;
                else
                    return false;
            }

        }

        return stabilization;

    }
}

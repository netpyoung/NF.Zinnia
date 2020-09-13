using System;
using System.Collections.Generic;

namespace NF.Zinnia
{
    public class SVM
    {
        // Support Vector Machine

        private const double EPS = 0.1;
        private const double INF = 1e+37;
        private readonly static Random RAND = new Random();

        public static bool Train(
            int l,
            int n,
            List<double> y,
            List<List<FeatureNode>> x,
            double C,
            double[] w)
        {
            int active_size = l;
            double PGmax_old = INF;
            double PGmin_old = -INF;
            double[] QD = new double[l];
            int[] index = new int[l];
            double[] alpha = new double[l];

            Array.Clear(w, 0, n);

            for (int i = 0; i < l; i++)
            {
                index[i] = i;
                QD[i] = 0;

                foreach (FeatureNode f in x[i])
                {
                    if (f.Index < 0)
                    {
                        break;
                    }

                    QD[i] += (f.Value * f.Value);
                }
            }

            const int MAX_ITERATION = 2000;
            for (int iter = 0; iter < MAX_ITERATION; ++iter)
            {
                double PGmax_new = -INF;
                double PGmin_new = INF;

                RAND.Shuffle(index);

                for (int s = 0; s < active_size; ++s)
                {
                    int i = index[s];
                    double G = 0;

                    List<FeatureNode> featureNodes = x[i];
                    foreach (FeatureNode f in featureNodes)
                    {
                        if (f.Index < 0)
                        {
                            break;
                        }
                        G += w[f.Index] * f.Value;
                    }

                    G = G * y[i] - 1;

                    double PG = 0;

                    if (alpha[i] == 0)
                    {
                        if (G > PGmax_old)
                        {
                            active_size--;
                            int tmp = index[s];
                            index[s] = index[active_size];
                            index[active_size] = tmp;
                            s--;
                            continue;
                        }
                        else if (G < 0.0)
                        {
                            PG = G;
                        }
                    }
                    else if (alpha[i] == C)
                    {
                        if (G < PGmin_old)
                        {
                            active_size--;
                            int tmp = index[s];
                            index[s] = index[active_size];
                            index[active_size] = tmp;
                            s--;
                            continue;
                        }
                        else if (G > 0)
                        {
                            PG = G;
                        }
                    }
                    else
                    {
                        PG = G;
                    }

                    PGmax_new = Math.Max(PGmax_new, PG);
                    PGmin_new = Math.Min(PGmin_new, PG);

                    if (Math.Abs(PG) > 1.0e-12)
                    {
                        double alphaOld = alpha[i];
                        alpha[i] = Math.Min(Math.Max(alpha[i] - G / QD[i], 0.0), C);
                        double d = (alpha[i] - alphaOld) * y[i];
                        foreach (FeatureNode f in featureNodes)
                        {
                            if (f.Index < 0)
                            {
                                break;
                            }
                            w[f.Index] += d * f.Value;
                        }
                    }
                }

                if (iter % 4 == 0)
                {
                    Console.Write(".");
                }

                if ((PGmax_new - PGmin_new) < EPS)
                {
                    if (active_size == l)
                    {
                        break;
                    }
                    else
                    {
                        active_size = l;
                        PGmax_old = INF;
                        PGmin_old = -INF;
                        continue;
                    }
                }

                PGmax_old = PGmax_new;
                PGmin_old = PGmin_new;
                if (PGmax_old <= 0)
                {
                    PGmax_old = INF;
                }
                if (PGmin_old <= 0)
                {
                    PGmin_old = -INF;
                }
            }

            Console.WriteLine();
            return true;
        }
    }
}

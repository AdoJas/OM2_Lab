using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace BoxOptimization
{
    class Program
    {
        // Tikslo funkcija: f(X1,X2) = -1/8(X1X2 - X1²X2 - X2²X1)
        static double ObjectiveFunction(double X1, double X2)
        {
            double X3 = 1 - X1 - X2;
            return -(X1 * X2 * X3) / 8;
        }

        // Gradientas
        static (double dfdX1, double dfdX2) Gradient(double X1, double X2)
        {
            return (
                -(X2 * (1 - 2 * X1 - X2)) / 8,
                -(X1 * (1 - X1 - 2 * X2)) / 8
            );
        }

        // Lipsico konstanta
        static double ComputeGamma(double X1, double X2)
        {
            double a = 0.25 * X2;
            double b = -0.125 * (1 - 2 * X1 - 2 * X2);
            double c = 0.25 * X1;
    
            double lambda = (a + c + Math.Sqrt((a - c) * (a - c) + 4 * b * b)) / 2;
            return 1.0 / lambda;
        }

        static (double X1, double X2, double volume, int steps, int gradCalls, int objCalls, List<(double X1, double X2, double Value)> path) OptimizedGradientDescent(double X1, double X2, int maxSteps)
        {
            int gradCalls = 0, objCalls = 0;
            int stepCount = 0;
            var path = new List<(double X1, double X2, double Value)>();
            path.Add((X1, X2, ObjectiveFunction(X1, X2)));

            for (int i = 0; i < maxSteps; i++)
            {
                stepCount++;
                var (dfdX1, dfdX2) = Gradient(X1, X2);
                gradCalls++;
                
                if (Math.Sqrt(dfdX1 * dfdX1 + dfdX2 * dfdX2) < 1e-6) break;
                
                double gamma = ComputeGamma(X1, X2);
                X1 -= gamma * dfdX1;
                X2 -= gamma * dfdX2;

                path.Add((X1, X2, ObjectiveFunction(X1, X2)));
            }

            double X3 = 1 - X1 - X2;
            double volume = Math.Sqrt(X1 * X2 * X3 / 8);
            return (X1, X2, volume, stepCount, gradCalls, objCalls, path);
        }

        public static (double X1, double X2, double Volume, int Steps, int GradCalls, int ObjCalls, List<(double X1, double X2, double Value)> Path) SteepestDescent(double X1, double X2, int maxSteps, double tolerance = 1e-6)
        {
        int gradCalls = 0, objCalls = 0, steps = 0;
        var path = new List<(double X1, double X2, double Value)>();
        path.Add((X1, X2, ObjectiveFunction(X1, X2)));

        for (int i = 0; i < maxSteps; i++)
        {
            steps++;
            
            var (dfdX1, dfdX2) = Gradient(X1, X2);
            gradCalls++;

            double gradNorm = Math.Sqrt(dfdX1 * dfdX1 + dfdX2 * dfdX2);
            if (gradNorm < 1e-6)
                break;

            // phi(alpha) = f(X1 - alpha*dfdX1, X2 - alpha*dfdX2)
            double Phi(double alpha)
            {
                double newX1 = X1 - alpha * dfdX1;
                double newX2 = X2 - alpha * dfdX2;
                objCalls++;
                return ObjectiveFunction(newX1, newX2);
            }

            double a = 0, b = 4.5; 
            double phi = (Math.Sqrt(5) - 1) / 2; 
            double alpha1 = b - phi * (b - a);
            double alpha2 = a + phi * (b - a);
            double f1 = Phi(alpha1);
            double f2 = Phi(alpha2);
            
            while (Math.Abs(b - a) > tolerance)
            {
                if (f1 < f2)
                {
                    b = alpha2;
                    alpha2 = alpha1;
                    f2 = f1;
                    alpha1 = b - phi * (b - a);
                    f1 = Phi(alpha1);
                }
                else
                {
                    a = alpha1;
                    alpha1 = alpha2;
                    f1 = f2;
                    alpha2 = a + phi * (b - a);
                    f2 = Phi(alpha2);
                }
            }
            double alpha = (a + b) / 2; 
            X1 -= alpha * dfdX1;
            X2 -= alpha * dfdX2;

            path.Add((X1, X2, ObjectiveFunction(X1, X2)));
        }

        double X3 = 1 - X1 - X2;
        double volume = Math.Sqrt(X1 * X2 * X3 / 8);

        return (X1, X2, volume, steps, gradCalls, objCalls, path);
    }

static (double X1, double X2, double volume, int steps, int objCalls, List<(double X1, double X2, double Value)> path) DeformedSimplex(double X1, double X2, int maxSteps)
{
    int objCalls = 0;
    var path = new List<(double X1, double X2, double Value)>();
    
    var simplex = new (double X1, double X2, double Value)[3];
    double stepSize = 0.3;
    
    double val0 = ObjectiveFunction(X1, X2);
    simplex[0] = (X1, X2, val0);
    path.Add((X1, X2, val0));
    objCalls++;
    
    double x1 = Math.Min(X1 + stepSize, 1 - X2 - 1e-8);
    double val1 = ObjectiveFunction(x1, X2);
    simplex[1] = (x1, X2, val1);
    path.Add((x1, X2, val1));
    objCalls++;
    
    double x2 = Math.Min(X2 + stepSize, 1 - X1 - 1e-8);
    double val2 = ObjectiveFunction(X1, x2);
    simplex[2] = (X1, x2, val2);
    path.Add((X1, x2, val2));
    objCalls++;

    int steps = 0;
    for (int i = 0; i < maxSteps; i++)
    {
        steps++;
        Array.Sort(simplex, (a, b) => a.Value.CompareTo(b.Value));

        double edgeLength = Math.Max(
            Math.Sqrt(Math.Pow(simplex[1].X1 - simplex[0].X1, 2) + 
                     Math.Pow(simplex[1].X2 - simplex[0].X2, 2)),
            Math.Sqrt(Math.Pow(simplex[2].X1 - simplex[0].X1, 2) + 
                     Math.Pow(simplex[2].X2 - simplex[0].X2, 2))
        );
        if (edgeLength < 1e-6) break;

        double cX1 = (simplex[0].X1 + simplex[1].X1) / 2;
        double cX2 = (simplex[0].X2 + simplex[1].X2) / 2;

        double rX1 = cX1 + 1.0 * (cX1 - simplex[2].X1);
        double rX2 = cX2 + 1.0 * (cX2 - simplex[2].X2);
        rX1 = Math.Max(1e-8, Math.Min(rX1, 1 - 1e-8));
        rX2 = Math.Max(1e-8, Math.Min(rX2, 1 - rX1 - 1e-8));
        double fReflect = ObjectiveFunction(rX1, rX2);
        path.Add((rX1, rX2, fReflect));
        objCalls++;

        if (fReflect < simplex[0].Value)
        {
            double eX1 = cX1 + 2.0 * (rX1 - cX1);
            double eX2 = cX2 + 2.0 * (rX2 - cX2);
            eX1 = Math.Max(1e-8, Math.Min(eX1, 1 - 1e-8));
            eX2 = Math.Max(1e-8, Math.Min(eX2, 1 - eX1 - 1e-8));
            double fExpand = ObjectiveFunction(eX1, eX2);
            path.Add((eX1, eX2, fExpand));
            objCalls++;

            simplex[2] = (fExpand < fReflect) ? (eX1, eX2, fExpand) : (rX1, rX2, fReflect);
        }
        else if (fReflect < simplex[1].Value)
        {
            simplex[2] = (rX1, rX2, fReflect);
        }
        else
        {
            if (fReflect < simplex[2].Value)
            {
                double coX1 = cX1 + 0.5 * (rX1 - cX1);
                double coX2 = cX2 + 0.5 * (rX2 - cX2);
                coX1 = Math.Max(1e-8, Math.Min(coX1, 1 - 1e-8));
                coX2 = Math.Max(1e-8, Math.Min(coX2, 1 - coX1 - 1e-8));
                double fContract = ObjectiveFunction(coX1, coX2);
                path.Add((coX1, coX2, fContract));
                objCalls++;

                if (fContract <= fReflect)
                    simplex[2] = (coX1, coX2, fContract);
                else
                    PerformShrink();
            }
            else
            {
                double ciX1 = cX1 - 0.5 * (cX1 - simplex[2].X1);
                double ciX2 = cX2 - 0.5 * (cX2 - simplex[2].X2);
                ciX1 = Math.Max(1e-8, Math.Min(ciX1, 1 - 1e-8));
                ciX2 = Math.Max(1e-8, Math.Min(ciX2, 1 - ciX1 - 1e-8));
                double fContract = ObjectiveFunction(ciX1, ciX2);
                path.Add((ciX1, ciX2, fContract));
                objCalls++;

                if (fContract < simplex[2].Value)
                    simplex[2] = (ciX1, ciX2, fContract);
                else
                    PerformShrink();
            }
        }

        void PerformShrink()
        {
            for (int j = 1; j < 3; j++)
            {
                double sX1 = simplex[0].X1 + 0.5 * (simplex[j].X1 - simplex[0].X1);
                double sX2 = simplex[0].X2 + 0.5 * (simplex[j].X2 - simplex[0].X2);
                sX1 = Math.Max(1e-8, Math.Min(sX1, 1 - 1e-8));
                sX2 = Math.Max(1e-8, Math.Min(sX2, 1 - sX1 - 1e-8));
                double fShrink = ObjectiveFunction(sX1, sX2);
                simplex[j] = (sX1, sX2, fShrink);
                path.Add((sX1, sX2, fShrink));
                objCalls++;
            }
        }
    }

    double X3 = 1 - simplex[0].X1 - simplex[0].X2;
    double volume = Math.Sqrt(simplex[0].X1 * simplex[0].X2 * X3 / 8);
    
    return (simplex[0].X1, simplex[0].X2, volume, steps, objCalls, path);
}        static void RunPythonVisualization(string jsonFilePath)
        {
            string pythonScriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "visualize_paths.py");
    
            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = "python3",
                Arguments = $"\"{pythonScriptPath}\" \"{jsonFilePath}\"",
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(start))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
        
                process.WaitForExit();
        
                Console.WriteLine(output);
                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine("Python Errors:");
                    Console.WriteLine(error);
                }
            }
        }

        static void Main(string[] args)
        {
            var testPoints = new (string Name, double X1, double X2)[] 
            {
                ("X0", 0.0, 0.0),
                ("X1", 1.0, 1.0),
                ("Xm", 0.7, 0.7)
            };

            Console.WriteLine("| Method            | Start Point | X1        | X2        | Volume    | Steps | GradCalls | ObjCalls | Total Calls |");
            Console.WriteLine("|-------------------|-------------|-----------|-----------|-----------|-------|-----------|----------|-------------|");

            foreach (var point in testPoints)
            {
                var (ogdX1, ogdX2, ogdVol, ogdSteps, ogdGrad, ogdObj, ogdPath) = OptimizedGradientDescent(point.X1, point.X2, 1000);
                Console.WriteLine($"| {"GradientLipschitz",-17} | {point.Name,-11} | {ogdX1,9:F6} | {ogdX2,9:F6} | {ogdVol,9:F6} | {ogdSteps,5} | {ogdGrad,9} | {ogdObj,8} | {ogdGrad + ogdObj,11} |");

                var (sdX1, sdX2, sdVol, sdSteps, sdGrad, sdObj, sdPath) = SteepestDescent(point.X1, point.X2, 1000);
                Console.WriteLine($"| {"SteepestDescent",-17} | {point.Name,-11} | {sdX1,9:F6} | {sdX2,9:F6} | {sdVol,9:F6} | {sdSteps,5} | {sdGrad,9} | {sdObj,8} | {sdGrad + sdObj,11} |");

                var (dsX1, dsX2, dsVol, dsSteps, dsObj, dsPath) = DeformedSimplex(point.X1, point.X2, 1000);
                Console.WriteLine($"| {"DeformedSimplex",-17} | {point.Name,-11} | {dsX1,9:F6} | {dsX2,9:F6} | {dsVol,9:F6} | {dsSteps,5} | {"-",9} | {dsObj,8} | {dsObj,11} |");
                Console.WriteLine("|-------------------|-------------|-----------|-----------|-----------|-------|-----------|----------|-------------|");
            }
            var optimizationResults = new List<object>();

            foreach (var point in testPoints)
            {
                var (ogdX1, ogdX2, ogdVol, ogdSteps, ogdGrad, ogdObj, ogdPath) = OptimizedGradientDescent(point.X1, point.X2, 1000);
                optimizationResults.Add(new
                {
                    Method = "GradientLipschitz",
                    StartPoint = point.Name,
                    Path = ogdPath
                });

                var (sdX1, sdX2, sdVol, sdSteps, sdGrad, sdObj, sdPath) = SteepestDescent(point.X1, point.X2, 1000);
                optimizationResults.Add(new
                {
                    Method = "SteepestDescent",
                    StartPoint = point.Name,
                    Path = sdPath
                });

                var (dsX1, dsX2, dsVol, dsSteps, dsObj, dsPath) = DeformedSimplex(point.X1, point.X2, 1000);
                optimizationResults.Add(new
                {
                    Method = "DeformedSimplex",
                    StartPoint = point.Name,
                    Path = dsPath
                });
            }

            // Sudedam i JSON objekta
            var json = JsonConvert.SerializeObject(optimizationResults, Formatting.Indented);
            File.WriteAllText("optimizationPaths.json", json);

            Console.WriteLine("Optimization paths have been written to 'optimizationPaths.json'.");
            
            var jsonFilePath = "optimizationPaths.json";
            File.WriteAllText(jsonFilePath, json);
            
            try 
            {
                RunPythonVisualization(jsonFilePath);  
                Console.WriteLine("Visualization completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running visualization: {ex.Message}");
            }
        }
    }
}

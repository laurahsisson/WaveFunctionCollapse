﻿// Copyright (C) 2016 Maxim Gumin, The MIT License (MIT)

using System;
using System.Xml.Linq;
using System.Diagnostics;

static class Program
{
    static void Main()
    {
        Stopwatch sw = Stopwatch.StartNew();
        var folder = System.IO.Directory.CreateDirectory("output");
        foreach (var file in folder.GetFiles()) file.Delete();

        Random random = new();
        XDocument xdoc = XDocument.Load("samples.xml");

        foreach (XElement xelem in xdoc.Root.Elements("overlapping", "simpletiled"))
        {
            Model model;
            string name = xelem.Get<string>("name");
            Console.WriteLine($"< {name}");

            bool isOverlapping = xelem.Name == "overlapping";
            int size = xelem.Get("size", isOverlapping ? 48 : 24);
            int width = xelem.Get("width", size);
            int height = xelem.Get("height", size);
            bool periodic = xelem.Get("periodic", false);
            string heuristicString = xelem.Get<string>("heuristic");
            var heuristic = heuristicString == "Scanline" ? Model.Heuristic.Scanline : (heuristicString == "MRV" ? Model.Heuristic.MRV : Model.Heuristic.Entropy);

            if (isOverlapping)
            {
                int N = xelem.Get("N", 3);
                bool periodicInput = xelem.Get("periodicInput", true);
                int symmetry = xelem.Get("symmetry", 8);
                bool ground = xelem.Get("ground", false);

                model = new OverlappingModel(name, N, width, height, periodicInput, periodic, symmetry, ground, heuristic);
            }
            else
            {
                string subset = xelem.Get<string>("subset");
                bool blackBackground = xelem.Get("blackBackground", false);

                model = new SimpleTiledModel(name, subset, width, height, periodic, blackBackground, heuristic);
            }

            int imgcount = xelem.Get("screenshots", 2);
            int retries = xelem.Get("retries", 10);

            for (int i = 0; i < imgcount; i++)
            {
                for (int k = 0; k < retries; k++)
                {
                    Console.Write("> ");
                    int seed = random.Next();
                    bool success = model.Run(seed, xelem.Get("limit", -1));
                    if (success)
                    {
                        Console.WriteLine("DONE");
                        string fname = $"output/{name}";
                        if (imgcount>1) {
                            fname = $"output/{name}{seed}";
                        }
                        Console.WriteLine($"{fname}.png");
                        model.Save($"{fname}.png");
                        if (model is SimpleTiledModel stmodel && xelem.Get("textOutput", false))
                            System.IO.File.WriteAllText($"{fname}.txt", stmodel.TextOutput());
                        break;
                    }
                    else Console.WriteLine($"CONTRADICTION{k}");
                }
            }
        }

        Console.WriteLine($"time = {sw.ElapsedMilliseconds}");
    }
}

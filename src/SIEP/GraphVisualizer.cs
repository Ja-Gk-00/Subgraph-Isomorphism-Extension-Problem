using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

internal static class GraphVisualizer
{
    public static void Visualize(Graph graph, string baseName, ISet<int>? highlightVertices = null)
    {
        string dotPath = baseName + ".dot";
        string pngPath = baseName + ".png";

        var sb = new StringBuilder();
        sb.AppendLine("graph G {");
        sb.AppendLine("  node [shape=circle];");

        for (int i = 0; i < graph.VertexCount; i++)
        {
            if (highlightVertices != null && highlightVertices.Contains(i))
            {
                sb.AppendLine($"  {i} [label=\"{i}\", style=filled, fillcolor=\"yellow\"];");
            }
            else
            {
                sb.AppendLine($"  {i} [label=\"{i}\"];");
            }
        }

        for (int i = 0; i < graph.VertexCount; i++)
        {
            for (int j = i + 1; j < graph.VertexCount; j++)
            {
                int w = graph.Get(i, j);
                if (w > 0)
                {
                    string color =
                        highlightVertices != null &&
                        highlightVertices.Contains(i) &&
                        highlightVertices.Contains(j)
                            ? "red"
                            : "black";

                    sb.AppendLine($"  {i} -- {j} [label=\"{w}\", color=\"{color}\"];");
                }
            }
        }

        sb.AppendLine("}");

        File.WriteAllText(dotPath, sb.ToString());
        Console.WriteLine($"[viz] DOT written: {Path.GetFullPath(dotPath)}");

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "dot",
                Arguments = $"-Tpng \"{dotPath}\" -o \"{pngPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi);
            if (proc == null)
            {
                Console.WriteLine("[viz] Failed to start 'dot' process.");
                return;
            }

            string stdout = proc.StandardOutput.ReadToEnd();
            string stderr = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            if (proc.ExitCode == 0)
            {
                Console.WriteLine($"[viz] PNG written: {Path.GetFullPath(pngPath)}");
            }
            else
            {
                Console.WriteLine($"[viz] dot exit code: {proc.ExitCode}");
                if (!string.IsNullOrWhiteSpace(stdout))
                    Console.WriteLine($"[viz] dot stdout:\n{stdout}");
                if (!string.IsNullOrWhiteSpace(stderr))
                    Console.WriteLine($"[viz] dot stderr:\n{stderr}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[viz] Exception while running dot: {ex.Message}");
        }
    }
}

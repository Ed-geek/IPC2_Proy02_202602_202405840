using System.Diagnostics;

namespace Proyecto2.Services;

public class GraphvizService
{
    private readonly string _dir;

    public GraphvizService(IWebHostEnvironment env)
    {
        _dir = Path.Combine(env.WebRootPath ?? "wwwroot", "reports");
        Directory.CreateDirectory(_dir);
    }

    public string Generate(string dot, string baseName)
    {
        var dotPath = Path.Combine(_dir, baseName + ".dot");
        var svgPath = Path.Combine(_dir, baseName + ".svg");
        File.WriteAllText(dotPath, dot);

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "dot",
                Arguments = $"-Tsvg \"{dotPath}\" -o \"{svgPath}\"",
                RedirectStandardError = true,
                UseShellExecute = false
            };
            using var p = Process.Start(psi);
            p?.WaitForExit(8000);
        }
        catch
        {
           
        }
        return $"/reports/{baseName}.svg";
    }
}
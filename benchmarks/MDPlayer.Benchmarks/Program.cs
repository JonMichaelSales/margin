using System.Diagnostics;
using System.Text.Json;
using MDPlayer.Rendering;

var parser = new MarkdownParser();
const string paragraph = "## A heading\n\nA paragraph with **strong text**, Unicode 日本語 and [a link](https://example.com).\n\n";
var results = new List<object>();
foreach (var bytes in new[] { 100_000, 1_000_000, 10_000_000 })
{
    var source = string.Concat(Enumerable.Repeat(paragraph, bytes / paragraph.Length + 1));
    parser.Parse(source, 0);
    var timings = new List<double>();
    for (var i = 0; i < 30; i++)
    {
        var timer = Stopwatch.StartNew();
        var result = parser.Parse(source, i);
        timings.Add(timer.Elapsed.TotalMilliseconds);
        GC.KeepAlive(result);
    }
    timings.Sort();
    results.Add(new { sourceCharacters = source.Length, samples = timings.Count, p50Ms = timings[14], p95Ms = timings[28], maxMs = timings[^1] });
}
Console.WriteLine(JsonSerializer.Serialize(new { measurement = "Parser only; does not measure open, first viewport, frames, or packaged startup", runtime = Environment.Version.ToString(), architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString(), processors = Environment.ProcessorCount, results }, new JsonSerializerOptions { WriteIndented = true }));

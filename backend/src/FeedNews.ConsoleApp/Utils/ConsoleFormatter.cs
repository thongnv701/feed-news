using FeedNews.ConsoleApp.Models;

namespace FeedNews.ConsoleApp.Utils;

/// <summary>
/// Utility class for formatting console output
/// </summary>
public static class ConsoleFormatter
{
    /// <summary>
    /// Prints the header banner for the application
    /// </summary>
    public static void PrintHeader()
    {
        Console.WriteLine("\n");
        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                                                            ║");
        Console.WriteLine("║         🔄 FeedNews Aggregation Console Application       ║");
        Console.WriteLine("║                                                            ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
        Console.WriteLine();
    }

    /// <summary>
    /// Prints the footer/summary banner
    /// </summary>
    public static void PrintFooter(List<AggregationResult> results)
    {
        Console.WriteLine("\n");
        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║              ✅ AGGREGATION COMPLETE                       ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

        // Calculate totals
        int totalProcessed = results.Sum(r => r.TotalFetched);
        int totalSummarized = results.Sum(r => r.SummarizedCount);
        int totalSelected = results.Sum(r => r.TopSelected);
        int totalSlackSent = results.Sum(r => r.SlackSent);
        int totalFailed = results.Sum(r => r.FailedCount);
        var totalDuration = TimeSpan.FromMilliseconds(results.Sum(r => r.Duration.TotalMilliseconds));
        int successCount = results.Count(r => r.IsSuccess);

        Console.WriteLine("📊 SUMMARY STATISTICS:");
        Console.WriteLine($"   • Total Categories Processed:   {results.Count}");
        Console.WriteLine($"   • Successfully Processed:       {successCount}/{results.Count}");
        Console.WriteLine($"   • Total Articles Fetched:       {totalProcessed}");
        Console.WriteLine($"   • Total Articles Summarized:    {totalSummarized}");
        Console.WriteLine($"   • Total Articles Selected:      {totalSelected}");
        Console.WriteLine($"   • Total Sent to Slack:          {totalSlackSent}");
        Console.WriteLine($"   • Total Failed:                 {totalFailed}");
        Console.WriteLine($"   • Total Duration:               {totalDuration:hh\\:mm\\:ss}");
        Console.WriteLine();

        // Print category breakdown
        Console.WriteLine("📋 CATEGORY BREAKDOWN:");
        foreach (var result in results)
        {
            string statusIcon = result.IsSuccess ? "✅" : "⚠️";
            Console.WriteLine($"\n   {statusIcon} {result.Category}");
            Console.WriteLine($"      ├─ Fetched:     {result.TotalFetched} articles");
            Console.WriteLine($"      ├─ Summarized:  {result.SummarizedCount} articles");
            Console.WriteLine($"      ├─ Selected:    {result.TopSelected} articles");
            Console.WriteLine($"      ├─ Slack Sent:  {result.SlackSent} articles");
            Console.WriteLine($"      ├─ Failed:      {result.FailedCount} articles");
            Console.WriteLine($"      └─ Duration:    {result.Duration.TotalSeconds:F2}s");

            if (!result.IsSuccess && !string.IsNullOrEmpty(result.ErrorMessage))
            {
                Console.WriteLine($"      ⚠️  Warning: {result.ErrorMessage}");
            }
        }

        Console.WriteLine("\n");
    }

    /// <summary>
    /// Prints the start of category processing
    /// </summary>
    public static void PrintCategoryStart(string category)
    {
        Console.WriteLine($"\n🔄 Processing {category}...");
        Console.WriteLine("─────────────────────────────────────────────────────────────");
    }

    /// <summary>
    /// Prints a step progress message
    /// </summary>
    public static void PrintStep(int stepNumber, string stepName, bool isSuccess, string? details = null)
    {
        string statusIcon = isSuccess ? "✅" : "⚠️";
        Console.WriteLine($"  {statusIcon} Step {stepNumber}: {stepName}");
        if (!string.IsNullOrEmpty(details))
        {
            Console.WriteLine($"     └─ {details}");
        }
    }

    /// <summary>
    /// Prints an error message
    /// </summary>
    public static void PrintError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ ERROR: {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Prints a warning message
    /// </summary>
    public static void PrintWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"⚠️  WARNING: {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Prints an info message
    /// </summary>
    public static void PrintInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"ℹ️  INFO: {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Prints a success message
    /// </summary>
    public static void PrintSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✅ {message}");
        Console.ResetColor();
    }
}

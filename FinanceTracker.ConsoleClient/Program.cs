using System.Net.Http;
using System.Text.Json;

Console.WriteLine("====================================");
Console.WriteLine("   Finance Tracker - Summary Report ");
Console.WriteLine("====================================");
Console.WriteLine();

try
{
    var client = new HttpClient();
    var response = await client.GetAsync("https://financetrackernicole-bhbfgxfjfkesdyhb.canadacentral-01.azurewebsites.net/api/analysis/summary");

    if (response.IsSuccessStatusCode)
    {
        var json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<JsonElement>(json);
        Console.WriteLine($"Total Income:          €{data.GetProperty("totalIncome").GetDecimal():0.00}");
        Console.WriteLine($"Total Expenses:        €{data.GetProperty("totalExpenses").GetDecimal():0.00}");
        Console.WriteLine($"Savings Rate:          {data.GetProperty("savingsRate").GetDecimal():0.00}%");
        Console.WriteLine($"Top Expense Category:  {data.GetProperty("topExpenseCategory").GetString()}");
        Console.WriteLine();
        Console.WriteLine("Monthly Trend:");
        Console.WriteLine("------------------------------------");

        var trend = data.GetProperty("monthlyTrend");
        foreach (var month in trend.EnumerateArray())
        {
            Console.WriteLine($"  {month.GetProperty("month").GetString()}: " +
                $"Income €{month.GetProperty("income").GetDecimal():0.00} | " +
                $"Expenses €{month.GetProperty("expenses").GetDecimal():0.00}");
        }
    }
    else
    {
        Console.WriteLine($"Error: {response.StatusCode}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Could not connect to the API: {ex.Message}");
    Console.WriteLine("Make sure the FinanceTracker web app is running first!");
}

Console.WriteLine();
Console.WriteLine("====================================");
Console.ReadLine(); 
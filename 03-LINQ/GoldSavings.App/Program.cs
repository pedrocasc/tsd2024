using GoldSavings.App.Model;
using GoldSavings.App.Client;
using GoldSavings.App.Services;
using System.Xml.Serialization;
namespace GoldSavings.App;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, Gold Investor!");

        // Step 1: Get gold prices
        GoldDataService dataService = new GoldDataService();
        DateTime startDate = new DateTime(2024,09,18);
        DateTime endDate = DateTime.Now;
        List<GoldPrice> goldPrices = dataService.GetGoldPrices(startDate, endDate).GetAwaiter().GetResult();

        if (goldPrices.Count == 0)
        {
            Console.WriteLine("No data found. Exiting.");
            return;
        }

        Console.WriteLine($"Retrieved {goldPrices.Count} records. Ready for analysis.");

        // Step 2: Perform analysis
        GoldAnalysisService analysisService = new GoldAnalysisService(goldPrices);
        var avgPrice = analysisService.GetAveragePrice();

        // Step 3: Print results
        GoldResultPrinter.PrintSingleValue(Math.Round(avgPrice, 2), "Average Gold Price Last Half Year");

        // Question 2.
        // a. Top 3 highest and lowest prices within the last year
        startDate = new DateTime(2024, 03, 18);
        endDate = DateTime.Now;
        var top3Highest = goldPrices.OrderByDescending(gp => gp.Price).Take(3).ToList();
        var top3Lowest = goldPrices.OrderBy(gp => gp.Price).Take(3).ToList();
        GoldResultPrinter.PrintPrices(top3Highest, "\nTop 3 highest prices within the last year\n");
        GoldResultPrinter.PrintPrices(top3Lowest, "\nTop 3 lowest prices within the last year\n");

        // b. 5% gain since January 2020
        // Because of maximum 1 year API calls
        var prices2020 = dataService.GetGoldPrices(new DateTime(2020, 01, 01), new DateTime(2020, 12, 31)).GetAwaiter().GetResult();
        var prices2021 = dataService.GetGoldPrices(new DateTime(2021, 01, 01), new DateTime(2021, 12, 31)).GetAwaiter().GetResult();
        var prices2022 = dataService.GetGoldPrices(new DateTime(2022, 01, 01), new DateTime(2022, 12, 31)).GetAwaiter().GetResult();
        var prices2023 = dataService.GetGoldPrices(new DateTime(2023, 01, 01), new DateTime(2023, 12, 31)).GetAwaiter().GetResult();
        var prices2024 = dataService.GetGoldPrices(new DateTime(2024, 01, 01), new DateTime(2024, 12, 31)).GetAwaiter().GetResult();
        var prices2025 = dataService.GetGoldPrices(new DateTime(2025, 01, 01), DateTime.Now).GetAwaiter().GetResult();
        
        var pricesSinceJan2020 = prices2020
            .Concat(prices2021)
            .Concat(prices2022)
            .Concat(prices2023)
            .Concat(prices2024)
            .Concat(prices2025)
            .OrderBy(gp => gp.Date)
            .ToList();
        
        var jan2020Price = pricesSinceJan2020.FirstOrDefault()?.Price ?? 0;
        var fivePercentDays = pricesSinceJan2020.Where(gp => gp.Price > jan2020Price * 1.05).Take(10).ToList();
        GoldResultPrinter.PrintPrices(fivePercentDays, "\nDays with 5% gain since Jan 2020\n");

        // c. Second ten of the prices ranking in 2019-2022
        var prices2019 = dataService.GetGoldPrices(new DateTime(2019, 01, 01), new DateTime(2019, 12, 31)).GetAwaiter().GetResult();
        var prices2019to2022 = prices2019.Concat(prices2020)
                                            .Concat(prices2021)
                                            .Concat(prices2022)
                                            .OrderBy(gp => gp.Date)
                                            .ToList();
        var ranking2019to2022 = prices2019to2022.OrderByDescending(gp => gp.Price)
                                                .Skip(10).Take(3).ToList();
        GoldResultPrinter.PrintPrices(ranking2019to2022, "Second ten of prices ranking (2019-2022)");

        // d. Averages of gold prices in 2020, 2023 and 2024
        // 2020
        GoldAnalysisService analysisService2020 = new GoldAnalysisService(prices2020);
        var avgPrice2020 = analysisService2020.GetAveragePrice();
        GoldResultPrinter.PrintSingleValue(Math.Round(avgPrice2020, 2), "Average Gold Price in 2020");

        // 2023
        GoldAnalysisService analysisService2023 = new GoldAnalysisService(prices2023);
        var avgPrice2023 = analysisService2023.GetAveragePrice();
        GoldResultPrinter.PrintSingleValue(Math.Round(avgPrice2023, 2), "Average Gold Price in 2023");

        // 2024
        GoldAnalysisService analysisService2024 = new GoldAnalysisService(prices2024);
        var avgPrice2024 = analysisService2024.GetAveragePrice();
        GoldResultPrinter.PrintSingleValue(Math.Round(avgPrice2024, 2), "Average Gold Price in 2024");

        // e. Best buy and sell points for max ROI between 2020 and 2024
        var prices2020to2024 = pricesSinceJan2020.Where(gp => gp.Date.Year >= 2020 && gp.Date.Year <= 2024).ToList();
        var bestBuy = prices2020to2024.OrderBy(gp => gp.Price).FirstOrDefault();
        var bestSell = prices2020to2024.Where(gp => gp.Date > bestBuy.Date).OrderByDescending(gp => gp.Price).FirstOrDefault();
        var roi = ((bestSell.Price - bestBuy.Price) / bestBuy.Price) * 100;
        Console.WriteLine($"\nBest day to buy: {bestBuy.Date.ToShortDateString()} at {bestBuy.Price}");
        Console.WriteLine($"Best day to sell: {bestSell.Date.ToShortDateString()} at {bestSell.Price}");
        Console.WriteLine($"Return on Investment: {Math.Round(roi, 2)}%");

        Console.WriteLine("\nGold Analyis Queries with LINQ Completed.");
    }

    static void SavePricesToXml(List<GoldPrice> prices, string filePath)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(List<GoldPrice>));
        using (TextWriter writer = new StreamWriter(filePath))
        {
            serializer.Serialize(writer, prices);
        }
    }

    static List<GoldPrice> ReadPricesFromXml(string filePath) => (List<GoldPrice>) new XmlSerializer(typeof(List<GoldPrice>)).Deserialize(new StreamReader(filePath));
}

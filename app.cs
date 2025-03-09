/*
 * C# console application
 * Fetches weather data from an external API-openweathermaps, processes the data, generates a chart visualisation using a chart API-ScottPlot, and saves the resulting image locally.
 * Outputs: log.txt, charts (.png), SQLITE database with data
 * To run:
 * In this script, enter your API key @ testingAPIKey OR for production save your key as a environment variable on your machine as: OPENWEATHER_API_KEY
 * Execute the ConsoleApp1.exe program to run, and follow instruction given on the command line
 * If needed, publishing to self contained file with dependencies: dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
 */

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ScottPlot; // v5.0.54
using Serilog; // v 4.2.0
using System.IO;
using System.Data.SQLite; // 1.0.119
using System.Linq;

class Program
{

    // File paths for saving charts, logs, database (can add file path to external folder)
    private static string LogFilePath = "logs.txt";
    private static string dbFilePath = "weather_data.db"; // Database file path
    private static string savePath = "weather_chart_scatter_THW.png";
    private static string savePathTempOnly = "weather_chart_scatter_temp.png";
    private static string savePathTemperature = "weather_chart_bar_temp.png";
    private static string savePathHumidity = "weather_chart_bar_hum.png";
    private static string savePathWindspeed = "weather_chart_bar_wspeed.png";

    // API key for testing (set environment variable for production)
    private static string testingAPIKey = "enter your API key here for testing";

    // Image sizes for chart saving
    private static int imageWidth = 1920;
    private static int imageHeight = 1080;
    static async Task Main()
    {
        // Setup logging
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()  // Console sink
            .WriteTo.File(LogFilePath, rollingInterval: RollingInterval.Day)  // File sink
            .CreateLogger();

        Log.Information("Application has started.");

        try
        {
            // Display list of 5 cities for the user to select
            Console.WriteLine("Select a city by entering the corresponding number:");
            Console.WriteLine("1. Townsville");
            Console.WriteLine("2. Sydney");
            Console.WriteLine("3. Melbourne");
            Console.WriteLine("4. Brisbane");
            Console.WriteLine("5. Adelaide");

            int cityChoice = 0;

            // Validate user input
            while (cityChoice < 1 || cityChoice > 5)
            {
                Console.Write("Enter the number of your choice: ");
                if (int.TryParse(Console.ReadLine(), out cityChoice) && cityChoice >= 1 && cityChoice <= 5)
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid input, please enter a number between 1 and 5.");
                }
            }

            // Switch expression
            // Map the user's choice to the corresponding city name.
            string city = cityChoice switch
            {
                1 => "Townsville",
                2 => "Sydney",
                3 => "Melbourne",
                4 => "Brisbane",
                5 => "Adelaide",
                _ => throw new ArgumentOutOfRangeException()
            };

            // Fetch weather data from an API
            string apiKey = Environment.GetEnvironmentVariable("OPENWEATHER_API_KEY") ?? testingAPIKey; // Fetch API key from environment variable or use default for testing (if LHS null)
            var weatherData = await FetchWeatherData(apiKey, city); // function call, returns list of objects

            if (weatherData == null || weatherData.Count == 0)
            {
                Console.WriteLine($"No weather data found for {city}. Please check the city name or try again later.");
                return;
            }

            // Insert weather data into SQLite database
            if (!File.Exists(dbFilePath))
            {
                CreateDatabase(dbFilePath); // function call, Create the database and table if not exists
            }
            InsertWeatherData(dbFilePath, weatherData, city); // function call, Insert weather data

            Console.WriteLine("Weather data has been inserted into the SQLite database.");

            // Ask user for chart type preference (scatter or bar)
            Console.WriteLine("Select the type of graph you want to generate:");
            Console.WriteLine("1. One Scatter Plot with Temperature, Humidity and Wind speed combined");
            Console.WriteLine("2. Three Bar Charts (Temperature, Humidity, Wind Speed)");
            Console.WriteLine("3. Scatter Plot with Temperature Only");

            int chartChoice = 0;

            // Validate user input for chart type
            while (chartChoice < 1 || chartChoice > 3)
            {
                Console.Write("Enter the number of your choice: ");
                if (int.TryParse(Console.ReadLine(), out chartChoice) && chartChoice >= 1 && chartChoice <= 3)
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid input, please enter 1, 2, or 3.");
                }
            }


            // Generate chart based on user choice
            if (chartChoice == 1)
            {
                GenerateScatterChart(weatherData, savePath); // function call
                Log.Information("Scatter plot saved successfully at {SavePath}", savePath);
            }
            else if (chartChoice == 2)
            {
                GenerateTemperatureBarChart(weatherData, savePathTemperature); // function call
                Log.Information("Bar chart saved successfully at {SavePathTemperature}", savePathTemperature);

                GenerateHumidityBarChart(weatherData, savePathHumidity); // function call
                Log.Information("Bar chart saved successfully at {SavePathHumidity}", savePathHumidity);

                GenerateWindspeedBarChart(weatherData, savePathWindspeed); // function call
                Log.Information("Bar chart saved successfully at {SavePathWindspeed}", savePathWindspeed);
            }
            else
            {
                GenerateTemperatureScatterChart(weatherData, savePathTempOnly); // function call
                Log.Information("Scatter plot for temperature only saved successfully at {SavePathTempOnly}", savePathTempOnly);
            }
        }
        catch (Exception ex)
        {
            Log.Error("An error occurred: {Error}", ex.Message);
            Console.WriteLine("An error occurred. Please check the logs for details.");
        }

        // Close and flush the log
        Log.CloseAndFlush(); // function call

        // Wait for the user input before closing the console window
        Console.WriteLine("Press any key to exit...");
        Console.ReadLine();  // Wait for the user to press a key before closing the application
    } // End of main

    //***********************//

    // Fetch weather data from OpenWeatherMap API
    static async Task<List<WeatherData>> FetchWeatherData(string apiKey, string city)
    {
        string url = $"https://api.openweathermap.org/data/2.5/forecast?q={city}&units=metric&appid={apiKey}"; // free 5 day forcast, historical not free (URL can be added here but)

        using HttpClient client = new HttpClient();
        HttpResponseMessage response = await client.GetAsync(url); // send GET request to API

        if (!response.IsSuccessStatusCode)
        {
            Log.Error("Failed to fetch weather data for {City}. Status Code: {StatusCode}", city, response.StatusCode);
            return null;
        }

        string json = await response.Content.ReadAsStringAsync(); // read API response as string
        var weatherResponse = JsonSerializer.Deserialize<WeatherApiResponse>(json); // Converts the JSON response into a WeatherApiResponse object.

        if (weatherResponse?.list == null || weatherResponse.list.Count == 0)
        {
            Log.Warning("No weather data found for {City}.", city);
            return null;
        }

        // loop takes the raw weather forecast data from the API response and transforms it into a list of WeatherData objects
        List<WeatherData> data = new List<WeatherData>(); // initialise empty list
        foreach (var item in weatherResponse.list)
        {
            data.Add(new WeatherData
            {
                Date = DateTimeOffset.FromUnixTimeSeconds(item.dt).DateTime,
                Temperature = item.main.temp,
                Humidity = item.main.humidity,
                Windspeed = item.wind.speed
                
            });
        }

        return data;
    }

    // Create SQLite database and table
    static void CreateDatabase(string dbFilePath)
    {
        using (var connection = new SQLiteConnection($"Data Source={dbFilePath};Version=3;"))
        {
            connection.Open();

            string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS WeatherData (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Date TEXT,
                    Temperature REAL,
                    Humidity REAL,
                    Windspeed REAL,
                    City TEXT
                    
                );";

            using (var command = new SQLiteCommand(createTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }

    // Insert weather data into SQLite database
    static void InsertWeatherData(string dbFilePath, List<WeatherData> weatherData, string city)
    {
        using (var connection = new SQLiteConnection($"Data Source={dbFilePath};Version=3;"))
        {
            connection.Open();

            foreach (var data in weatherData)
            {
                string insertQuery = @"
                    INSERT INTO WeatherData (Date, Temperature, Humidity, Windspeed, City)
                    VALUES (@Date, @Temperature, @Humidity, @Windspeed, @City);";

                using (var command = new SQLiteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@Date", data.Date.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@Temperature", data.Temperature);
                    command.Parameters.AddWithValue("@Humidity", data.Humidity);
                    command.Parameters.AddWithValue("@Windspeed", data.Windspeed);
                    command.Parameters.AddWithValue("@City", city);


                    command.ExecuteNonQuery();
                }
            }
        }
    }

    // Generate scatter plot chart from weather data (3 data lines, one plot)
    static void GenerateScatterChart(List<WeatherData> weatherData, string savePath)
    {
        var plt = new Plot();

        // converts the list of WeatherData objects into an array of double values
        double[] xs = weatherData.ConvertAll(d => d.Date.ToOADate()).ToArray();
        double[] ysTemperature = weatherData.ConvertAll(d => d.Temperature).ToArray();
        double[] ysHumidity = weatherData.ConvertAll(d => d.Humidity).ToArray();
        double[] ysWindspeed = weatherData.ConvertAll(d => d.Windspeed).ToArray();

        // Add scatter plots for Temperature, Humidity, and Windspeed
        var scatterTemp = plt.Add.Scatter(xs, ysTemperature);
        scatterTemp.LegendText = "Temperature (°C)";
        scatterTemp.LineWidth = 3;
        scatterTemp.Color = ScottPlot.Colors.Magenta;

        var scatterHumidity = plt.Add.Scatter(xs, ysHumidity);
        scatterHumidity.LegendText = "Humidity (%)";
        scatterHumidity.LineWidth = 3;
        scatterHumidity.Color = ScottPlot.Colors.Green;

        var scatterWindspeed = plt.Add.Scatter(xs, ysWindspeed);
        scatterWindspeed.LegendText = "Windspeed (m/s)";
        scatterWindspeed.LineWidth = 3;
        scatterWindspeed.Color = ScottPlot.Colors.Cyan;

        // Set title and labels
        plt.Title("Weather Data Over Time");
        plt.XLabel("Date");
        plt.YLabel("Value");

        // Enable the legend
        plt.ShowLegend();

        plt.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.DateTimeAutomatic();

        // Save the chart to a file
        plt.Save(savePath, imageWidth, imageHeight);
    }

    // Scatter plot for Temperature only
    static void GenerateTemperatureScatterChart(List<WeatherData> weatherData, string savePath)
    {
        var plt = new Plot();

        // converts the list of WeatherData objects into an array of double values
        double[] xs = weatherData.ConvertAll(d => d.Date.ToOADate()).ToArray();
        double[] ysTemperature = weatherData.ConvertAll(d => d.Temperature).ToArray();

        // Add scatter plot for Temperature
        var scatterTemp = plt.Add.Scatter(xs, ysTemperature);
        scatterTemp.LegendText = "Temperature (°C)";
        scatterTemp.LineWidth = 3;
        scatterTemp.Color = ScottPlot.Colors.Magenta;

        // Set title and labels
        plt.Title("Temperature Over Time");
        plt.XLabel("Date");
        plt.YLabel("Temperature (°C)");

        // Enable the legend
        plt.ShowLegend();

        plt.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.DateTimeAutomatic();

        // Save the chart to a file
        plt.Save(savePath, imageWidth, imageHeight);
    }

    // Generate bar chart for Temperature
    static void GenerateTemperatureBarChart(List<WeatherData> weatherData, string savePathTemperature)
    {
        var pltTemp = new Plot();

        double[] ysTemperature = weatherData.ConvertAll(d => d.Temperature).ToArray();
        double[] xs = Enumerable.Range(0, weatherData.Count).Select(i => (double)i).ToArray();

        // Add the temperature bars
        var barTemp = pltTemp.Add.Bars(ysTemperature);
        barTemp.Color = ScottPlot.Colors.Magenta;

        // Set x-axis labels (dates)
        var dateLabels = weatherData.ConvertAll(d => d.Date.ToString("dd/MM/yyyy HH:mm")).ToArray();
        pltTemp.Axes.Bottom.SetTicks(xs, dateLabels);
        pltTemp.Axes.Bottom.TickLabelStyle.Rotation = -45;

        // Set the title and labels
        pltTemp.Title("Temperature Over Time");
        pltTemp.XLabel("Time");
        pltTemp.YLabel("Temperature (°C)");

        // Show legend
        pltTemp.Legend.IsVisible = true;
        pltTemp.Legend.ManualItems.Add(new() { LabelText = "Temperature (°C)", FillColor = ScottPlot.Colors.Magenta });

        // Save the chart
        pltTemp.Save(savePathTemperature, imageWidth, imageHeight);
    }

    // Generate bar chart for Humidity
    static void GenerateHumidityBarChart(List<WeatherData> weatherData, string savePathHumidity)
    {
        var pltHumidity = new Plot();

        double[] ysHumidity = weatherData.ConvertAll(d => d.Humidity).ToArray();
        double[] xs = Enumerable.Range(0, weatherData.Count).Select(i => (double)i).ToArray();

        // Add the humidity bars
        var barHumidity = pltHumidity.Add.Bars(ysHumidity);
        barHumidity.Color = ScottPlot.Colors.Green;

        // Set x-axis labels (dates)
        var dateLabels = weatherData.ConvertAll(d => d.Date.ToString("dd/MM/yyyy HH:mm")).ToArray();
        pltHumidity.Axes.Bottom.SetTicks(xs, dateLabels);
        pltHumidity.Axes.Bottom.TickLabelStyle.Rotation = -45;

        // Set the title and labels
        pltHumidity.Title("Humidity Over Time");
        pltHumidity.XLabel("Time");
        pltHumidity.YLabel("Humidity (%)");

        // Show legend
        pltHumidity.Legend.IsVisible = true;
        pltHumidity.Legend.ManualItems.Add(new() { LabelText = "Humidity (%)", FillColor = ScottPlot.Colors.Green });

        // Save the chart
        pltHumidity.Save(savePathHumidity, imageWidth, imageHeight);
    }

    // Generate bar chart for Windspeed
    static void GenerateWindspeedBarChart(List<WeatherData> weatherData, string savePathWindspeed)
    {
        var pltWindspeed = new Plot();

        double[] ysWindspeed = weatherData.ConvertAll(d => d.Windspeed).ToArray();
        double[] xs = Enumerable.Range(0, weatherData.Count).Select(i => (double)i).ToArray();

        // Add the windspeed bars
        var barWindspeed = pltWindspeed.Add.Bars(ysWindspeed);
        barWindspeed.Color = ScottPlot.Colors.Cyan;

        // Set x-axis labels (dates)
        var dateLabels = weatherData.ConvertAll(d => d.Date.ToString("dd/MM/yyyy HH:mm")).ToArray();
        pltWindspeed.Axes.Bottom.SetTicks(xs, dateLabels);
        pltWindspeed.Axes.Bottom.TickLabelStyle.Rotation = -45;

        // Set the title and labels
        pltWindspeed.Title("Windspeed Over Time");
        pltWindspeed.XLabel("Time");
        pltWindspeed.YLabel("Windspeed (m/s)");

        // Show legend
        pltWindspeed.Legend.IsVisible = true;
        pltWindspeed.Legend.ManualItems.Add(new() { LabelText = "Windspeed (m/s)", FillColor = ScottPlot.Colors.Cyan });

        // Save the chart
        pltWindspeed.Save(savePathWindspeed, imageWidth, imageHeight);
    }
    // end of functions

    //***********************//

} // end of Program class


// data models representing the structure of the JSON response returned by the OpenWeatherMap API
public class WeatherData
{
    public DateTime Date { get; set; } // property can be accessed (read) and modified (written) from outside the class.
    public double Temperature { get; set; }
    public double Humidity { get; set; }
    public double Windspeed { get; set; }
    public string City { get; set; } // Added city property
}

public class WeatherApiResponse
{
    public List<WeatherApiItem> list { get; set; }
}

public class WeatherApiItem
{
    public MainData main { get; set; }
    public WindData wind { get; set; }
    public long dt { get; set; }
}

public class MainData
{
    public double temp { get; set; }
    public double humidity { get; set; }
}

public class WindData
{
    public double speed { get; set; }
}

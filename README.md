# Weather Data Console Application

## Framework:
- **C# .NET Core** (Console Application)
- **.NET Version**: 8.0
- **C# Version**: 12.0

## Package Dependencies:
- `Plotly.NET` Version: 5.1.0
- `ScottPlot` Version: 5.0.54
- `Serilog` Version: 4.2.0
- `Serilog.Sinks.Console` Version: 6.0.0
- `Serilog.Sinks.File` Version: 6.0.0
- `System.Data.SQLite` Version: 1.0.119

## Tested On:
- **Operating System**: Windows 11, 24H2
- **Date Tested**: 9/3/25

---

## Usage:
This console application fetches weather data from the **OpenWeatherMap** API, processes the data, generates a chart visualization using **ScottPlot**, and saves the resulting image locally.  
The application also logs data and stores it in an **SQLite database**.

### Outputs:
- **log.txt**: A log of the application’s execution.
- **Charts**: `.png` images of weather data visualizations.
- **SQLite Database**: Contains the fetched weather data.

---

## How to Run:

1. **API Key**:  
   In this script, enter your **API key** at `testingAPIKey`. For production, save your key as an environment variable on your machine as `OPENWEATHER_API_KEY`.

2. **Execute**:  
   Run the `ConsoleApp1.exe` program (ensure the 5 dependency files are in the same folder). Follow the instructions provided on the command line.

3. **Recompile the Source Code**:  
   You can recompile the source code on your machine with the following command:
   ```bash
   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true



![2](https://github.com/user-attachments/assets/3a75b4d4-1248-4ac7-8254-81036ef08e7d)


![3](https://github.com/user-attachments/assets/2bd7e2a3-e615-48b6-b63e-6229c550d2f6)


![db1](https://github.com/user-attachments/assets/1e90f43f-bd30-49af-ad85-540917b8722c)


![weather_chart_scatter_THW](https://github.com/user-attachments/assets/55dd0de9-2997-4dea-8bb4-8fec3c88797f)


![weather_chart_bar_temp](https://github.com/user-attachments/assets/5635e624-a38c-4391-a57c-030499c63a49)


![weather_chart_bar_hum](https://github.com/user-attachments/assets/d704193e-f4e5-4c69-a22d-8bbb51626e85)


![weather_chart_bar_wspeed](https://github.com/user-attachments/assets/738a0953-999e-495d-89c4-b0b04b55ab44)


![weather_chart_scatter_temp](https://github.com/user-attachments/assets/bdffdeb0-19fb-4dfb-9f35-ccb3a108f1c0)

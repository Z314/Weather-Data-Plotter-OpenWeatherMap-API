# Framework:
C# .NET core, console application
\n .NET (8.0)
C# (12.0)
Package dependencies:
Include="Plotly.NET" Version="5.1.0"
Include="ScottPlot" Version="5.0.54"
Include="Serilog" Version="4.2.0"
Include="Serilog.Sinks.Console" Version="6.0.0"
Include="Serilog.Sinks.File" Version="6.0.0"
Include="System.Data.SQLite" Version="1.0.119"
Tested on Windows 11, 24H2, 9/3/25

#-------------------------#

# Use:  
Fetches weather data from an external API-openweathermaps, processes the data, generates a chart visualisation using # a chart API-ScottPlot, and saves the resulting image locally.
Outputs: log.txt, charts (.png), SQLITE database with data

#-------------------------#

# To run:
In this script, enter your API key @ testingAPIKey OR for production save your key as a environment variable on your # machine as: OPENWEATHER_API_KEY
Execute the ConsoleApp1.exe program to run (5 dependency files are needed within same folder), and follow instruction given on the command line
Source code maybe re-compiled on your machine with: dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

#-------------------------#


![2](https://github.com/user-attachments/assets/3a75b4d4-1248-4ac7-8254-81036ef08e7d)


![3](https://github.com/user-attachments/assets/2bd7e2a3-e615-48b6-b63e-6229c550d2f6)


![db1](https://github.com/user-attachments/assets/1e90f43f-bd30-49af-ad85-540917b8722c)


![weather_chart_scatter_THW](https://github.com/user-attachments/assets/55dd0de9-2997-4dea-8bb4-8fec3c88797f)


![weather_chart_bar_temp](https://github.com/user-attachments/assets/5635e624-a38c-4391-a57c-030499c63a49)


![weather_chart_bar_hum](https://github.com/user-attachments/assets/d704193e-f4e5-4c69-a22d-8bbb51626e85)


![weather_chart_bar_wspeed](https://github.com/user-attachments/assets/738a0953-999e-495d-89c4-b0b04b55ab44)


![weather_chart_scatter_temp](https://github.com/user-attachments/assets/bdffdeb0-19fb-4dfb-9f35-ccb3a108f1c0)

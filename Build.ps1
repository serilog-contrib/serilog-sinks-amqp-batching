if(Test-Path .\artifacts) { Remove-Item .\artifacts -Force -Recurse }

dotnet restore .\src\Serilog.Sinks.AMQP.Batching\Serilog.Sinks.AMQP.Batching.csproj
dotnet build .\src\Serilog.Sinks.AMQP.Batching\Serilog.Sinks.AMQP.Batching.csproj

echo "RUNNING dotnet pack .\src\Serilog.Sinks.AMQP.Batching\Serilog.Sinks.AMQP.Batching.csproj -c Release -o .\artifacts"
dotnet pack .\src\Serilog.Sinks.AMQP.Batching\Serilog.Sinks.AMQP.Batching.csproj -c Release -o .\artifacts

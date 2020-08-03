if(Test-Path .\artifacts) { Remove-Item .\artifacts -Force -Recurse }

dotnet restore .\src\Serilog.Sinks.AMQP\Serilog.Sinks.AMQP.csproj
dotnet build .\src\Serilog.Sinks.AMQP\Serilog.Sinks.AMQP.csproj

echo "RUNNING dotnet pack .\src\Serilog.Sinks.AMQP\Serilog.Sinks.AMQP.csproj -c Release -o .\artifacts"
dotnet pack .\src\Serilog.Sinks.AMQP\Serilog.Sinks.AMQP.csproj -c Release -o .\artifacts

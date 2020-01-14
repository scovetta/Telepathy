Set-Location -Path .\
Start-Process cmd.exe -ArgumentList "/K dotnet run -p SchedulerServer 50051"
Start-Process cmd.exe -ArgumentList "/K dotnet run -p SchedulerServer 50052"
Start-Process cmd.exe -ArgumentList "/K dotnet run -p SchedulerServer 50053"
Start-Process cmd.exe -ArgumentList "/K dotnet run -p WorkerServer 50054"
Start-Process cmd.exe -ArgumentList "/K dotnet run -p WorkerServer 50055"
Start-Process cmd.exe -ArgumentList "/K dotnet run -p WorkerServer 50056"
Start-Process cmd.exe -ArgumentList "/K dotnet run -p WorkerServer 50057"
Start-Process cmd.exe -ArgumentList "/K dotnet run -p WorkerServer 50058"

Start-Sleep -s 5

$max_iterations = 3;

for ($i=1; $i -le $max_iterations; $i++)
{
	$proc = Start-Process cmd.exe -ArgumentList "/K dotnet run -p SchedulerClient & exit" -NoNewWindow -PassThru
	$proc.WaitForExit()
}
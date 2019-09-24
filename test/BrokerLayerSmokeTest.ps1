$env:REPO_ROOT=(Split-Path $PSScriptRoot -Parent) 
$svchost = Start-Process "$env:REPO_ROOT\src\soa\CcpServiceHost\bin\Release\CcpServiceHost.exe" -ArgumentList "-standalone" -PassThru
$brkLauncher = Start-Process "$env:REPO_ROOT\src\soa\BrokerOutput\Release\HpcBroker.exe" -ArgumentList "-d -r $env:REPO_ROOT\test\registration --EnableAzureStorageQueueEndpoint False" -PassThru
&"$env:REPO_ROOT\src\soa\EchoClient\bin\Release\EchoClient.exe" -isnosession -regpath "$env:REPO_ROOT\test\registration" -targetlist 127.0.0.1 -h 127.0.0.1
Stop-Process -Id $svchost.Id
Stop-Process -Id $brkLauncher.Id
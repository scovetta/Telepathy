$env:REPO_ROOT=(Split-Path $PSScriptRoot -Parent) 
$svchost = Start-Process "$env:REPO_ROOT\src\soa\CcpServiceHost\bin\Debug\CcpServiceHost.exe" -ArgumentList "-standalone" -PassThru
$brkLauncher = Start-Process "$env:REPO_ROOT\src\soa\BrokerOutput\Debug\HpcBroker.exe" -ArgumentList "-d -CCP_SERVICEREGISTRATION_PATH $env:REPO_ROOT\test\registration -EnableAzureStorageQueueEndpoint False" -PassThru
&"$env:REPO_ROOT\src\soa\EchoClient\bin\Debug\EchoClient.exe" -isnosession -regpath "$env:REPO_ROOT\test\registration" -targetlist 127.0.0.1
Stop-Process -Id $svchost.Id
Stop-Process -Id $brkLauncher.Id
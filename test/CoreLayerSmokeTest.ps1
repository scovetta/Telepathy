$env:REPO_ROOT=(Split-Path $PSScriptRoot -Parent) 
$svchost = Start-Process "$env:REPO_ROOT\src\soa\CcpServiceHost\bin\Release\CcpServiceHost.exe" -ArgumentList "-standalone" -PassThru
&"$env:REPO_ROOT\src\soa\EchoClient\bin\Release\EchoClient.exe" -inprocess -isnosession -regpath "$env:REPO_ROOT\test\registration" -targetlist 127.0.0.1
Stop-Process -Id $svchost.Id
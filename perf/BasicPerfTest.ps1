$env:REPO_ROOT=(Split-Path $PSScriptRoot -Parent) 
$svchost = Start-Process "$env:REPO_ROOT\src\soa\CcpServiceHost\bin\Debug\CcpServiceHost.exe" -ArgumentList "-standalone" -PassThru
Start-Sleep 2
&"$env:REPO_ROOT\perf\TestClient\bin\Debug\TestClient.exe" -regpath "$env:REPO_ROOT\perf\registration"  -m 1 -min 1 -n 10 -r 0  -batch 1 -responsehandler -save "CTQ"
Stop-Process -Id $svchost.Id
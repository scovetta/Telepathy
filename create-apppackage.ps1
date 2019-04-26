$env:REPO_ROOT=$PSScriptRoot
if (Test-Path "$env:REPO_ROOT\artifacts\AzureBatchAppPackage"){
	Remove-Item -Recurse -Force "$env:REPO_ROOT\artifacts\AzureBatchAppPackage"
}
Copy-Item "$env:REPO_ROOT\src\soa\BrokerOutput\Debug" -Destination "$env:REPO_ROOT\artifacts\AzureBatchAppPackage\BrokerOutput" -Recurse
Copy-Item "$env:REPO_ROOT\src\soa\CcpServiceHost\bin\Debug" -Destination "$env:REPO_ROOT\artifacts\AzureBatchAppPackage\CcpServiceHost" -Recurse
Copy-Item "$env:REPO_ROOT\src\soa\EchoSvcLib\bin\Debug" -Destination "$env:REPO_ROOT\artifacts\AzureBatchAppPackage\EchoSvcLib" -Recurse
Copy-Item "$env:REPO_ROOT\src\soa\SessionLauncher\bin\Debug" -Destination "$env:REPO_ROOT\artifacts\AzureBatchAppPackage\SessionLauncher" -Recurse
Copy-Item "$env:REPO_ROOT\samples\batch\EchoSvcSample\Registration" -Destination "$env:REPO_ROOT\artifacts\AzureBatchAppPackage\Registration" -Recurse
$env:REPO_ROOT=$PSScriptRoot
if (Test-Path "$env:REPO_ROOT\artifacts"){
	Remove-Item -Recurse -Force "$env:REPO_ROOT\artifacts"
}
Copy-Item "$env:REPO_ROOT\src\soa\BrokerOutput\Debug" -Destination "$env:REPO_ROOT\artifacts\BrokerOutput" -Recurse
Copy-Item "$env:REPO_ROOT\src\soa\CcpServiceHost\bin\Debug" -Destination "$env:REPO_ROOT\artifacts\CcpServiceHost" -Recurse
Copy-Item "$env:REPO_ROOT\src\soa\EchoSvcLib\bin\Debug" -Destination "$env:REPO_ROOT\artifacts\EchoSvcLib" -Recurse
Copy-Item "$env:REPO_ROOT\src\soa\SessionLauncher\bin\Debug" -Destination "$env:REPO_ROOT\artifacts\SessionLauncher" -Recurse
Copy-Item "$env:REPO_ROOT\samples\batch\EchoSvcSample\Registration" -Destination "$env:REPO_ROOT\artifacts\Registration" -Recurse
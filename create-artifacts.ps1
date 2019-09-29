param($Configuration)
$env:REPO_ROOT = $PSScriptRoot
if (Test-Path "$env:REPO_ROOT\artifacts") {
    Remove-Item -Recurse -Force "$env:REPO_ROOT\artifacts"
}

if ($Configuration -eq 'Debug') {
    Copy-Item "$env:REPO_ROOT\src\soa\BrokerOutput\Debug" -Destination "$env:REPO_ROOT\artifacts\Debug\BrokerOutput" -Recurse
    Copy-Item "$env:REPO_ROOT\src\soa\CcpServiceHost\bin\Debug" -Destination "$env:REPO_ROOT\artifacts\Debug\CcpServiceHost" -Recurse
    Copy-Item "$env:REPO_ROOT\src\soa\EchoSvcLib\bin\Debug" -Destination "$env:REPO_ROOT\artifacts\Debug\EchoSvcLib" -Recurse
    Copy-Item "$env:REPO_ROOT\src\soa\SessionLauncher\bin\Debug" -Destination "$env:REPO_ROOT\artifacts\Debug\SessionLauncher" -Recurse
    Copy-Item "$env:REPO_ROOT\batch\Registration" -Destination "$env:REPO_ROOT\artifacts\Debug\Registration" -Recurse
}
else {
    Copy-Item "$env:REPO_ROOT\src\soa\BrokerOutput\Release" -Destination "$env:REPO_ROOT\artifacts\Release\BrokerOutput" -Recurse
    Copy-Item "$env:REPO_ROOT\src\soa\CcpServiceHost\bin\Release" -Destination "$env:REPO_ROOT\artifacts\Release\CcpServiceHost" -Recurse
    Copy-Item "$env:REPO_ROOT\src\soa\EchoSvcLib\bin\Release" -Destination "$env:REPO_ROOT\artifacts\Release\EchoSvcLib" -Recurse
    Copy-Item "$env:REPO_ROOT\src\soa\SessionLauncher\bin\Release" -Destination "$env:REPO_ROOT\artifacts\Release\SessionLauncher" -Recurse
    Copy-Item "$env:REPO_ROOT\batch\Registration" -Destination "$env:REPO_ROOT\artifacts\Release\Registration" -Recurse
    Copy-Item "$env:REPO_ROOT\deploy\EnableTelepathyStorage.ps1" -Destination "$env:REPO_ROOT\artifacts\Release"
    Copy-Item "$env:REPO_ROOT\deploy\StartTelepathyService.ps1" -Destination "$env:REPO_ROOT\artifacts\Release"
    Copy-Item "$env:REPO_ROOT\deploy\StartBroker.ps1" -Destination "$env:REPO_ROOT\artifacts\Release"
}
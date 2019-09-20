param (
    [string]$DestinationPath,
    [string]$DesStorageConnectionString,
    [string]$BatchAccountName,
    [string]$BatchPoolName,
    [string]$BatchAccountKey,
    [string]$BatchAccountServiceUrl
)

cmd /c "sc.exe config NetTcpPortSharing start=demand & reg ADD "HKLM\Software\Microsoft\StrongName\Verification\*,*" /f & reg ADD "HKLM\Software\Wow6432Node\Microsoft\StrongName\Verification\*,*^" /f"

$sessionLauncher = "$DestinationPath\SessionLauncher\HpcSession.exe"
$broker = "$DestinationPath\BrokerOutput\HpcBroker.exe"
$serviceName = "TelepathySessionLauncher"
New-Service -Name $serviceName `
 -BinaryPathName "$sessionLauncher --AzureBatchServiceUrl $BatchAccountServiceUrl --AzureBatchAccountName $BatchAccountName --AzureBatchAccountKey $BatchAccountkey --AzureBatchPoolName $BatchPoolName --AzureBatchBrokerStorageConnectionString $DesStorageConnectionString --BrokerLauncherExePath $broker" `
 -DisplayName "Telepathy Session Launcher Service" `
 -StartupType Automatic `
 -Description "Telepathy Session Launcher service." 

Start-Service -Name $serviceName
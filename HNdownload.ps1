param([string] $version, [string] $connectString)

$ConName = "telepathy"

$Ctx = New-AzStorageContext -ConnectionString $connectString

$SessionPrefix = $version + "/Release/SessionLauncher"
$BrokerPrefix = $version + "/Release/BrokerOutput"

$List = Get-AzStorageBlob -prefix $SessionPrefix -Container $ConName -Context $Ctx

$List = $List.name
foreach ( $l in $list ){
    Write-Output "Downloading $l!"
    Get-AzStorageBlobContent -Blob $l -Container $conname -Context $ctx -Destination "C:\"
}

  

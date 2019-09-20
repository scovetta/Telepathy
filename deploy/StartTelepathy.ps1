<# Custom Script for Windows to install a file from Azure Storage using the staging folder created by the deployment script #>
param (
    [switch]$EnableTelepathyStorage,
    [switch]$StartSessionLauncher,
    [string]$Location,
    [string]$BatchAccountName,
    [string]$BatchPoolName,
    [string]$ArtifactsFolderName,
    [string]$ContainerName,
    [string]$SrcStorageAccountName,
    [string]$SrcStorageContainerSasToken,
    [string]$DesStorageAccountName,
    [string]$DesStorageAccountKey,
    [string]$BatchAccountKey
)
Install-PackageProvider -Name NuGet -MinimumVersion 2.8.5.201 -Force
Install-Module -Name Az -AllowClobber -Force
$destination_path = "C:\telepathy"
$srcStorageContext = New-AzStorageContext -StorageAccountName $SrcStorageAccountName -SasToken $SrcStorageContainerSasToken
$blobs = Get-AzStorageBlob -Container $ContainerName -Blob "$ArtifactsFolderName*" -Context $srcStorageContext
foreach($blob in $blobs) {  
    New-Item -ItemType Directory -Force -Path $destination_path  
    Get-AzStorageBlobContent -Container $ContainerName -Blob $blob.Name -Destination $destination_path -Context $srcStorageContext   
} 

$artifactsPath = "$destination_path\$ArtifactsFolderName\Release"
$desStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=$DesStorageAccountName;AccountKey=$DesStorageAccountKey;EndpointSuffix=core.windows.net"
$batchServiceUrl = "https://$BatchAccountName.$Location.batch.azure.com"
if($EnableTelepathyStorage) {
    invoke-expression "$artifactsPath\EnableTelepathyStorage.ps1 -DestinationPath $artifactsPath -DesStorageConnectionString '$DesStorageConnectionString'"
}

if($StartSessionLauncher) {
    invoke-expression "$artifactsPath\StartSessionLauncher.ps1 -DestinationPath $artifactsPath -DesStorageConnectionString '$DesStorageConnectionString' -BatchAccountName $BatchAccountName -BatchPoolName $BatchPoolName -BatchAccountKey $BatchAccountKey -BatchAccountServiceUrl $batchServiceUrl"
}
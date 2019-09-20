param (
    [string]$DestinationPath,
    [string]$DesStorageConnectionString
)

$desStorageContext = New-AzStorageContext -ConnectionString $DesStorageConnectionString

"runtime service-assembly service-registration".split() | New-AzStorageContainer -Context $desStorageContext

function uploadFiles{
    param($LocalPath, $RemotePath, $ContainerName, $StorageContext)
    $files = Get-childItem $localPath
    foreach($file in $files)
    {
        $localFile = "$localPath\$file"
        if($RemotePath) {
            $remoteFile= "$remotePath/$file"
        }
        else {
            $remoteFile= "$file"
        }
        
        if(-not $file.PSIsContainer) {
            Set-AzStorageBlobContent -File $localFile -Blob $remoteFile -Container $ContainerName -Context $StorageContext -Force
        }
        else {
            uploadFiles -LocalPath $localFile -RemotePath $remoteFile -ContainerName $containerName -StorageContext $storageContext 
        }
        
    }
}

uploadFiles -LocalPath "$DestinationPath\CcpServiceHost" -RemotePath "ccpservicehost" -ContainerName "runtime" -StorageContext $desStorageContext
uploadFiles -LocalPath "$DestinationPath\EchoSvcLib" -RemotePath "ccpechosvc" -ContainerName "service-assembly" -StorageContext $desStorageContext
uploadFiles -LocalPath "$DestinationPath\Registration"  -ContainerName "service-registration" -StorageContext $desStorageContext
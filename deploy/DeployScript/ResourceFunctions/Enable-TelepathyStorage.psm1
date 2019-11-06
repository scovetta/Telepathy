function Enable-TelepathyStorage {
    param (
        [string]$ArtifactsPath,
        [string]$DesStorageConnectionString
    )

    function uploadFiles {
        param($LocalPath, $RemotePath, $ContainerName, $StorageContext)
        $files = Get-childItem $localPath
        foreach ($file in $files) {
            $localFile = "$localPath\$file"
            if ($RemotePath) {
                $remoteFile = "$remotePath/$file"
            }
            else {
                $remoteFile = "$file"
            }
        
            if (-not $file.PSIsContainer) {
                Set-AzStorageBlobContent -File $localFile -Blob $remoteFile -Container $ContainerName -Context $StorageContext -Force
            }
            else {
                uploadFiles -LocalPath $localFile -RemotePath $remoteFile -ContainerName $containerName -StorageContext $storageContext 
            }
        
        }
    }

    Write-Log -Message "Files to upload in path : $ArtifactsPath"
    Write-Log -Message "DesStorageConnectionString : $DesStorageConnectionString"

    Try {
        $desStorageContext = New-AzStorageContext -ConnectionString $DesStorageConnectionString
    }
    Catch {
        Write-Log -Message "Error when get destination storage context" -Level Error
        Write-Log -Message $_ -Level Error
    }

    Try {
        "runtime service-assembly service-registration".split() | New-AzStorageContainer -Context $desStorageContext
    }
    Catch {
        Write-Log -Message "Error when new storage container" -Level Error
        Write-Log -Message $_ -Level Error
    }

    Try {
        uploadFiles -LocalPath "$ArtifactsPath\CcpServiceHost" -RemotePath "ccpservicehost" -ContainerName "runtime" -StorageContext $desStorageContext
        uploadFiles -LocalPath "$ArtifactsPath\EchoSvcLib" -RemotePath "ccpechosvc" -ContainerName "service-assembly" -StorageContext $desStorageContext
        uploadFiles -LocalPath "$ArtifactsPath\Registration"  -ContainerName "service-registration" -StorageContext $desStorageContext
    }
    Catch {
        Write-Log -Message "Error when upload files to storage" -Level Error
        Write-Log -Message $_ -Level Error
    }
}
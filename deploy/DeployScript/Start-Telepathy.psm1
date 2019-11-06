function Start-Telepathy {
    param (
        [switch]$EnableTelepathyStorage,
        [switch]$StartTelepathyService,
        [string]$Location,
        [string]$BatchAccountName,
        [string]$BatchPoolName,
        [string]$ArtifactsFolderName,
        [string]$ContainerName,
        [string]$SrcStorageAccountName,
        [string]$DesStorageAccountName,
        [string]$SrcStorageContainerSasToken,
        [string]$DesStorageAccountKey,
        [string]$BatchAccountKey,
        [string]$EnableLogAnalytics,
        [string]$WorkspaceId,
        [string]$AuthenticationId
    )
    
    Install-PackageProvider -Name NuGet -MinimumVersion 2.8.5.201 -Force
    Install-Module -Name Az -AllowClobber -Force
    
    $destination_path = "C:\telepathy"
    $artifactsPath = "$destination_path\$ArtifactsFolderName\Release"
  
    Set-LogSource -SourceName "StartTelepathy"

    Try {
        Write-Log -Message "StorageAccountName : $SrcStorageAccountName"
        Write-Log -Message "StorageSasToken : $SrcStorageContainerSasToken"
        $srcStorageContext = New-AzStorageContext -StorageAccountName $SrcStorageAccountName -SasToken $SrcStorageContainerSasToken  
    }
    Catch {
        Write-Log -Message "Please provide valid storage account name and sas token" -Level Error
        Write-Log -Message $_ -Level Error
    }
    
    Try {
        Write-Log -Message "ContainerName : $ContainerName"
        Write-Log -Message "ArtifactsFolderName : $ArtifactsFolderName"
        $blobs = Get-AzStorageBlob -Container $ContainerName -Blob "$ArtifactsFolderName*" -Context $srcStorageContext
    }
    Catch {
        Write-Log -Message "Error occurs when get source storage blob, can't get storage blob, please confirm you provide valid container name, blob name and storage context " -Level Error
        Write-Log -Message $_ -Level Error
    }
    
    Try {
        Write-Log -Message "DestinationPath in VM : $destination_path"
        foreach ($blob in $blobs) {  
            New-Item -ItemType Directory -Force -Path $destination_path  
            Get-AzStorageBlobContent -Container $ContainerName -Blob $blob.Name -Destination $destination_path -Context $srcStorageContext   
        } 
    }
    Catch {
        Write-Log -Message "Error occurs when download source storage blob to VM " -Level Error
        Write-Log -Message $_ -Level Error
    }
    #>
    
    $desStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=$DesStorageAccountName;AccountKey=$DesStorageAccountKey;EndpointSuffix=core.windows.net"
    $batchServiceUrl = "https://$BatchAccountName.$Location.batch.azure.com"
    
    Write-Log -Message "Artifacts path: $artifactsPath"
    Write-Log -Message "desStorageConnectionString: $desStorageConnectionString"
    Write-Log -Message "batchServiceUrl: $batchServiceUrl"
    
    if($EnableTelepathyStorage) {
        Write-Log -Message "Enable Telepathy Storage"
        Enable-TelepathyStorage -ArtifactsPath $artifactsPath -DesStorageConnectionString $DesStorageConnectionString
    }
    
    if ($StartTelepathyService) {
        Write-Log -Message "EnableLogAnalytics: $EnableLogAnalytics"
        Write-Log -Message "WorkspaceId: $WorkspaceId"
        Write-Log -Message "AuthenticationId: $AuthenticationId"
        $expression = @{ 
            DestinationPath            = $artifactsPath
            DesStorageConnectionString = $DesStorageConnectionString
            BatchAccountName           = $BatchAccountName
            BatchPoolName              = $BatchPoolName
            BatchAccountKey            = $BatchAccountKey
            BatchAccountServiceUrl     = $batchServiceUrl
        }
        if ($EnableLogAnalytics -eq "enable") {
            Write-Log -Message "Enable Azure Log Analytics"
            $logConfig = @{
                EnableLogAnalytics = $true
                WorkspaceId        = $WorkspaceId
                AuthenticationId   = $AuthenticationId;
            }
            $expression = $expression + $logConfig
        }
        Start-TelepathyService @expression
    }
}

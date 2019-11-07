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
    
    $desStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=$DesStorageAccountName;AccountKey=$DesStorageAccountKey;EndpointSuffix=core.windows.net"
    $batchServiceUrl = "https://$BatchAccountName.$Location.batch.azure.com"
    
    Write-Log -Message "Artifacts path: $artifactsPath"
    Write-Log -Message "desStorageConnectionString: $desStorageConnectionString"
    Write-Log -Message "batchServiceUrl: $batchServiceUrl"
    
    if ($EnableTelepathyStorage) {
        Write-Log -Message "Enable Telepathy Storage"
        Enable-TelepathyStorage -ArtifactsPath $artifactsPath -DesStorageConnectionString $desStorageConnectionString
    }
    
    if ($StartTelepathyService) {
        Write-Log -Message "EnableLogAnalytics: $EnableLogAnalytics"
        Write-Log -Message "WorkspaceId: $WorkspaceId"
        Write-Log -Message "AuthenticationId: $AuthenticationId"
        $expression = @{ 
            DestinationPath            = $artifactsPath
            DesStorageConnectionString = $desStorageConnectionString
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

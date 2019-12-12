function Start-Telepathy {
    param (
        [switch]$EnableTelepathyStorage,
        [switch]$StartTelepathyService,
        [string]$BatchAccountName,
        [string]$BatchPoolName,
        [string]$ArtifactsFolderName,
        [string]$ContainerName,
        [string]$SrcStorageAccountName,
        [string]$DesStorageAccountName,
        [string]$SrcStorageContainerSasToken,
        [string]$DesStorageAccountKey,
        [string]$BatchAccountKey,
        [string]$BatchServiceUrl,
        [string]$EnableLogAnalytics,
        [string]$WorkspaceId,
        [string]$AuthenticationId
    )
    
    $destination_path = "C:\telepathy"
    $artifactsPath = "$destination_path\$ArtifactsFolderName\Release"
  
    Write-Log -Message "Set log source name as StartTelepathy"
    Set-LogSource -SourceName "StartTelepathy"
    
    Write-Log -Message "Get desStorageConnectionString"
    $desStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=$DesStorageAccountName;AccountKey=$DesStorageAccountKey;EndpointSuffix=core.windows.net"
    
    Write-Log -Message "Artifacts path: $artifactsPath"
    
    if ($EnableTelepathyStorage) {
        Write-Log -Message "Enable Telepathy Storage"
        Enable-TelepathyStorage -ArtifactsPath $artifactsPath -DesStorageConnectionString $desStorageConnectionString
    }
    
    if ($StartTelepathyService) {
        Write-Log -Message "EnableLogAnalytics: $EnableLogAnalytics"
        $expression = @{ 
            DestinationPath            = $artifactsPath
            DesStorageConnectionString = $desStorageConnectionString
            BatchAccountName           = $BatchAccountName
            BatchPoolName              = $BatchPoolName
            BatchAccountKey            = $BatchAccountKey
            BatchAccountServiceUrl     = $BatchServiceUrl
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
        Write-Log -Message "Start Telepathy Service with hashtable parameters"
        Start-TelepathyService @expression
    }
}

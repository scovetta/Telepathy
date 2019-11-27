function Start-SessionLauncher {
    param (
        [string]$DestinationPath,
        [string]$DesStorageConnectionString,
        [string]$BatchAccountName,
        [string]$BatchPoolName,
        [string]$BatchAccountKey,
        [string]$BatchAccountServiceUrl,
        [switch]$EnableLogAnalytics,
        [string]$WorkspaceId,
        [string]$AuthenticationId
    )

    Set-LogSource -SourceName "StartSessionLauncher"
    
    Write-Log -Message "DestinationPath to find resource : $DestinationPath"
    Write-Log -Message "BatchAccountName : $BatchAccountName"
    Write-Log -Message "BatchPoolName: $BatchPoolName"
    
    $serviceName = "TelepathySessionLauncher"
    $SessionLauncher = "$DestinationPath\SessionLauncher\HpcSession.exe"
    $SessionLauncherPath = "$DestinationPath\SessionLauncher"
    
    Try {
        Write-Log -Message "Add SessionLauncher in PATH environment varaible"
        $env:path = $env:path + ";$SessionLauncherPath"
        [System.Environment]::SetEnvironmentVariable("PATH", $env:path, "Machine")
    
        if ($EnableLogAnalytics) {
            $LoggingLevel = "Warning"
            Write-Log -Message "Start to config log analytics in SessionLauncher"
            Invoke-Expression "$SessionLauncher -l --Logging `"Enable`" --AzureAnalyticsLogging true --AzureAnalyticsLoggingLevel $LoggingLevel --AzureAnalyticsWorkspaceId $WorkspaceId --AzureAnalyticsAuthenticationId $AuthenticationId"
        }
        
        Write-Log -Message "Start to new session launcher windows service"
        New-Service -Name $serviceName `
            -BinaryPathName "$SessionLauncher --AzureBatchServiceUrl $BatchAccountServiceUrl --AzureBatchAccountName $BatchAccountName --AzureBatchAccountKey $BatchAccountkey --AzureBatchPoolName $BatchPoolName --AzureBatchBrokerStorageConnectionString $DesStorageConnectionString" `
            -DisplayName "Telepathy Session Launcher Service" `
            -StartupType Automatic `
            -Description "Telepathy Session Launcher service." 
    }
    Catch {
        Write-Log -Message "Error happens when new session launcher windows service" -Level Error
        Write-Log -Message $_ -Level Error
    }
    Try {
        Write-Log -Message "Start session launcher windows service"
        Start-Service -Name $serviceName
    }
    Catch {
        Write-Log -Message "Fail to start session launcher windows service" -Level Error
        Write-Log -Message $_ -Level Error
    }
}
Export-ModuleMember -Function Start-SessionLauncher
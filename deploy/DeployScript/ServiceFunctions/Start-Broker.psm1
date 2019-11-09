function Start-Broker {
    param (
        [string]$DestinationPath,
        [string]$SessionAddress,
        [switch]$EnableLogAnalytics,
        [string]$WorkspaceId,
        [string]$AuthenticationId
    )

    Set-LogSource -SourceName "StartBroker"

    Write-Log -Message "DestinationPath to find resource : $DestinationPath"
    Write-Log -Message "Session Address: $SessionAddress"
    
    $serviceName = "TelepathyBroker"
    $BrokerOutputPath = "$DestinationPath\BrokerOutput"
    $Broker = "$BrokerOutputPath\HpcBroker.exe"
    $BrokerWorker = "$BrokerOutputPath\HpcBrokerWorker.exe"
    
    Try {
        Write-Log -Message "Add BrokerOutput in PATH environment varaible"
        $env:path = $env:path + ";$BrokerOutputPath"
        [System.Environment]::SetEnvironmentVariable("PATH", $env:path, "Machine")
    
        if ($EnableLogAnalytics) {
            $LoggingLevel = "Warning"
            Write-Log -Message "Start to config log analytics in Broker"
            Invoke-Expression "$Broker -l --Logging `"Enable`" --AzureAnalyticsLogging true --AzureAnalyticsLoggingLevel $LoggingLevel --AzureAnalyticsWorkspaceId $WorkspaceId --AzureAnalyticsAuthenticationId $AuthenticationId"
            Write-Log -Message "Start to config log analytics in BrokerWorker"
            Invoke-Expression "$BrokerWorker -l --Logging `"Enable`" --AzureAnalyticsLogging true --AzureAnalyticsLoggingLevel $LoggingLevel --AzureAnalyticsWorkspaceId $WorkspaceId --AzureAnalyticsAuthenticationId $AuthenticationId"
        }
    
        Write-Log -Message "Start to new broker windows service"
        New-Service -Name $serviceName `
            -BinaryPathName "$Broker --SessionAddress $SessionAddress" `
            -DisplayName "Telepathy Broker Service" `
            -StartupType Automatic `
            -Description "Telepathy Broker service." 
    }
    Catch {
        Write-Log -Message "Error happens when new broker windows service" -Level Error
        Write-Log -Message $_ -Level Error
    }
    
    Try {
        Write-Log -Message "Start broker windows service"
        Start-Service -Name $serviceName
    }
    Catch {
        Write-Log -Message "Fail to start broker windows service" -Level Error
        Write-Log -Message $_ -Level Error
    }
}
Export-ModuleMember -Function Start-Broker
param (
    [string]$Broker,
    [string]$BrokerWorker,
    [string]$SessionAddress,
    [switch]$EnableLogAnalytics,
    [string]$WorkspaceId,
    [string]$AuthenticationId
)

function Write-Log 
{ 
    [CmdletBinding()] 
    Param 
    ( 
        [Parameter(Mandatory=$true, 
                   ValueFromPipelineByPropertyName=$true)] 
        [ValidateNotNullOrEmpty()] 
        [Alias("LogContent")] 
        [string]$Message, 
 
        [Parameter(Mandatory=$false)] 
        [Alias('LogPath')] 
        [string]$Path='C:\Logs\telepathy.log', 
         
        [Parameter(Mandatory=$false)] 
        [ValidateSet("Error","Warn","Info")] 
        [string]$Level="Info", 
         
        [Parameter(Mandatory=$false)] 
        [switch]$NoClobber 
    ) 
 
    Begin 
    { 
        # Set VerbosePreference to Continue so that verbose messages are displayed. 
        $VerbosePreference = 'Continue' 
    } 
    Process 
    { 
         
        # If the file already exists and NoClobber was specified, do not write to the log. 
        if ((Test-Path $Path) -AND $NoClobber) { 
            Write-Error "Log file $Path already exists, and you specified NoClobber. Either delete the file or specify a different name." 
            Return 
            } 
 
        # If attempting to write to a log file in a folder/path that doesn't exist create the file including the path. 
        elseif (!(Test-Path $Path)) { 
            Write-Verbose "Creating $Path." 
            $NewLogFile = New-Item $Path -Force -ItemType File 
            } 
 
        else { 
            # Nothing to see here yet. 
            } 
 
        # Format Date for our Log File 
        $FormattedDate = Get-Date -Format "yyyy-MM-dd HH:mm:ss" 
 
        # Write message to error, warning, or verbose pipeline and specify $LevelText 
        switch ($Level) { 
            'Error' { 
                Write-Error $Message 
                $LevelText = 'ERROR:' 
                } 
            'Warn' { 
                Write-Warning $Message 
                $LevelText = 'WARNING:' 
                } 
            'Info' { 
                Write-Verbose $Message 
                $LevelText = 'INFO:' 
                } 
            } 
         
        # Write log entry to $Path 
        "$FormattedDate $LevelText [StartBroker] $Message" | Out-File -FilePath $Path -Append 
    } 
    End 
    { 
    } 
}

Write-Log -Message "DestinationPath to find resource : $DestinationPath"
Write-Log -Message "Session Address: $SessionAddress"

$serviceName = "TelepathyBroker"

Try {
    Write-Log -Message "set Broker environment varaible in session machine"
    cmd /c "setx /m Broker $Broker"

    Write-Log -Message "set BrokerWorker environment varaible in session machine"
    cmd /c "setx /m BrokerWorker $BrokerWorker"

    if($EnableLogAnalytics)
    {
        Write-Log -Message "Start to config log analytics"
        Invoke-Expression "$Broker -l --AzureAnalyticsLogging true --AzureAnalyticsLoggingLevel 'Warning' --AzureAnalyticsWorkspaceId $WorkspaceId --AzureAnalyticsAuthenticationId $AuthenticationId"
        Invoke-Expression "$BrokerWorker -l --AzureAnalyticsLogging true --AzureAnalyticsLoggingLevel 'Warning' --AzureAnalyticsWorkspaceId $WorkspaceId --AzureAnalyticsAuthenticationId --AuthenticationId"
    }

    Write-Log -Message "Start to new broker windows service"
    New-Service -Name $serviceName `
    -BinaryPathName "$Broker --SessionAddress $SessionAddress" `
    -DisplayName "Telepathy Broker Service" `
    -StartupType Automatic `
    -Description "Telepathy Broker service." 
} Catch {
    Write-Log -Message "Error happens when new broker windows service" -Level Error
    Write-Log -Message $_ -Level Error
}

Try {
    Write-Log -Message "Start broker windows service"
    Start-Service -Name $serviceName
} Catch {
    Write-Log -Message "Fail to start broker windows service" -Level Error
    Write-Log -Message $_ -Level Error
}
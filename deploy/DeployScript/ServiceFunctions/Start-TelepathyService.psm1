function Start-TelepathyService {
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

    Set-LogSource -SourceName "StartTelepathyService"

    Write-Log -Message "Start open NetTCPPortSharing & enable StrongName"
    cmd /c "sc.exe config NetTcpPortSharing start=demand & reg ADD "HKLM\Software\Microsoft\StrongName\Verification\*,*" /f & reg ADD "HKLM\Software\Wow6432Node\Microsoft\StrongName\Verification\*,*^" /f"

    Write-Log -Message "set TELEPATHY_SERVICE_REGISTRATION_WORKING_DIR environment varaibles in session machine"
    cmd /c "setx /m TELEPATHY_SERVICE_REGISTRATION_WORKING_DIR ^"C:\TelepathyServiceRegistration\^""

    Write-Log -Message "Open tcp port"
    New-NetFirewallRule -DisplayName "Open TCP port for telepathy" -Direction Inbound -LocalPort 9087, 9090, 9091, 9092, 9093 -Protocol TCP -Action Allow

    Write-Log -Message "Script location path: $DestinationPath"
    write-Log -Message "DesStorageConnectionString: $DesStorageConnectionString"
    write-Log -Message "BatchAccountName: $BatchAccountName"
    Write-Log -Message "BatchPoolName: $BatchPoolName"
    Write-Log -Message "BatchAccountKey: $BatchAccountKey"
    Write-Log -Message "BatchAccountServiceUrl: $BatchAccountServiceUrl"

    Try {
        Write-Log -Message "Start session launcher"
        $sessionLauncherExpression = @{
            DestinationPath = $DestinationPath
            DesStorageConnectionString = $DesStorageConnectionString 
            BatchAccountName = $BatchAccountName 
            BatchPoolName = $BatchPoolName
            BatchAccountKey = $BatchAccountKey
            BatchAccountServiceUrl = $BatchAccountServiceUrl
        }
        if ($EnableLogAnalytics) {
         $logConfig = @{
               EnableLogAnalytics=$true
               WorkspaceId = $WorkspaceId
               AuthenticationId = $AuthenticationId;
            }
         $sessionLauncherExpression = $sessionLauncherExpression + $logConfig
        }
        Start-SessionLauncher @sessionLauncherExpression
	
        Write-Log -Message "Start broker"
        $brokerExpression = @{
          DestinationPath = $DestinationPath
          SessionAddress = "localhost"  
        } 
        if ($EnableLogAnalytics) {
            $logConfig = @{
               EnableLogAnalytics=$true
               WorkspaceId = $WorkspaceId
               AuthenticationId = $AuthenticationId;
            }
         $brokerExpression = $brokerExpression + $logConfig
        }
        Start-Broker @brokerExpression
    }
    Catch {
        Write-Log -Message "Fail to start telepathy service" -Level Error
        Write-Log -Message $_ -Level Error
    }

    Write-Log -Message "Add EchoClient in PATH environment varaible"
    $EchoClientPath = "$DestinationPath\Echoclient\EchoClient.exe"
    $env:path = $env:path + ";$EchoClientPath"
    [System.Environment]::SetEnvironmentVariable("PATH", $env:path, "Machine")
}
Export-ModuleMember -Function Start-TelepathyService

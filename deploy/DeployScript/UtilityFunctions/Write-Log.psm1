function Set-LogSource {
    [CmdletBinding()] 
    Param 
    ( 
        [Parameter(Mandatory = $true)] 
        [string]$SourceName
    )

    Process {
        try {
            $Source = (Get-Variable -Name "LogSource" -Scope 1 -ValueOnly -ErrorAction Stop)
            Set-Variable -Name "LogSource" -Scope 1 -Value $SourceName
        }
        catch {
            New-Variable -Name 'LogSource' -Value $SourceName -Scope 1
        }
    }
}


function Write-Log { 
    [CmdletBinding()] 
    Param 
    (
        [Parameter(Mandatory = $true, 
            ValueFromPipelineByPropertyName = $true)] 
        [ValidateNotNullOrEmpty()] 
        [Alias("LogContent")] 
        [string]$Message, 
 
        [Parameter(Mandatory = $false)] 
        [Alias('LogPath')] 
        [string]$Path = 'C:\Logs\telepathy.log', 
         
        [Parameter(Mandatory = $false)] 
        [ValidateSet("Error", "Warn", "Info")] 
        [string]$Level = "Info", 
         
        [Parameter(Mandatory = $false)] 
        [switch]$NoClobber 
    ) 
 
    Begin { 
        # Set VerbosePreference to Continue so that verbose messages are displayed. 
        $VerbosePreference = 'Continue' 
    } 
    Process { 
         
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
 
        # Format Date for Log File 
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

        try {
            $Source = (Get-Variable -Name 'LogSource' -Scope 1 -ValueOnly -ErrorAction Stop)
        }
        catch {
            $Source = "Unknown"
        }
                
        # Write log entry to $Path 
        "$FormattedDate $LevelText [$Source] $Message" | Out-File -FilePath $Path -Append 
    } 
    End { 
    } 
}

Export-ModuleMember -Function Write-Log, Set-LogSource
param (
    [string]$ArtifactsPath,
    [string]$DesStorageConnectionString
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
        "$FormattedDate $LevelText [EnableTelepathyStorage] $Message" | Out-File -FilePath $Path -Append 
    } 
    End 
    { 
    } 
}

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

Write-Log -Message "Files to upload in path : $ArtifactsPath"
Write-Log -Message "DesStorageConnectionString : $DesStorageConnectionString"

Try {
    $desStorageContext = New-AzStorageContext -ConnectionString $DesStorageConnectionString
} Catch {
    Write-Log -Message "Error when get destination storage context" -Level Error
    Write-Log -Message $_ -Level Error
}

Try {
    "runtime service-assembly service-registration".split() | New-AzStorageContainer -Context $desStorageContext
} Catch {
    Write-Log -Message "Error when new storage container" -Level Error
    Write-Log -Message $_ -Level Error
}

Try {
    uploadFiles -LocalPath "$ArtifactsPath\CcpServiceHost" -RemotePath "ccpservicehost" -ContainerName "runtime" -StorageContext $desStorageContext
    uploadFiles -LocalPath "$ArtifactsPath\EchoSvcLib" -RemotePath "ccpechosvc" -ContainerName "service-assembly" -StorageContext $desStorageContext
    uploadFiles -LocalPath "$ArtifactsPath\Registration"  -ContainerName "service-registration" -StorageContext $desStorageContext
} Catch {
    Write-Log -Message "Error when upload files to storage" -Level Error
    Write-Log -Message $_ -Level Error
}
<# Custom Script for Windows to install files from Azure Storage using the staging folder created by the deployment script #>
param (
    [HashTable]$Params
)

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
                
        # Write log entry to $Path 
        "$FormattedDate $LevelText [StartTelepathyShim] $Message" | Out-File -FilePath $Path -Append 
    } 
    End { 
    } 
}

Install-PackageProvider -Name NuGet -MinimumVersion 2.8.5.201 -Force
Install-Module -Name Az -AllowClobber -Force

$destination_path = "C:\telepathy"
$artifactsFolderName = $Params["ArtifactsFolderName"]
$artifactsPath = "$destination_path\$artifactsFolderName\Release"

<# Download artifacts from the specified Azure Storage containers #>
Try {
    Write-Log -Message 'StorageAccountName : $($Params["SrcStorageAccountName"])'
    Write-Log -Message 'StorageSasToken : $($Params["SrcStorageContainerSasToken"])'
    $srcStorageContext = New-AzStorageContext -StorageAccountName $Params["SrcStorageAccountName"] -SasToken $Params["SrcStorageContainerSasToken"]  
}
Catch {
    Write-Log -Message "Please provide valid storage account name and sas token" -Level Error
    Write-Log -Message $_ -Level Error
}

Try {
    Write-Log -Message 'ContainerName : $($Params["ContainerName"])'
    Write-Log -Message 'ArtifactsFolderName : $($Params["ArtifactsFolderName"])'
    
    $blobs = Get-AzStorageBlob -Container $Params["ContainerName"] -Blob "$($Params["ArtifactsFolderName"])*" -Context $srcStorageContext
}
Catch {
    Write-Log -Message "Error occurs when get source storage blob, can't get storage blob, please confirm you provide valid container name, blob name and storage context " -Level Error
    Write-Log -Message $_ -Level Error
}

Try {
    Write-Log -Message "DestinationPath in VM : $destination_path"
    foreach ($blob in $blobs) {  
        New-Item -ItemType Directory -Force -Path $destination_path  
        Get-AzStorageBlobContent -Container $Params["ContainerName"] -Blob $blob.Name -Destination $destination_path -Context $srcStorageContext   
    } 
}
Catch {
    Write-Log -Message "Error occurs when download source storage blob to VM " -Level Error
    Write-Log -Message $_ -Level Error
}
<# Artifacts are all ready #>

Import-Module -Name $artifactsPath\DeployScript\Start-Telepathy.psd1 -Verbose
Start-Telepathy -Params @Params   
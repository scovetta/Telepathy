<# Custom Script for Windows to install a file from Azure Storage using the staging folder created by the deployment script #>
param (
    [HashTable]$Params
)
$destination_path = "C:\telepathy"
$artifactsFolderName = $Params["ArtifactsFolderName"]
$artifactsPath = "$destination_path\$artifactsFolderName\Release"
Import-Module -Name $artifactsPath\DeployScript\Start-Telepathy.psd1 -Verbose
Start-Telepathy -Params @Params   





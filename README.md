# Microsoft Telepathy 

![Azure DevOps builds (branch)](https://img.shields.io/azure-devops/build/bc-telepathy/32d89ced-58e3-4e1d-835e-b6e22ec7cc80/3/dev) ![GitHub](https://img.shields.io/github/license/Azure/Telepathy) [![GitHub issues](https://img.shields.io/github/issues/Azure/Telepathy)](https://github.com/Azure/Telepathy/issues) ![GitHub last commit](https://img.shields.io/github/last-commit/azure/telepathy)

Microsoft Telepathy is a SOA runtime framework works in a cloud native way, enables running high-throughput and low-latency calculation workload in Azure.

## Get Started with Nightly Build

### Deploy in Azure Portal

<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure%2FTelepathy%2Fdev%2Fdeploy%2Fazuredeploy.json" target="_blank">
    <img src="http://azuredeploy.net/deploybutton.png"/>
</a>

### Deploy using Azure CLI

```shell
[ "$(az group exists -n ResourceGroupName)" = "true" ] && az group delete -n  ResourceGroupName -y
az group create --name ResourceGroupName --location "japaneast" --subscription subscriptionName
az group deployment create `
  --name DeployementName `
  --resource-group ResourceGroupName `
  --subscription subscriptionName `
  --template-file  "location of template file" `
  --parameters "location of parameters.json file"
```

### Deploy using PowerShell

```powershell
$ResourceGroupName = ""
$Location = ""
$TemplateFile = ""
$TemplateParameterFile = ""

Connect-AzAccount
if(Get-AzResourceGroup -Name $ResourceGroupName) {
    Remove-AzResourceGroup -Name $ResourceGroupName -Force
}

New-AzResourceGroup -Name $ResourceGroupName -Location $Location
New-AzResourceGroupDeployment -ResourceGroupName $ResourceGroupName -TemplateFile $TemplateFile -TemplateParameterFile $TemplateParameterFile
```

### Reference Microsoft.Telepathy.Session SDK NuGet package

To use nightly SDK package, add following NuGet source.

- Name: telepathy-sdk-preview
- Source: https://pkgs.dev.azure.com/bc-telepathy/telepathy/_packaging/telepathy-sdk-preview/nuget/v3/index.json

Check [Add the feed to your NuGet configuration](https://docs.microsoft.com/en-us/azure/devops/artifacts/nuget/consume?view=azure-devops) for detailed instruction.

## Developing

### Developing Environment

- Visual Studio 2019 of latter
- Excel 2016 or latter if developing Excel plugin

### Build from Source Code

```shell
git clone https://github.com/Azure/Telepathy.git
cd Telepathy
build.bat
```


# Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

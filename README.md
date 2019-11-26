# Microsoft Telepathy 

![Azure DevOps builds (branch)](https://img.shields.io/azure-devops/build/bc-telepathy/32d89ced-58e3-4e1d-835e-b6e22ec7cc80/3/dev) ![GitHub](https://img.shields.io/github/license/Azure/Telepathy) [![GitHub issues](https://img.shields.io/github/issues/Azure/Telepathy)](https://github.com/Azure/Telepathy/issues) ![GitHub last commit](https://img.shields.io/github/last-commit/azure/telepathy)

Microsoft Telepathy is a SOA runtime framework works in a cloud native way, enables running high-throughput and low-latency calculation workload in Azure. Evolving from the battle-tested SOA Runtime of [Microsoft HPC Pack](https://docs.microsoft.com/en-us/powershell/high-performance-computing/overview?view=hpc16-ps).

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

### Reference `Microsoft.Telepathy.Session` SDK NuGet package

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

## Migrate From HPC Pack SOA

See [Migrate From HPC Pack SOA to Microsoft Telepathy](doc/migrate_from_hpc_pack_soa_to_microsoft_telepathy.md)

## Benchmark (use Azure Batch Backend)

*Average result of 10 trials*. [Benchmark detail and how to benchmark your cluster](doc/performance_benchmark.md).

### Throughput

#### Interactive Session

- Message Send Throughput: **35394.15** messages/second
- Broker Process Throughput: **15973.63** messages/second
- End to End Throughput: **15652.66** messages/second

### Latency

#### Interactive Session

- Warm Latency: **98.59431** millisecond

### CPU Efficiency

#### Interactive Session

- CPU Efficiency: **99.603%** (**398.412%** on 4-core compute nodes)

## Documentation

See [Documentation Index](doc/index.md).

## vNext

- Cross platform support
- Cross language support
- Data service intigration
- IdentityServer intigration

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

# Microsoft Telepathy [![Build Status](https://dev.azure.com/bc-telepathy/telepathy/_apis/build/status/Azure.Telepathy?branchName=dev)](https://dev.azure.com/bc-telepathy/telepathy/_build/latest?definitionId=3&branchName=dev) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Microsoft Telepathy is a SOA runtime framework works in a cloud native way, enables running high-throughput and low-latency calculation workload in Azure.

## Performance (use Azure Batch Backend)

### Message Throughput

*Average result of 10 trials*

|        Type         | Concurrency | Total Request Count | Request Size (byte) | Send Throughput (message/second) | Broker Throughput (message/second) | Overall Throughput (message/second) |
| :-----------------: | :---------: | :-----------------: | :-----------------: | :------------------------------: | :--------------------------------: | :---------------------------------: |
| Interactive Session |      8      |       800,000       |          4          |           **35394.15**           |            **15973.63**            |            **15652.66**             |

### Warm First Response Time

*Average result of 10 trials*

|        Type         | Concurrency | Request Count Per Core | Request Size (byte) | Warm First Response Time (millisecond) |
| :-----------------: | :---------: | :--------------------: | :-----------------: | :------------------------------------: |
| Interactive Session |      1      |           1            |          4          |              **98.59431**              |

## Get Started

### Deploy in Azure Portal

<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure%2FTelepathy%2Fmaster%2Fdeploy%2Fazuredeploy.json" target="_blank">
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

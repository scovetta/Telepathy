# Microsoft Telepathy 

![Azure DevOps builds (branch)](https://img.shields.io/azure-devops/build/bc-telepathy/32d89ced-58e3-4e1d-835e-b6e22ec7cc80/3/dev) ![GitHub](https://img.shields.io/github/license/Azure/Telepathy)![GitHub release (latest SemVer)](https://img.shields.io/github/v/release/azure/telepathy) ![Nuget](https://img.shields.io/nuget/v/Microsoft.Telepathy.Session)

Microsoft Telepathy is a SOA runtime framework works in a cloud native way, enables running high-throughput and low-latency calculation workload in Azure. Evolving from the battle-tested SOA Runtime of [Microsoft HPC Pack](https://docs.microsoft.com/en-us/powershell/high-performance-computing/overview?view=hpc16-ps).

## Get Started

### Deploy a Cluster

<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure%2FTelepathy%2Fmaster%2Fdeploy%2Fazuredeploy.release.json" target="_blank">
    <img src="http://azuredeploy.net/deploybutton.png"/>
</a>

Check [Deploy a Telepathy Cluster Step by Step](doc/deployment.md) for detailed instruction.

### SDK NuGet package

Add [Microsoft.Telepathy.Session](https://www.nuget.org/packages/Microsoft.Telepathy.Session/) to your project [using NuGet manager](https://docs.microsoft.com/en-us/nuget/quickstart/install-and-use-a-package-in-visual-studio).

## Try the Nightly Build

### Deploy a Cluster

<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure%2FTelepathy%2Fdev%2Fdeploy%2Fazuredeploy.json" target="_blank">
    <img src="http://azuredeploy.net/deploybutton.png"/>
</a>

### SDK NuGet package

To use `Microsoft.Telepathy.Session` nightly package, add following NuGet source.

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

#### Durable Session

- Message Send Throughput: **2998.94** messages/second
- Broker Process Throughput: **1038.23** messages/second
- End to End Throughput: **751.61** messages/second

### Latency

#### Interactive Session

- Warm Latency: **98.59431** millisecond

#### Durable Session

- Warm Latency: **1434.801444** millisecond

### CPU Efficiency

#### Interactive Session

- CPU Efficiency: **99.603%** (**398.412%** on 4-core compute nodes)

#### Durable Session

- CPU Efficiency: **92.627%** (**370.663%** on 4-core compute nodes)

## Documentation

See [Documentation Index](doc/index.md).

## vNext

- Cross platform support
- Cross language support
- Data service integration
- IdentityServer integration

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

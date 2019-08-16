# Telepathy

[![Codacy Badge](https://api.codacy.com/project/badge/Grade/0dc35569832b4f8fa08ac7b14399bcb0)](https://app.codacy.com/app/amat27/telepathy?utm_source=github.com&utm_medium=referral&utm_content=amat27/telepathy&utm_campaign=Badge_Grade_Dashboard)
[![Build status](https://ci.appveyor.com/api/projects/status/1av6v6xb5bfbv7t5/branch/master?svg=true)](https://ci.appveyor.com/project/amat27/telepathy/branch/master)
[![Build Status](https://dev.azure.com/bc-telepathy/telepathy/_apis/build/status/telepathy-CI?branchName=dev)](https://dev.azure.com/bc-telepathy/telepathy/_build/latest?definitionId=2&branchName=dev)

Home repo of Project Telepathy. Currently in **prototyping** stage.

## Developing Environment

- Visual Studio 2017 of latter
- Excel 2016 or latter if developing Excel service

## Engineering Practice

This project uses git flow. All new features will go into dev branch.


## Targeting Azure Batch as Back End

When starting Session Launcher, using parameter

```cmd
-d --AzureBatchServiceUrl <AzureBatchAddress> --AzureBatchAccountName <BatchAccountName> --AzureBatchAccountKey <BatchAccountKey> --AzureBatchPoolName <BatchPoolName>  -c <AzureStorageConnectionString>
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

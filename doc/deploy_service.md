# Deploy A SOA Service to Microsoft Telepathy Cluster

Before starting, you need to have write access to the cluster's admin Azure Storage Account. It is usually deployed and managed by the cluster admin.

## Structure of the Admin Storage 

In the admin Azure Storage Account, there are 3 blob storage containers, named **runtime**, **service-assembly** and **service-registration**.

![image-20191128114604435](C:\Users\hpcadmin\AppData\Roaming\Typora\typora-user-images\image-20191128114604435.png)

### `runtime` Container

This container contains all the binaries required by Telepathy. We won't touch this container when deploying new services.

### `service-assembly` Container

This container contains all the service assemblies, all in folders having the same name with corresponding service name.

![image-20191128114959727](C:\Users\hpcadmin\AppData\Roaming\Typora\typora-user-images\image-20191128114959727.png)

> Note: Due to restriction of Azure Storage Blob, all subfolders of `service-assembly`  must be named in lowercase.

### `service-registration` Container

This container contains all the service registration files, each named with corresponding service name.

![image-20191128115218166](C:\Users\hpcadmin\AppData\Roaming\Typora\typora-user-images\image-20191128115218166.png)

> Note: Due to restriction of Azure Storage Blob, all service registration files must be named in lowercase.

## How to Create a new SOA Service

Please check [Microsoft Telepathy SOA Tutorial I – Write your first SOA service and client](tutorial/soa-tutorial-1-write-your-first-soa-service-and-client.md).

## How to Deploy a new SOA Service

Deployment of a new SOA Service is made simple. 

- Decide a service name of your new service. We'll use **SampleService** to illustrate.

- Create a new folder in `service-assembly` container in admin storage account, change its name to the service name and in lowercase. Example: service-assembly**/sampleserivce**
- Copy all the service assemblies into above folder.
- Copy the service registration file into `service-registration` container, and make sure its name is identical to your service name and is in lowercase. Example: service-registration**/sampleservice.config**
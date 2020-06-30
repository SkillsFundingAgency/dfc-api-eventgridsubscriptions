# Digital First Careers – Subscriptions API

## Introduction
This is a function app that allows consumers to create and remove subscriptions to an Event Grid Topic.

## Getting Started

### Installing

Clone the project and open the solution in Visual Studio 2019.

## List of dependencies

|Item	|Purpose|
|-------|-------|
|Event Grid Topic | An Event Grid Topic to subscribe / delete |
|Azure Subscription | Access to the Dev Azure subscription is required to access KeyVault.

## Install Azure CLI
1) https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest
2) Open PowerShell and type 'az login' - follow the instructions and login to your Citizen account.
3) As long as your Azure permissions are correct on the KeyVault the application should now run.

## Local Config Files

## Configuring to run locally

The project contains a number of "appsettings-template.json" files which contain sample appsettings for the web app and the test projects. To use these files, rename them to "appsettings.json" and edit and replace the configuration item values with values suitable for your environment.

## Running locally

To run this product locally, you will need to configure the list of dependencies, once configured and the configuration files updated, it should be F5 to run and debug locally. The application can be run using IIS Express or full IIS.

## Deployments

This API is deployed via an Azure DevOps release pipeline.

## Built With

* Microsoft Visual Studio 2019
* .Net Core 3.1

## References

Supported API Endpoints:

1) POST - /Execute
    Body:
```
{
    "Name":"test-subscription",
    "Endpoint":"https://somewhere.azurewebsites.net/api/webhook/receiveevents",
    "Filter":{
        "BeginsWith":"atestbeginswith",
        "EndsWith":"atestendswith",
        "IncludeEventTypes":["blobcreated","contentcreated"],
        "PropertyContainsFilter":{"key":"subject", "values":["guid1","guid2","emails"]}
    }
}
```
2) DELETE - /Execute/{subscriptionName}

Please note, as AdvancedFilters are all derived types, the list of advanced filters has to be constructed in code.

Currently the only supported Advanced Filter as part of this solution is StringInAdvanced Filter, as outlined here:
https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.management.eventgrid.models.stringinadvancedfilter?view=azure-dotnet

The key the property to apply the filter on.

At the time of writing the limits for advanced filters are as follows:
- 5 Advanced filters per Event Grid Topic Subscription
- 25 values in an Advanced Filter item collection, across all applied advanced filters

More information can be found here:
https://docs.microsoft.com/en-us/azure/event-grid/event-filtering
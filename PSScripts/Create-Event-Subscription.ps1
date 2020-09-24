<#
.SYNOPSIS
Update an APIM API with an openapi definition

.DESCRIPTION
Update an APIM API with a openapi definition

.PARAMETER appSharedResourceGroup
The name of the resource group that contains the APIM instnace

.PARAMETER appSharedStorageAccountName
The name of the APIM instance

.PARAMETER DeadLetterBlobContainerName
The name of the API to update

.PARAMETER DeadLetterSubscriptionName
The path to save the openapi specification file to update the APIM instance with.

.PARAMETER DeadLetterSubscriptionEndPoint
The path to save the openapi specification file to update the APIM instance with.

.EXAMPLE
Import-ApimOpenApiDefinitionFromFile -ApimResourceGroup dfc-foo-bar-rg -InstanceName dfc-foo-bar-apim -ApiName bar -OpenApiSpecificationFile some-file.yaml -Verbose

#>
param(
    [Parameter(Mandatory=$true)]
    [string]$appSharedResourceGroupName,
    [Parameter(Mandatory=$true)]
    [string]$appSharedStorageAccountName,
    [Parameter(Mandatory=$true)]
    [string]$DeadLetterBlobContainerName,
    [Parameter(Mandatory=$true)]
    [string]$DeadLetterSubscriptionName,
    [Parameter(Mandatory=$true)]
    [string]$DeadLetterSubscriptionEndPoint
)
try {
    # --- Build context and retrieve apiid
    $storageAccount = (Get-AzStorageAccount  `
      -ResourceGroupName $appSharedResourceGroupName  `
      -Name $appSharedStorageAccountName)
    $ctx = $storageAccount.Context
    $storageid = $storageAccount.Id
    New-AzStorageContainer -Name $DeadLetterBlobContainerName  `
      -Context $ctx  `
      -Permission blob  `
      -Verbose
    New-AzEventGridSubscription  `
      -ResourceId $storageid `
      -EventSubscriptionName $DeadLetterSubscriptionName  `
      -Endpoint $DeadLetterSubscriptionEndPoint  `
      -Verbose
}
catch {
   throw $_
}
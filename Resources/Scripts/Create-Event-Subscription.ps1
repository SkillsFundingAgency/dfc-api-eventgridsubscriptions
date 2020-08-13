<#
.SYNOPSIS
Update an APIM API with an openapi definition

.DESCRIPTION
Update an APIM API with a openapi definition

.PARAMETER ApimResourceGroup
The name of the resource group that contains the APIM instnace

.PARAMETER InstanceName
The name of the APIM instance

.PARAMETER ApiName
The name of the API to update

.PARAMETER OpenApiSpecificationFile
The path to save the openapi specification file to update the APIM instance with.

.EXAMPLE
Import-ApimOpenApiDefinitionFromFile -ApimResourceGroup dfc-foo-bar-rg -InstanceName dfc-foo-bar-apim -ApiName bar -OpenApiSpecificationFile some-file.yaml -Verbose

#>

try {
    # --- Build context and retrieve apiid
    Write-Host "Building APIM context for"
    Set-AzContext -SubscriptionId '962cae10-2950-412a-93e3-d8ae92b17896' -Verbose
    $containername = 'event-grid-dead-letter-events' 
    $topicid = (Get-AzEventGridTopic -ResourceGroupName 'dfc-dev-stax-editor-rg' -Name 'dfc-dev-stax-egt').Id 
    $storageid = (Get-AzStorageAccount -ResourceGroupName 'dfc-dev-compui-shared-rg' -Name 'dfcdevcompuisharedstr').Id 
    New-AzEventGridSubscription -ResourceId $topicid -EventSubscriptionName 'dfc-dead-letter' -Endpoint 'https://dfc-dev-api-eventgridsubscriptions-fa.azurewebsites.net/api/DeadLetter/api/updates' -DeadLetterEndpoint "$($storageid)/blobServices/default/containers/$($containername)" 
}
catch {
   throw $_
}
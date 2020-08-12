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
    az eventgrid event-subscription create --source-resource-id '/subscriptions/962cae10-2950-412a-93e3-d8ae92b17896/resourceGroups/dfc-dev-compui-shared-rg/providers/Microsoft.Storage/storageAccounts/dfcdevcompuisharedstr' --name 'dfc-dead-letter' --endpoint 'https://dfc-dev-api-eventgridsubscriptions-fa.azurewebsites.net/api/DeadLetter/api/updates'


    # --- Import openapi definition
    #Write-Host "Updating API $InstanceName\$($ApiName) from definition $($OutputFile.FullName)"
    #Import-AzApiManagementApi -Context $Context -SpecificationFormat OpenApi -SpecificationPath $OpenApiSpecificationFile -ApiId $ApiName -Path $ApiPath -ErrorAction Stop -Verbose:$VerbosePreference
}
catch {
   throw $_
}
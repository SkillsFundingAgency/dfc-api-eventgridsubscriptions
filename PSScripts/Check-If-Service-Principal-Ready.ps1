param(
    [string]$ServicePrincipalName
)

$elapsed = 0;
$delay = 3;
$limit = 5 * 60;

$checkMsg = "Checking for service principal $ServicePrincipalName"
Write-Verbose $checkMsg
$AdServicePrincipal = Get-AzADServicePrincipal -DisplayName $ServicePrincipalName
while(!$AdServicePrincipal -and $elapsed -le $limit) {
    $elapsedSeconds = $elapsed + "s";
    Write-Verbose "Service principal is not yet available. Retrying in $delay seconds... ($elapsedSeconds elapsed)"
    Start-Sleep -Seconds $delay;
    $elapsed += $delay;

    Write-Verbose $checkMsg
    $AdServicePrincipal = Get-AzADServicePrincipal -DisplayName $ServicePrincipalName
}

if($AdServicePrincipal) {
    Write-Verbose "Service principal is now available for use."
    exit 1
}

Write-Verbose "Service principal did not become ready within the allotted time."
exit 0


$client = $args[0]
$server = $args[1]
$sourceFolder = $args[2]
$buildOnly = $args[3]
$octopusProject = $args[4]
$apiKey = $args[5]
$buildSteps = $args[6]
$skipSteps = $args[7]

#$clientScript = "$PSScriptRoot\Clients\$client.ps1"
. "$PSScriptRoot\Nuspecker.ps1"
#. $clientScript


cd "$sourceFolder\Deployment\NuSpecs"

$Packages = $buildSteps.Split(';',[System.StringSplitOptions]::RemoveEmptyEntries)
$Skips = $skipSteps.Split(';',[System.StringSplitOptions]::RemoveEmptyEntries)
if($Packages.Count -eq 0)
{
	Write-Host "No packages configured for building" -ForegroundColor Red
}
$success = $TRUE
foreach($package in $Packages)
{
    $success = $success -and ( BuildNuget $package)[-1]
}

if(-not $success)
{
  throw [System.Exception] "Error building NuSpecs."
}

$octopusSkips = ""
foreach($package in $Skips)
{
	write-host ""
	write-host "-- SKIPPING $($package)" -foregroundcolor "yellow"
	$octopusSkips += " --skip=$($package) "
}


if($buildOnly -eq "false")
{
	write-host "Starting Octopush" -foregroundcolor "green"
	$exec = "octo deploy-release --progress --forcepackagedownload --project $octopusProject --deployto LOC --version latest --waitfordeployment "
	$exec += "--server http://$($server):8080 --apiKey $apiKey $octopusSkips"
	write-host $exec
	iex $exec
	write-host "Completed Octopushing" -foregroundcolor "green"
}

$timestamp = get-date
$formattedTime = $timeStamp.ToString("h:mm:ss")
write-host ""
write-host "Done at $formattedTime" -foregroundcolor "green"

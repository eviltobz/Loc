$client = $args[0]
$server = $args[1]
$buildOnly = $args[2]

$clientScript = "$PSScriptRoot\Clients\$client.ps1"
. "$PSScriptRoot\Nuspecker.ps1"
. $clientScript

write-host ""
if($buildOnly -eq "true") 
{ write-host "Starting $client as BuildOnly" -foregroundcolor "green" }
else 
{ write-host "Starting $client deploy to $server" -foregroundcolor "green" }

cd c:\dev\sourc0\Deployment\NuSpecs

$success = $TRUE
$octopusSkips = ""
foreach($package in $Packages)
{
  if($package[1] -eq 1)
  {
    $success = $success -and ( BuildNuget $package[0])[-1]
  }
  else
  {
    write-host ""
    write-host "-- SKIPPING $($package[0])" -foregroundcolor "yellow"
    $octopusSkips += " --skip=$($package[0]) "
  }
}

if(-not $success)
{
  throw [System.Exception] "Error building NuSpecs."
}

if($buildOnly -eq "false")
{
  . $PSScriptRoot\Octopush $OctopusProjectName $octopusSkips
}

$timestamp = get-date
$formattedTime = $timeStamp.ToString("h:mm:ss")
write-host ""
write-host "Done at $formattedTime" -foregroundcolor "green"

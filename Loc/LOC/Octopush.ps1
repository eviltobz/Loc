$project = $args[0]
$skips = $args[1]

Write-Host ""
Write-Host "NOTE TO SELF: might wanna add a triggery call jobbette to rebuild the klondike index so packageybits can be found!" -foregroundcolor "red"
Write-Host ""
Write-Host "NOTE TO SELF: This has hardcoded server info, which is configged, and network username & api key which could be added to config" -foregroundcolor "red"
Write-Host ""

Write-Host "Clearing remote caches" -foregroundcolor "GREEN"
Write-Host "If this step fails run this command on the target server: Enable-PSRemoting -force" -foregroundcolor "GRAY"
#Invoke-Command -ComputerName LOC-TC-03 -Credential toby.carter -FilePath "$PSScriptRoot\OctoKill2.ps1"
write-host "skipping Invoke-Command -ComputerName LOC-TC-03  -FilePath ""$PSScriptRoot\OctoKill2.ps1"""
Write-Host ""

# RUN DEPLOYMENT
write-host "Starting Octopush" -foregroundcolor "green"

$exec = "octo deploy-release --progress --forcepackagedownload --project $project --deployto LOC --version latest --waitfordeployment --server http://LOC-TC-03:8080 --apiKey API-OIEP5GZVJY9L8YPGEBCG9AWJ2PW $skips"

write-host $exec
iex $exec

# --skip=VALUE

write-host "Completed Octopushing" -foregroundcolor "green"

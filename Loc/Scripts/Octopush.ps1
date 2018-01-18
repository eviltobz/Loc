#$project = $args[0]
#$server = $args[1]
#$apiKey = $args[2]
#$skips = $args[3]


## RUN DEPLOYMENT
#write-host "Starting Octopush" -foregroundcolor "green"
#$exec = "octo deploy-release --progress --forcepackagedownload --project $project --deployto LOC --version latest --waitfordeployment "
#		+ "--server $server --apiKey $apiKey $skips"
#		#+ "--server http://LOC-TC-03:8080 --apiKey API-OIEP5GZVJY9L8YPGEBCG9AWJ2PW $skips"
#write-host $exec
#iex $exec
#write-host "Completed Octopushing" -foregroundcolor "green"

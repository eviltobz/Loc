
function killee($path)
{
  if (Test-Path $path)
  {
    cmd /C "rmdir /s /q ""$path"""
  }
}


write-host "Purging Octopus cache locations" -foregroundcolor "RED"

write-host "Tentacle\Files"
killee D:\Octopus\Tentacle\Files

write-host "Tentacle\Applications\LOC"
killee D:\Octopus\Tentacle\Applications\LOC

write-host "Server\PackageCache\"
killee D:\Octopus\Server\OctopusServer\PackageCache\


write-host "Purge complete" -foregroundcolor "YELLOW"

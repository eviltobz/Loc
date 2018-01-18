function killee($path)
{
  if (Test-Path $path)
  {
    cmd /C "rmdir /s /q ""$path"""
  }
}

write-host "Purging Octopus cache locations" -foregroundcolor "RED"
write-host "Tentacle\Applications\Packages"
write-host "-qa"
killee E:\Octopus2Tentacle\qa.localhost\Applications\.Tentacle\Packages\
write-host "-ra"
killee E:\Octopus2Tentacle\ra.localhost\Applications\.Tentacle\Packages\
write-host "-shared"
killee E:\Octopus2Tentacle\SharedDeploy\Applications\.Tentacle\Packages\
#rm -force -recurse E:\Octopus2Tentacle\SharedDeploy\Applications\.Tentacle\Packages\*.*

write-host "Tentacle\Applications\LOC"
write-host "-qa"
killee E:\Octopus2Tentacle\qa.localhost\Applications\LOC\
write-host "-ra"
killee E:\Octopus2Tentacle\ra.localhost\Applications\LOC\
write-host "-shared"
killee E:\Octopus2Tentacle\SharedDeploy\Applications\LOC\

write-host "Server\PackageCache\"
#get-childitem E:\Octopus2ServerData\PackageCache\ -recurse | remove-item -recurse
killee E:\Octopus2ServerData\PackageCache\

write-host "Purge complete" -foregroundcolor "YELLOW"

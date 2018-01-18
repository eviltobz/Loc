function BuildNuget ($Nuspec) 
{
  write-host ""
  write-host "Building $Nuspec Package" -foregroundcolor "yellow"

  $Nuspec = $Nuspec.Trim()

  $NuSpecPath = ".\" + $Nuspec + ".nuspec"
  $RootPath = "..\..\"
  $NugetFeed = $RootPath + "NugetFeed\"
  $TargetFolder = $NugetFeed + $Nuspec 
  $TargetFile =  $TargetFolder + "\" + $Nuspec + ".*.nupkg"

  $Nuget = $RootPath + "packages\NuGet.CommandLine\tools\nuget"
  if(!(Test-Path $NuSpecPath))
  {
    write-host "**********" -foregroundcolor "yellow"
    write-host "NuSpec file $Nuspec.nuspec doesn't exist in Nupecs Folder" -foregroundcolor "red"
    write-host "**********" -foregroundcolor "yellow"
    throw [System.IO.FileNotFoundException] "$NuSpecPath not found."
  }
  else
  {
    if(!(Test-Path $TargetFolder))
    {
      write-host "Creating NugetFeed folder" 
      New-Item $TargetFolder -type directory
    }

    if(Test-Path $TargetFile)
    {
      write-host "Removing the $TargetFile package."
      Remove-Item $TargetFile
    }

    $from = pwd
    write-host "Running from $from"
    write-host "Packaging NuSpec: $NuSpecPath"

    if(!(Test-Path $TargetFolder))
    {
      write-host "Creating Folder $TargetFolder"
      mkdir "$TargetFolder"
    }
    $command = "$Nuget pack $NuSpecPath -nopackageanalysis -outputdirectory $TargetFolder -basepath $RootPath"
    Write-Host $command
    iex $command

  }

  if(!(Test-Path $TargetFile))
  {
    write-host "Failed To Package" -foregroundcolor "red"
    return $FALSE
  }
  else
  {
    write-host "Package Success" -foregroundcolor "green"
    return $TRUE
  }
}

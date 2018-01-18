module ConfigManager

open System.Configuration
    

let private ClientKey = "SelectedClient"
let private VerboseKey = "Verbose"
let private ServerKey = "LocServer"
let private OctopusPurgeDirsKey = "OctopusPurgeDirs"

let private config() = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None)
let private settings() = config().AppSettings.Settings

let private isConfigPresent key =
    match settings().[key] with
    | null -> false
    | _ -> true

let private readConfigX key = 
    let get (s:KeyValueConfigurationCollection) key = 
       match s.[key] with 
       | null -> None
       | x    -> Some x.Value 
    get (settings()) key

let private readConfigOrDefault key defaultValue = 
    match settings().[key] with 
    | null -> defaultValue
    | x    -> x.Value 

let private readConfig key = 
    readConfigOrDefault key ("<NO VALUE CONFIGURED FOR " + key + ">")

let private readCsv key =
    let content = readConfigX key
    match content with
    | None -> [||]
    | Some all -> all.Split(';')


let private getClientConfig clientX =
    match clientX with
    | Some client ->
        if isConfigPresent (client + "_OctopusProject") then
            let octopusProject = readConfig (client+"_OctopusProject")
            let nugetsToBuild = readCsv (client+"_NugetsToBuild") 
            let stepsToSkip = readCsv (client+"_OctopusStepsToSkip") 
            (true, octopusProject, nugetsToBuild, stepsToSkip)
        else
            (false, "",[||],[||])
    | None -> (false, "",[||],[||])

let private readBool key =
    let value = readConfigOrDefault key "false"
    let success, result = bool.TryParse value
    success && result


let private writeConfig key value = 
    let config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None)
    let settings = config.AppSettings.Settings
    match settings.[key] with 
    | null -> settings.Add(key, value.ToString())
    | x    -> x.Value <- value.ToString()
    config.Save(ConfigurationSaveMode.Modified)

let SetClient selectedClient = 
    writeConfig ClientKey selectedClient

let ToggleVerbosity () =
    writeConfig VerboseKey (not (readBool VerboseKey))

//let SetServer address = 
//    writeConfig ServerKey address

type ConfigBlock = {
    Verbose:bool;
    Editor:string;
    LocSpecified:bool;
    ClientSpecified:bool;
    LocAddress:string;
    CurrentClient:string;
    OctopusApiKey:string;
    OctopusPurgeDirs:string[]
    OctopusProjectSpecified:bool;
    OctopusProject:string;
    NugetsToBuild :string[];
    StepsToSkip:string[];
    SourceFolder:string;
    }

let ReadConfig () =
    let client = readConfigX ClientKey
    let sourceFolder = readConfig "SourceFolder"
    let octopusPurgeDirs = readCsv "OctopusPurgeDirs"
    let octopusConfigured, octopusProject, nugetsToBuild, stepsToSkip = getClientConfig client
    {
        Verbose = readBool VerboseKey
        Editor = readConfigOrDefault "Editor" "notepad.exe"
        LocSpecified=isConfigPresent ServerKey
        ClientSpecified=isConfigPresent ClientKey
        LocAddress= readConfig ServerKey
        CurrentClient= readConfig ClientKey
        SourceFolder = readConfig "SourceFolder"
        OctopusApiKey = readConfig "OctopusApiKey" 
        OctopusPurgeDirs = octopusPurgeDirs
        OctopusProjectSpecified = octopusConfigured
        OctopusProject = octopusProject
        NugetsToBuild = nugetsToBuild
        StepsToSkip = stepsToSkip 
     }


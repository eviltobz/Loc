module Config

open System.Configuration

type config = {
    LocSpecified:bool;
    ClientSpecified:bool;
    LocAddress:string;
    CurrentClient:string
    }

let private ClientKey = "SelectedClient"
let private ServerKey = "LocServer"



let private readConfig key = 
    let config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None)
    let settings = config.AppSettings.Settings
    let get (s:KeyValueConfigurationCollection) key = 
       match s.[key] with 
       | null -> None
       | x    -> Some x.Value 
    get settings key

let get =
    let client = readConfig ClientKey
    let server = readConfig ServerKey
    {
        LocSpecified = server.IsSome
        ClientSpecified = client.IsSome
        LocAddress=
            match server with 
            | Some address -> address // "LOC-TC-01";
            | None -> "<NO LOC SERVER CONFIGURED>"
        CurrentClient=
            match client with
            | Some client -> client
            | None -> "<NO CLIENT SELECTED>" //  "LXA".ToUpper()
    }


let private writeConfig key value = 
    let config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None)
    let settings = config.AppSettings.Settings
    match settings.[key] with 
    | null -> settings.Add(key, value)
    | x    -> x.Value <- value
    config.Save(ConfigurationSaveMode.Modified)

let SetClient selectedClient = 
    writeConfig ClientKey selectedClient

let SetServer address = 
    writeConfig ServerKey address
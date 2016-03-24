module Config

type config = {
    LocAddress:string;
    CurrentClient:string
    }

    // yeah, need to load this stuff in from ...settings...
let get =
    {
        LocAddress="LOC-TC-01";
        CurrentClient="LXA".ToUpper()
    }
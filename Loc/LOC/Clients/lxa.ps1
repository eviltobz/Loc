$OctopusProjectName = "LXA-Rebrand"

$Packages = @( # Package name, active status - 1 will be built & deployed, 0 will be skipped in both build & deploy
    ("LXA.RendererExtensions", 1),
    ("Boarding.BoardingServices", 0),
    ("Boarding.DCS.Amadeus", 0)
    )


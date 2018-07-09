// namespace Loc.Tests
module Loc.Tests.Services

open NUnit.Framework
open FsUnitTyped

[<TestCase("15below.Mondia$LXA-LOC", "15below.Mondia", "LXA-LOC", "LXA")>]
[<TestCase("15below-LXA-LOC-ABCpdfHost", "15below-LXA-LOC-ABCpdfHost", null, "LXA")>]
[<TestCase("15below.GDSInteractionHost$LY-LOC-AW1", "15below.GDSInteractionHost", "LY-LOC-AW1", "LY")>]
[<TestCase("15below-LY-LOC-Renderer$RH1", "15below-LY-LOC-Renderer", "RH1", "LY")>]
[<TestCase("15below-LY-LOC-Scheduler$SCH.RA01", "15below-LY-LOC-Scheduler", "SCH.RA01", "LY")>]
[<TestCase("15below-LXA-LOC-GDSInteractionManager$AWA", "15below-LXA-LOC-GDSInteractionManager", "AWA", "LXA")>]
[<TestCase("EasyNetQ.Host$MicrositeSagasHost.LY-LOC", "EasyNetQ.Host", "MicrositeSagasHost.LY-LOC", "LY")>]
[<TestCase("not a 15b name format", null, null, null)>]
let ExtractServiceDetails_CanParseOurNames (fullname, name, instance, client) =
    let (actualName, actualInstance, actualClient) = Services.extractServiceDetails(fullname)
    actualName |> shouldEqual name
    actualInstance |> shouldEqual instance
    actualClient |> shouldEqual client

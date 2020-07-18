namespace CoolapkUWP.Core.Test

open CoolapkUWP.Core.Helpers
open NUnit.Framework
open System

module UtilTest =
    let [<SetUp>]Setup () = ()

    let areEqual (expected:'a) (actual:'a) = Assert.AreEqual (expected, actual)

    [<Test>]
    let testMD5 () =
        Utils.GetMD5 "1122"
        |> areEqual "3b712de48137572f3849aabd5666a4e3"

    let sourceDateTime =
        [
            let random = Random()
            for _ = 0 to 9 do
                DateTime (
                    random.Next(1970, 2050),
                    random.Next(1, 13),
                    random.Next(1, 29),
                    random.Next(0, 24),
                    random.Next(0, 60),
                    random.Next(0, 60),
                    DateTimeKind.Utc)
        ]

    [<TestCaseSource("sourceDateTime"); Order(1)>]
    let testDateTimeToUnixTimeStamp (source:DateTime) =
        let stamp = (source.Ticks - 621355968000000000L) / 10000000L
        source
        |> Utils.ConvertDateTimeToUnixTimeStamp
        |> (stamp |> float |> areEqual)

    [<Test; Order(2)>]
    let testTime () =
        let span = TimeSpan (0, 2, 30)
        let now = DateTime.UtcNow
        let now' = now.Subtract span
        let areEqual' struct(a, b:obj) = 
            Assert.That(fun () ->
                let r1 = a = Utils.TimeIntervalType.MinutesAgo
                let r2 = (b :?> TimeSpan).Minutes = 2
                r1 && r2
            )
        let UnixTimeToReadable' time = Utils.ConvertUnixTimeStampToReadable(time, now)

        now'
        |> Utils.ConvertDateTimeToUnixTimeStamp
        |> UnixTimeToReadable'
        |> areEqual'
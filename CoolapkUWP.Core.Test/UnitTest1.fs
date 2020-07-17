module CoolapkUWP.Core.Test

open System
open CoolapkUWP.Core.Helpers
open NUnit.Framework

[<SetUp>]
let Setup () = ()

let areEqual expected actual = Assert.AreEqual (expected, actual)

[<Test>]
let MD5'Test () =
    DataHelper.GetMD5 "1122"
    |> areEqual "3b712de48137572f3849aabd5666a4e3"

[<Test>]
let Time'Test () =
    DateTime (2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    |> DataHelper.ConvertTimeToUnix
    |> areEqual 1577836800
    
    TimeSpan (0, 2, 30)
    |> DateTime.Now.ToUniversalTime().Subtract
    |> DataHelper.ConvertTimeToUnix
    |> DataHelper.ConvertUnixTimeToReadable
    |> areEqual "2\u5206\u949f\u524d"
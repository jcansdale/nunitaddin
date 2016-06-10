#light

open NUnit.Framework

[<Test>]
let add() =
    Assert.That(1 + 2, Is.EqualTo(3))

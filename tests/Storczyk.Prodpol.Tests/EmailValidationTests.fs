module Storczyk.Prodpol.Tests.EmailValidationTests

open System.Text.RegularExpressions
open NUnit.Framework

let emailRegex = """^(([a-zA-Z\-_.+/]+)|("([+.a-zA-Z_\-]+)"))+@([a-zA-Z]+)(\.([a-zA-Z])+)*$"""

[<TestCase("nowaczka@gmail.com")>]
[<TestCase("test@example.com")>]
[<TestCase("user.name+tag@domain.co")>]
[<TestCase("user_name@domain.com")>]
[<TestCase("a@b.co")>]
[<TestCase("admin@opencode.ai")>]
let ``Valid emails pass backend regex`` (email: string) =
    Assert.That(Regex.IsMatch(email, emailRegex), Is.True)

[<TestCase("not-an-email")>]
[<TestCase("@domain.com")>]
[<TestCase("user@")>]
[<TestCase("user@.com")>]
[<TestCase("")>]
let ``Invalid emails fail backend regex`` (email: string) =
    Assert.That(Regex.IsMatch(email, emailRegex), Is.False)

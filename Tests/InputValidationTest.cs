// dotnet add package NUnit
using NUnit.Framework;

[TestFixture]
public class TestInputValidation
{
	[Test]
	public void TestSQLInjection()
	{
		string injection = "admin' OR '1'='1";
		bool valid = AuthService.LoginUser(injection, "<password>");
		Assert.That(valid, Is.False, "SQL Injection input validation failed!");
	}

	[Test]
	public void TestEmailValidation()
	{
		string malformed = "user@domain..com";
		bool valid = ValidationHelpers.IsValidEmail(malformed);
		Assert.That(valid, Is.False, "Email validation failed!");
	}

	[Test]
	public void TestXSSInput()
	{
		string xss = "<script>alert('XSS');</script>";
		bool valid = ValidationHelpers.IsValidXSSInput(xss);
		Assert.That(valid, Is.False, "XSS input validation failed!");
	}
}
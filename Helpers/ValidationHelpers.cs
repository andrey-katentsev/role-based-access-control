using System.Net.Mail;
public static class ValidationHelpers
{
	public static bool IsValidInput(string input, string allowedSpecialCharacters = "")
	{
		if (string.IsNullOrEmpty(input))
			return false;

		var validCharacters = allowedSpecialCharacters.ToHashSet();
		return input.All(c => char.IsLetterOrDigit(c) || validCharacters.Contains(c));
	}

	public static bool IsValidEmail(string email)
	{
		if (string.IsNullOrEmpty(email))
			return false;

		try
		{
			var mail = new MailAddress(email);
			return mail.Address == email;
		}
		catch
		{
			return false;
		}
	}

	public static bool IsValidXSSInput(string input)
	{
		if (string.IsNullOrEmpty(input))
			return true;

		if(input.ToLower().Contains("<script") || input.ToLower().Contains("<iframe"))
			return false;

		return true;
	}
}
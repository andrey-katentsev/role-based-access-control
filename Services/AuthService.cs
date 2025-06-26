// dotnet add package Microsoft.Data.SqlClient
using Microsoft.Data.SqlClient;

public class AuthService
{
public static bool LoginUser(string username, string password)
{
	string allowedSpecialCharacters = "!@#$%^&*?";
	if (!ValidationHelpers.IsValidInput(username) || !ValidationHelpers.IsValidInput(password, allowedSpecialCharacters))
		return false;

	using (var connection = new SqlConnection("Database=SafeVault;User Id=SafeLogin;Password=p@s$w0rd;"))
	{
		string query = "SELECT COUNT(1) FROM Users WHERE Username = @Username AND Password = @Password";
		using (var command = new SqlCommand(query, connection))
		{
			command.Parameters.AddWithValue("@Username", username);
			command.Parameters.AddWithValue("@Password", password);
			connection.Open();
			int count = (int)command.ExecuteScalar();
			return count > 0;
		}
	}
}
}
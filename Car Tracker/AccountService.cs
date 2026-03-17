using MobileCarRecoverySystem.Models;

namespace MobileCarRecoverySystem.Services
{
    public class AccountService
    {
        private List<UserAccount> users = new List<UserAccount>();

        public AccountService()
        {
            users.Add(new UserAccount
            {
                Username = "admin",
                Password = "admin123"
            });
        }

        public bool Register(UserAccount user)
        {
            foreach (var existingUser in users)
            {
                if (existingUser.Username == user.Username)
                {
                    return false;
                }
            }

            users.Add(user);
            return true;
        }

        public bool Login(string username, string password)
        {
            foreach (var user in users)
            {
                if (user.Username == username && user.Password == password)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
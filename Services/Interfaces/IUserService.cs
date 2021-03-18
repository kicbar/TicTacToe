using System.Threading.Tasks;
using TicTacToe.Models;

namespace TicTacToe.Services.Interfaces
{
    public interface IUserService
    {
        Task<bool> RegisterUser(UserModel userModel);
        Task<UserModel> GetUserByEmail(string email);
        Task UpdateUser(UserModel userModel);
    }
}
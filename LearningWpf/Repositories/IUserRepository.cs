using LearningWpf.Models;

namespace LearningWpf.Repositories
{
    public interface IUserRepository
    {
        List<User> GetAllUsers();
    }
}
using OnlineLearningPlatform.DataAccess.Entities;
using System.Linq;

namespace OnlineLearningPlatform.DataAccess.IRepositories
{
    public interface IEnrollmentRepository : IGenericRepository<Enrollment>
    {
        IQueryable<Enrollment> GetQueryable();
    }
}

using OnlineLearningPlatform.DataAccess.IRepositories;
using OnlineLearningPlatform.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLearningPlatform.DataAccess.Repositories
{
    public class UserLessonProgressRepository : GenericRepository<UserLessonProgress>, IUserLessonProgressRepository
    {
        public UserLessonProgressRepository(AppDbContext context) : base(context)
        {
        }
    }
}

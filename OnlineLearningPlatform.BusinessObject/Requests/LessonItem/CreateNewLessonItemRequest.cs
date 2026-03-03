using OnlineLearningPlatform.BusinessObject.Requests.GradedItem;
using OnlineLearningPlatform.BusinessObject.Requests.LessonResource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLearningPlatform.BusinessObject.Requests.LessonItem
{
    public class CreateNewLessonItemRequest
    {
        public Guid LessonId { get; set; }
        public int OrderIndex { get; set; }
        public CreateGradedItemRequest? GradedItem { get; set; }
        public List<CreateLessonResourceRequest>? LessonResources { get; set; }
    }
}

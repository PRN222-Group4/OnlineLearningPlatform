using System;
using System.Collections.Generic;

namespace OnlineLearningPlatform.DataAccess.Entities;

public partial class SubmissionAnswerOption
{
    public Guid SubmissionAnswerOptionId { get; set; }

    public Guid QuestionSubmissionId { get; set; }

    public Guid AnswerOptionId { get; set; }

    public decimal Score { get; set; }

    public virtual AnswerOption AnswerOption { get; set; } = null!;

    public virtual QuestionSubmission QuestionSubmission { get; set; } = null!;
}

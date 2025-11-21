namespace EduPortal.Domain.Enums;

public enum VisaStatus
{
    NotStarted = 0,
    DocumentsPreparation = 1,
    ApplicationSubmitted = 2,
    InterviewScheduled = 3,
    InterviewCompleted = 4,
    UnderReview = 5,
    Approved = 6,
    Rejected = 7,
    AppealInProgress = 8
}

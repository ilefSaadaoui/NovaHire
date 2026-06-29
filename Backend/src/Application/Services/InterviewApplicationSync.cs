using Domain.Entities;

namespace Application.Services
{
    /// <summary>
    /// Aligne le statut pipeline de la candidature (ApplicationStatus)
    /// et le statut de chaque entretien (InterviewStatus).
    /// </summary>
    public static class InterviewApplicationSync
    {
        /// <summary>
        /// Met à jour les entretiens lorsque le recruteur déplace la carte Kanban.
        /// </summary>
        public static void SyncInterviewsFromApplicationStatus(
            ApplicationStatus applicationStatus,
            IList<Interview> interviews)
        {
            if (interviews.Count == 0)
                return;

            var utcNow = DateTime.UtcNow;

            switch (applicationStatus)
            {
                case ApplicationStatus.Interviewed:
                case ApplicationStatus.Accepted:
                case ApplicationStatus.OfferSent:
                    SetPlannedOrRescheduledTo(interviews, InterviewStatus.Completed, utcNow);
                    break;

                case ApplicationStatus.Rejected:
                    SetPlannedOrRescheduledTo(interviews, InterviewStatus.Cancelled, utcNow);
                    break;
            }
        }

        /// <summary>
        /// Met à jour la candidature lorsque le statut d'un entretien change.
        /// </summary>
        public static void SyncApplicationFromInterviewStatus(
            JobApplication application,
            InterviewStatus interviewStatus)
        {
            switch (interviewStatus)
            {
                case InterviewStatus.Completed:
                    if (application.Status is ApplicationStatus.Interview
                        or ApplicationStatus.Shortlisted
                        or ApplicationStatus.UnderReview)
                    {
                        application.Status = ApplicationStatus.Interviewed;
                        application.UpdatedAt = DateTime.UtcNow;
                    }
                    break;

                case InterviewStatus.Planned:
                case InterviewStatus.Rescheduled:
                    if (application.Status is ApplicationStatus.Submitted
                        or ApplicationStatus.UnderReview
                        or ApplicationStatus.Shortlisted)
                    {
                        application.Status = ApplicationStatus.Interview;
                        application.UpdatedAt = DateTime.UtcNow;
                    }
                    break;
            }
        }

        private static void SetPlannedOrRescheduledTo(
            IList<Interview> interviews,
            InterviewStatus targetStatus,
            DateTime utcNow)
        {
            foreach (var interview in interviews)
            {
                if (interview.Status is InterviewStatus.Planned or InterviewStatus.Rescheduled)
                {
                    interview.Status = targetStatus;
                    interview.UpdatedAt = utcNow;
                }
            }
        }
    }
}

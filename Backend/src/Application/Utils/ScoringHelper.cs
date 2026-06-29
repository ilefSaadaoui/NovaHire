namespace Application.Utils
{
    public static class ScoringHelper
    {
        /// <summary>
        /// Calcule le score global d'un candidat en fonction du score IA (sur CV) et du score Quiz.
        /// Donne un poids de 60% à l'IA (compétences/expérience) et 40% au Quiz (connaissances).
        /// </summary>
        public static int CalculateOverallScore(int? aiScore, int? quizScore)
        {
            if (!aiScore.HasValue && !quizScore.HasValue) return 0;
            if (!aiScore.HasValue) return quizScore!.Value;
            if (!quizScore.HasValue) return aiScore.Value;

            double weightedAi = aiScore.Value * 0.6;
            double weightedQuiz = quizScore.Value * 0.4;

            return (int)System.Math.Round(weightedAi + weightedQuiz);
        }
    }
}

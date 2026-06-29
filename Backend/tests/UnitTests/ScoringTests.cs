using Xunit;
using Application.Utils;

namespace UnitTests
{
    public class ScoringTests
    {
        [Fact]
        public void CalculateOverallScore_ShouldWeightAiAndQuizProperly()
        {
            // Arrange
            int aiScore = 80;
            int quizScore = 90;

            // Act
            // AI weight is 60%, Quiz weight is 40%
            // Expected: (80 * 0.6) + (90 * 0.4) = 48 + 36 = 84
            int result = ScoringHelper.CalculateOverallScore(aiScore, quizScore);

            // Assert
            Assert.Equal(84, result);
        }

        [Fact]
        public void CalculateOverallScore_ShouldReturnAiScore_IfNoQuizScore()
        {
            // Arrange
            int aiScore = 75;
            int? quizScore = null;

            // Act
            int result = ScoringHelper.CalculateOverallScore(aiScore, quizScore);

            // Assert
            Assert.Equal(75, result);
        }

        [Fact]
        public void CalculateOverallScore_ShouldReturnQuizScore_IfNoAiScore()
        {
            // Arrange
            int? aiScore = null;
            int quizScore = 60;

            // Act
            int result = ScoringHelper.CalculateOverallScore(aiScore, quizScore);

            // Assert
            Assert.Equal(60, result);
        }

        [Fact]
        public void CalculateOverallScore_ShouldReturnZero_IfBothNull()
        {
            // Act
            int result = ScoringHelper.CalculateOverallScore(null, null);

            // Assert
            Assert.Equal(0, result);
        }
    }
}

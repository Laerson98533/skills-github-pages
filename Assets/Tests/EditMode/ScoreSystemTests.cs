using NUnit.Framework;

namespace QuizRush.Tests.EditMode
{
    public class ScoreSystemTests
    {
        [Test]
        public void StreakIncreasesMultiplier()
        {
            var system = new ScoreSystem();
            system.Reset();

            system.RegisterCorrect(5f);
            Assert.AreEqual(1, system.Streak);
            Assert.AreEqual(1f, system.Multiplier);

            system.RegisterCorrect(3f);
            Assert.AreEqual(2, system.Streak);
            Assert.AreEqual(1.2f, system.Multiplier, 0.001f);
        }

        [Test]
        public void IncorrectResetsStreak()
        {
            var system = new ScoreSystem();
            system.Reset();
            system.RegisterCorrect(2f);
            system.RegisterCorrect(1f);
            Assert.Greater(system.Streak, 0);

            system.RegisterIncorrect();
            Assert.AreEqual(0, system.Streak);
            Assert.AreEqual(1f, system.Multiplier);
        }

        [Test]
        public void ScoreAccumulatesWithTimeBonus()
        {
            var system = new ScoreSystem();
            system.Reset();

            var points = system.RegisterCorrect(7.4f);
            Assert.AreEqual(points, system.Score);
            Assert.Greater(points, 100);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace QuizRush.Tests.PlayMode
{
    public class QuestionRepositoryTests
    {
        [UnityTest]
        public IEnumerator DoesNotRepeatWithinRound()
        {
            var go = new GameObject("Repository");
            var repository = go.AddComponent<QuestionRepository>();
            repository.LoadQuestions();
            repository.BeginRound();

            var seen = new HashSet<string>();
            for (int i = 0; i < 10; i++)
            {
                var question = repository.GetNextQuestion(2);
                Assert.IsNotNull(question, "Question should not be null");
                Assert.IsTrue(seen.Add(question.Text), "Question repeated within the same round");
            }

            Object.Destroy(go);
            yield break;
        }
    }
}

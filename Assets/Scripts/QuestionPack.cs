using System.Collections.Generic;
using UnityEngine;

namespace QuizRush
{
    /// <summary>
    /// ScriptableObject wrapper that can store a list of questions for authoring in the editor.
    /// Even though the default dataset is JSON driven, the repository can also ingest this asset.
    /// </summary>
    [CreateAssetMenu(fileName = "QuestionPack", menuName = "QuizRush/Question Pack", order = 0)]
    public class QuestionPack : ScriptableObject
    {
        [SerializeField] private List<Question> questions = new();

        /// <summary>
        /// Ordered list of questions contained in this pack.
        /// </summary>
        public IReadOnlyList<Question> Questions => questions;
    }
}

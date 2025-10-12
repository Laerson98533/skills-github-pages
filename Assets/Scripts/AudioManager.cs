using UnityEngine;

namespace QuizRush
{
    /// <summary>
    /// Manages the simple audio needs for Quiz Rush including mute toggle and SFX playback.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip correctClip;
        [SerializeField] private AudioClip incorrectClip;
        [SerializeField] private AudioClip tickClip;

        private bool muted;

        private void Awake()
        {
            var sources = GetComponents<AudioSource>();
            if (musicSource == null && sources.Length > 0)
            {
                musicSource = sources[0];
                musicSource.loop = true;
            }

            if (sfxSource == null)
            {
                if (sources.Length > 1)
                {
                    sfxSource = sources[1];
                }
                else
                {
                    sfxSource = gameObject.AddComponent<AudioSource>();
                }
            }

            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Applies a mute toggle to both music and SFX output.
        /// </summary>
        public void SetMuted(bool isMuted)
        {
            muted = isMuted;
            if (musicSource != null)
            {
                musicSource.mute = isMuted;
            }

            if (sfxSource != null)
            {
                sfxSource.mute = isMuted;
            }
        }

        /// <summary>
        /// Plays the feedback for a correct answer.
        /// </summary>
        public void PlayCorrect()
        {
            PlayClip(correctClip);
        }

        /// <summary>
        /// Plays the feedback for an incorrect answer.
        /// </summary>
        public void PlayIncorrect()
        {
            PlayClip(incorrectClip);
        }

        /// <summary>
        /// Plays a ticking sound for the countdown.
        /// </summary>
        public void PlayTick()
        {
            PlayClip(tickClip);
        }

        private void PlayClip(AudioClip clip)
        {
            if (muted || clip == null)
            {
                return;
            }

            sfxSource?.PlayOneShot(clip);
        }
    }
}

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamerator
{
    public class Sound
    {
        public float sound_timer;
        public float volume;
        public float volume_factor;
        public float duration;
        private SoundEffect sound;
        SoundEffectInstance sound_instance;

        public SoundEffectInstance getInstance { get { return sound_instance; } }

        // same name as audio files
        public enum SoundType { Hit_1, Hit_Spike, Hit_Chest, Destruction_1, Item_1, Critical_Hit_1, Monster_Growl_1, Raven_1 };

        public Sound(ContentManager content, SoundType sound_type, float duration, float volume)
        {
            sound = content.Load<SoundEffect>(sound_type.ToString());
            sound_instance = sound.CreateInstance();

            this.duration = duration;
            if (this.duration == 0)
                this.duration = (float)sound.Duration.TotalSeconds;

            this.volume = volume;
            if (this.volume > 1f)
                this.volume = 1f;

            sound_instance.Volume = this.volume;
            sound_timer = 0f;
            volume_factor = this.volume / this.duration;
        }

        public void UnloadContent()
        {
            if (sound_instance != null)
                sound_instance.Dispose();
        }

        public void Play()
        {
            sound_instance.Play();
        }

        public void Stop()
        {
            sound_instance.Stop();
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotControl.Services
{
    public abstract class MediaController
    {
        // General Update Method
        public abstract Task GetPlaybackInfoAsync();

        // Set hooks
        public abstract Task PreviousTrackAsync();
        public abstract Task PlayPauseAsync();
        public abstract Task NextTrackAsync();
        public abstract Task SetVolume(int volumePercent);
        public abstract Task SeekAsync(int positionMs);
        public abstract Task SetShuffleAsync(bool isOn);
        public abstract Task SetRepeatAsync(string mode);
    }
}

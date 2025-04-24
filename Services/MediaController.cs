using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotControl.Services
{
    public abstract class MediaController
    {
        public abstract Task PlayPauseAsync();
        public abstract Task NextTrackAsync();
        public abstract Task PreviousTrackAsync();
    }
}

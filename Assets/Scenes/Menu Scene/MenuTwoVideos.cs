using UnityEngine;
using UnityEngine.Video;

public class MenuTwoVideos : MonoBehaviour
{
    public VideoPlayer vp;
    public VideoClip video1;
    public VideoClip video2;

    void Start()
    {
        // play first video with its own embedded audio
        vp.audioOutputMode = VideoAudioOutputMode.Direct;
        vp.EnableAudioTrack(0, true);
        vp.SetDirectAudioMute(0, false);

        vp.isLooping = false;
        vp.clip = video1;

        vp.loopPointReached += OnVideo1Finished;
        vp.Play();
    }

    void OnVideo1Finished(VideoPlayer source)
    {
        // switch to second video (optionally loop this one)
        vp.loopPointReached -= OnVideo1Finished;

        vp.clip = video2;
        vp.isLooping = true;             // set false if you don't want it to loop

        // Prepare first to reduce the tiny hitch, then play
        vp.prepareCompleted += OnPreparedVideo2;
        vp.Prepare();
    }

    void OnPreparedVideo2(VideoPlayer source)
    {
        vp.prepareCompleted -= OnPreparedVideo2;
        vp.Play();                       // plays video2 with its own audio
    }
}

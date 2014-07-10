using UnityEngine;
using System.Collections;
using System.Threading;

/* Voice is in a separate thread to ensure there is no delay in delivering data to the recognition module. */

public class VoiceThread : MonoBehaviour {
    private System.Object cs=new System.Object();
	private System.Threading.Thread thread=null;
	private PXCUPipeline pp=null;
	private volatile bool stop=false;
	private volatile string line;
	private string text="\n\n\n\n\n\n\n\n";

    void Start () {
		if ((Options.mode&PXCUPipeline.Mode.VOICE_RECOGNITION)==0) return;
		
		pp=new PXCUPipeline();
		if (!pp.Init(PXCUPipeline.Mode.VOICE_RECOGNITION)) {
			print("Failed to initialize PXCUPipeline for voice recognition");
			return;
		}
		
		/* It's critical to set the volume for recognition because the camera is very sensitive */
		float[] volume=new float[1]{0.2f};
		pp.SetDeviceProperty(PXCMCapture.Device.Property.PROPERTY_AUDIO_MIX_LEVEL,volume);
		
		stop=false; 
		line=null;
		thread=new Thread(ThreadRoutine);
		thread.Start();
    }
    
    void OnDisable() {
		if (thread!=null) {
			stop=true;
			Thread.Sleep(5);
			thread.Join();
			thread=null;
		}
		
		if (pp!=null) {
			pp.Dispose();
			pp=null;
		}
    }

    void Update () {
		lock (cs) {
			if (line!=null) {
				text=text+line+"\n";
				text=text.Substring(text.IndexOf("\n")+1);
				GameObject.Find ("Console").guiText.text=text;
				line=null;
			}
		}
    }
	
	private void ThreadRoutine() {
		while (pp.AcquireFrame(true) && !stop) {
			PXCMVoiceRecognition.Recognition rdata;
			if (pp.QueryVoiceRecognized(out rdata)) {
				lock (cs) {
					line="voice (label="+rdata.label+", text="+rdata.dictation+")";
				}
			}
			pp.ReleaseFrame();
		}
	}
}

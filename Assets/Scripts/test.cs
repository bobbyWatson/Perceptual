using UnityEngine;
using System.Collections;

public class test : MonoBehaviour {
	public float rotativite;
	public float frein;

	private PXCUPipeline.Mode mode = PXCUPipeline.Mode.GESTURE;
	private PXCUPipeline pp;
	private PXCMGesture.Gesture handLeftGesture;
	private PXCMGesture.Gesture handRightGesture;
	private PXCMGesture.GeoNode node;
	private PXCMGesture.GeoNode index;
	private float rotateForceX = 0.0f;
	private float rotateForceY = 0.0f;

	void Start () {
		pp=new PXCUPipeline();
		pp.Init(mode);
	}

	void Update () {
		if (!pp.AcquireFrame(false)) return;
		
		pp.QueryGesture(PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_LEFT, out handLeftGesture);
		pp.QueryGesture(PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_RIGHT, out handRightGesture);

		pp.QueryGeoNode(PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_RIGHT, out node);

		if(rotateForceX > 0)
			rotateForceX -= frein;
		if(rotateForceX < 0)
			rotateForceX += frein;

		if(rotateForceY > 0)
			rotateForceY -= frein;
		if(rotateForceY < 0)
			rotateForceY += frein;


		if(handLeftGesture.label == PXCMGesture.Gesture.Label.LABEL_NAV_SWIPE_LEFT ||
			handRightGesture.label == PXCMGesture.Gesture.Label.LABEL_NAV_SWIPE_LEFT){
			rotateForceX += rotativite;
		}

		if(handLeftGesture.label == PXCMGesture.Gesture.Label.LABEL_NAV_SWIPE_RIGHT ||
		   handRightGesture.label == PXCMGesture.Gesture.Label.LABEL_NAV_SWIPE_RIGHT){
			rotateForceX -= rotativite;
		}

		if(handLeftGesture.label == PXCMGesture.Gesture.Label.LABEL_NAV_SWIPE_DOWN ||
		   handRightGesture.label == PXCMGesture.Gesture.Label.LABEL_NAV_SWIPE_DOWN){
			rotateForceY -= rotativite;
		}

		if(handLeftGesture.label == PXCMGesture.Gesture.Label.LABEL_NAV_SWIPE_UP ||
		   handRightGesture.label == PXCMGesture.Gesture.Label.LABEL_NAV_SWIPE_UP){
			rotateForceY += rotativite;
		}

		if(handLeftGesture.label == PXCMGesture.Gesture.Label.LABEL_SET_POSE ||
		   handRightGesture.label == PXCMGesture.Gesture.Label.LABEL_SET_POSE){
			rotateForceY = 0;
			rotateForceX = 0;
		}

		transform.Rotate(-Vector3.down * rotateForceX * Time.deltaTime);
		transform.Rotate(-Vector3.left * rotateForceY * Time.deltaTime);

		pp.ReleaseFrame();
	}
}

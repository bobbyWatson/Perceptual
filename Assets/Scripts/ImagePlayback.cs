/*******************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2012 Intel Corporation. All Rights Reserved.

*******************************************************************************/
using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class ImagePlayback : MonoBehaviour {
    private Texture2D 	 		rgbImage=null;
	private Texture2D	 		labelMapImage=null;
	private short[]		 		depthmap=null;
	private Vector3[]			pos2d=null;
	private float[]             untrusted=new float[2]{0,0};
	private PXCUPipeline 		pp=null;
	private PXCMFaceAnalysis.Landmark.LandmarkData[] 	ldata=new PXCMFaceAnalysis.Landmark.LandmarkData[2];
	private PXCMGesture.GeoNode[] 						ndata=new PXCMGesture.GeoNode[5];
	
    void Start () {
		/* VoiceRecognition is in a separate thread. See VoiceThread.cs. */
		PXCUPipeline.Mode mode=Options.mode&(~PXCUPipeline.Mode.VOICE_RECOGNITION);
		if (mode==0) return;
		
		pp=new PXCUPipeline();
		if (!pp.Init(mode)) {
			print("Unable to initialize the PXCUPipeline");
			return;
		}
		
	    int[] size=new int[2];
        if (pp.QueryDepthMapSize(size)) {
			labelMapImage=new Texture2D(size[0],size[1],TextureFormat.ARGB32, false);
			GameObject.Find("LabelMap").renderer.material.mainTexture=labelMapImage;
			
			pp.QueryDeviceProperty(PXCMCapture.Device.Property.PROPERTY_DEPTH_SATURATION_VALUE,untrusted);
			depthmap=new short[size[0]*size[1]];
			pos2d=new Vector3[size[0]*size[1]];
			for (int xy=0,y=0;y<size[1];y++)
				for (int x=0;x<size[0];x++,xy++)
					pos2d[xy]=new Vector3(x,y,0);
		}
		
		if (pp.QueryRGBSize(size)) {
			rgbImage=new Texture2D(size[0],size[1],	TextureFormat.ARGB32, false);
			GameObject.Find("RGB").renderer.material.mainTexture=rgbImage;
		}
    }
    
    void OnDisable() {
		if (pp==null) return;
		pp.Dispose();
		pp=null;
    }

    void Update () {
		if (pp==null) return;
		if (!pp.AcquireFrame(false)) return;

		if (labelMapImage!=null) if (pp.QueryLabelMapAsImage(labelMapImage)) labelMapImage.Apply();
		if (rgbImage!=null) if (pp.QueryRGB(rgbImage)) {
			if (depthmap!=null) if (pp.QueryDepthMap(depthmap)) AddProjection ();
			rgbImage.Apply(); 
		}
		
		for (int i=0;;i++) {
			int face; ulong timeStamp;
			if (!pp.QueryFaceID(i,out face, out timeStamp)) break;
			print("face "+i+" (id=" + face + ", timeStamp=" + timeStamp+")");
				
			PXCMFaceAnalysis.Detection.Data ddata;
			if (pp.QueryFaceLocationData(face,out ddata))
				print ("\tlocation(id="+face+", x="+ddata.rectangle.x+", y="+ddata.rectangle.y+", w="+ddata.rectangle.w+", h="+ddata.rectangle.h+")");

			if (pp.QueryFaceLandmarkData(face, PXCMFaceAnalysis.Landmark.Label.LABEL_6POINTS,ldata))
				print ("\tleft-eye (id="+face+", x="+(ldata[0].position.x+ldata[1].position.x)/2+", y="+(ldata[0].position.y+ldata[1].position.y)/2+")");
		}
		
		if (pp.QueryGeoNode(PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_PRIMARY,ndata))
			print ("geonode palm (x="+ndata[0].positionImage.x+", y="+ndata[0].positionImage.y+")");
		
		PXCMGesture.Gesture gdata;
		if (pp.QueryGesture(PXCMGesture.GeoNode.Label.LABEL_ANY, out gdata))
			print ("gesture (label="+gdata.label+")");
		
		pp.ReleaseFrame();
    }
	
	void AddProjection() {
		for (int xy=0;xy<pos2d.Length;xy++)
			pos2d[xy].z=depthmap[xy];
		
		Vector2[] posc;
		if (!pp.MapDepthToColorCoordinates(pos2d,out posc)) return;
		
		Color32[] pixels=rgbImage.GetPixels32();
		for (int xy=0;xy<posc.Length;xy++) {
			if (depthmap[xy]==untrusted[0] || depthmap[xy]==untrusted[1]) continue;
			int x=(int)posc[xy].x, y=(int)posc[xy].y;
			if (x<0 || x>=rgbImage.width || y<0 || y>=rgbImage.height) continue;
			pixels[y*rgbImage.width+x]=new Color32(0,255,0,255);
		}
		rgbImage.SetPixels32(pixels);
	}
}

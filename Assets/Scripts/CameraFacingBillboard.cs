// Billboard from:          http://www.unifycommunity.com/wiki/index.php?title=CameraFacingBillboard
// Y-version adapted from: http://forum.unity3d.com/viewtopic.php?t=897
// modified by:             Wolfram Kresse

using UnityEngine;

namespace NMY.CameraTools {
	// Billboard: have transform always face camera
	// note: local rotation will be overwritten
	// note: +Z is considered "front", so you need to locally orient your billboard object accordingly
	//
	// For "real" billboards (Yonly==false, strict==false):
	// - to translate the billboard in the camera plane, use its transform.localPosition
	// - to translate the billboard in world space (e.g., keep positioned "above" an object in world-up), use the transform.localPosition of its children
	// translation of billboards with Yonly==true will always be in world space (and therefore world-up aligned)
	//
	// The new variant Yonly==false, strict==true represents the InstantReality behaviour "<Billboard axisOfRotation='0 0 0'>".
	//
	// bool fixedScreensize:
	// 		true:   keep button at constant size; localScale=(1,1,1) means its native texture size in screen space
	// 		        generally, you'll want to use real billboards for this (Yonly==false)
	// 		false:  scale with perspective; localScale=(1,1,1) means 100 pixels == 1 world coord (cf. textureButton.cs)
	// bool Yonly:
	// 		true:   keep XZ-orientation, Y-rotation only (trees, flames, ...)
	// 		false:  strict==false: complete billboard, parallel to camera plane (text, hotspots, ...)
	// bool strict:
	// 		true:   Yonly==true: keep in plane world.up/camera.right
	//              Yonly==false: billboard normal at billboard origin will always intersect camera
	// 		false:  Yonly==true: Y-orient towards camera (billboard normal at billboard origin will always intersect the line "camera.position+x*camera.up")
	//              Yonly==false: "normal" screenspace behaviour
	//
	// For orthographic cameras, the "strict" toggle is ignored, since it doesn't make sense there.
	// NOTE: the effect of "strict" is kinda inverted between Yonly true and false, either for historic reasons - or no reason at all...
	public class CameraFacingBillboard : MonoBehaviour
	{
		public Camera cameraToLookAt;
		public bool fixedScreensize=false;
		public bool scaleOnly=false; // disables the whole Billboard functionality, except fixedScreensize, ignoreParentScaling
		public bool Yonly=false;
		public bool strict=false;
		public bool useCameraUp=false;
		public bool ignoreParentScaling=false; // true: apply local scaling, but ignore any scaling of any parents
		public bool invert = false;
	
		// scale factor to keep button normalized in screen space
		public float scaleFactor=1f;
		public int targetScreenResolution=1080;
		private float origScaleFactor;
		private Vector3 origScale;
		private Vector3 compensatedScale;
		private Vector3 up;

		public bool readyToAwake=true; // technically, it's a "readyToUpdate". but who's picky.

		void Start()
		{
			if(scaleOnly&&!fixedScreensize){
				Debug.LogWarning("Billboard: activating scaleOnly mandates fixedScreensize to be true.");
				fixedScreensize=true;
			}

			if(!cameraToLookAt)
				cameraToLookAt=Camera.main;
			origScale=transform.localScale;
			origScaleFactor=scaleFactor;
			compensatedScale=origScale;
		}

		void LateUpdate() // use LateUpdate, so we process this *after* any camera changes
		{
			if(!readyToAwake)
				return;
			if(!cameraToLookAt) // WK HACK if we loose our camera
				cameraToLookAt=Camera.main;
		
			if(!cameraToLookAt){ // emergency bailout - if this happens, your scene configuration is wrong
				Debug.LogError(NMY.StaticUtils.GetFullName(gameObject)+" does not have a MainCamera!");
				return;
			}

			// this value displays menu textures always at 100% pixel size.
			//   pro: menue elements are always shown at their native resolution
			//   con: for smaller screen resolutions the buttons will look too big
			//scaleFactor=origScaleFactor*100f/Screen.height;
			//
			// this value scales the buttons so that they have 100% pixel size at the given targetScreenResolution,
			// and adjusts the relative sizes for smaller screen resolutions
			//   pro: menu elements will be proportionally correct for smaller screen resolutions
			//   con: hardcoded target screen resolution
			scaleFactor=origScaleFactor*100f/targetScreenResolution;

			if(useCameraUp)
				up=cameraToLookAt.transform.up;
			else
				up=Vector3.up;
		
			if(ignoreParentScaling)
				compensatedScale=new Vector3(origScale.x*transform.localScale.x/transform.lossyScale.x,
											 origScale.y*transform.localScale.y/transform.lossyScale.y,
											 origScale.z*transform.localScale.z/transform.lossyScale.z);
			if(fixedScreensize){
				float distance=1;
				if(cameraToLookAt.orthographic){
					distance*=scaleFactor*cameraToLookAt.orthographicSize*2;
				}else{
					distance=Vector3.Dot(transform.position-cameraToLookAt.transform.position,cameraToLookAt.transform.forward);
					distance*=scaleFactor*Mathf.Tan(cameraToLookAt.fieldOfView/2*Mathf.Deg2Rad)*2/cameraToLookAt.rect.height;
				}
				transform.localScale=new Vector3(compensatedScale.x*distance,
												 compensatedScale.y*distance,
												 compensatedScale.z*distance);
			}else if(ignoreParentScaling)
				transform.localScale=compensatedScale;
		
			if(!scaleOnly){
				if(Yonly){
					if(strict || cameraToLookAt.orthographic){
						transform.LookAt(transform.position-
										new Vector3(cameraToLookAt.transform.forward.x,
													0,
													cameraToLookAt.transform.forward.z),up);
					}else{
						transform.LookAt(new Vector3(cameraToLookAt.transform.position.x,
													transform.position.y,
													cameraToLookAt.transform.position.z),up);
					}
				}else{
					if(strict && !cameraToLookAt.orthographic){
						transform.LookAt(new Vector3(cameraToLookAt.transform.position.x,
													cameraToLookAt.transform.position.y,
													cameraToLookAt.transform.position.z),up);
					}else{
						transform.LookAt(transform.position-cameraToLookAt.transform.forward,up);
					}
				}
				if (invert)
					transform.Rotate(0f,180f,0f);
			}
		
		
			// this is the same (does not handle the new case !Yonly&&strict)
			/*
       		transform.LookAt(new Vector3((!Yonly||strict?transform.position.x:0),
       													 transform.position.y,
       									 (!Yonly||strict?transform.position.z:0))+
       						 new Vector3((!Yonly||strict?-cameraToLookAt.transform.forward.x:cameraToLookAt.transform.position.x),
       									 (!Yonly        ?-cameraToLookAt.transform.forward.y:0),
       									 (!Yonly||strict?-cameraToLookAt.transform.forward.z:cameraToLookAt.transform.position.z)));
			*/

			// // billboard, full version, probably accounting for different up vectors(?)
       		// transform.LookAt(transform.position + cameraToLookAt.transform.rotation * Vector3.back,
			//   	cameraToLookAt.transform.rotation * Vector3.up);
		}
	}
}

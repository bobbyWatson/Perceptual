using UnityEngine;
using System.Collections;

public class ModifyTerrain : MonoBehaviour {

	private PXCUPipeline.Mode mode = PXCUPipeline.Mode.GESTURE;
	private PXCUPipeline pp;
	private PXCMGesture.Gesture handLeftGesture;
	private PXCMGesture.Gesture handRightGesture;
	private PXCMGesture.GeoNode index;
	private PXCMGesture.GeoNode node;

	public Texture2D cursorTexture;
	public Terrain myTerrain;
	public float size = 10;
	private Vector3 mousePos;
	private TerrainData data;
	private float[,] heights;
	private int[] res;
	private float currentHeight;
	private float modifiedHeight;
	private bool wasInBig5 = false;
	private float previousDepth;
	private float currentDepth;
	private bool changed;

	// Use this for initialization
	void Awake () {
		mousePos = Vector3.zero;
		data = myTerrain.terrainData;
		res = new int[]{data.heightmapWidth, data.heightmapHeight};
		heights = data.GetHeights(0,0, res[0], res[1]);
		node = new PXCMGesture.GeoNode();
		if(Application.platform != RuntimePlatform.WindowsWebPlayer)
			Screen.showCursor = false;
	}

	void Start () {
		pp=new PXCUPipeline();
		pp.Init(mode);
	}

	void OnDisable() {
		pp.Close();
	}
	
	// Update is called once per frame
	void Update () {
		changed = false;

		//use mouse
		if (!pp.AcquireFrame(false)){
			//get mouse position
			mousePos = Input.mousePosition;
			if(Input.GetMouseButton(0)){
				//action
				RaycastHit hit;
				Ray ray = Camera.main.ScreenPointToRay(mousePos);
				if(myTerrain.collider.Raycast(ray, out hit, Mathf.Infinity)){
					Vector3 coord = WorldToHeightMapPosition(hit.point);
					//set terraforming height
					if(!wasInBig5){
						currentHeight = heights[(int)coord.z,(int)coord.x];
						wasInBig5 = true;
					}else{
						//terraform
						modifiedHeight += Input.GetAxis("Mouse ScrollWheel") / 4;
						Terraform(coord);
					}
				}
			}else{
				wasInBig5 = false;
				modifiedHeight = 0;
			}

		}else{
			pp.QueryGesture(PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_RIGHT, out handRightGesture);
			//get hand position on screen
			mousePos.x = Mathf.Clamp((node.positionImage.x - 100) / 100 * Screen.width, 0, Screen.width);
			mousePos.x = Screen.width - mousePos.x;
			mousePos.y = Mathf.Clamp((node.positionImage.y - 50) / 100 * Screen.height, 0, Screen.height);
			//terraforming
			if(handRightGesture.label == PXCMGesture.Gesture.Label.LABEL_POSE_BIG5){
				RaycastHit hit;
				Ray ray = Camera.main.ScreenPointToRay(mousePos);
				if(myTerrain.collider.Raycast(ray, out hit, Mathf.Infinity)){
					Vector3 coord = WorldToHeightMapPosition(hit.point);
					//swtich to terraforming mode;
					if(!wasInBig5){
						currentHeight = heights[(int)coord.z,(int)coord.x];
						wasInBig5 = true;
					}
					//terraform
					else{
						//change the height 
						currentDepth = node.positionWorld.y;
						if(previousDepth != null)
							modifiedHeight += currentDepth - previousDepth;
						previousDepth = currentDepth;
						//change terrain
						Terraform(coord);
					}
				}
			}
			//exit terraforming mode
			else{
				wasInBig5 = false;
				modifiedHeight = 0;
			}
			pp.ReleaseFrame();
		}
		if(changed){
			data.SetHeights(0,0, heights);
		}
	}

	void Terraform(Vector3 coord){
		changed = true;
		int[] _coord = new int[]{(int)coord.z,(int)coord.x};
		for(int i = (int)-size; i < size; i++){
			if(_coord[0] + i > 0 && _coord[0] +i < heights.Length/2){
				int _i = Mathf.Abs(i);
				for(int j = (int)-size; j < size ; j++){
					if(_coord[1] + j > 0 && _coord[1] + j < heights.Length / 2){
						int _j = Mathf.Abs(j);
						float dist = Mathf.Sqrt(_i*_i + _j*_j) / size;
						if(dist <= 1){
							dist = Mathf.Clamp(dist, 0, 1);
							heights[_coord[0] + i, _coord[1] + j] = currentHeight + modifiedHeight;
						}
					}
				}
			}
		}
	}

	Vector3 WorldToHeightMapPosition(Vector3 position){
		Vector3 res = position;
		res -= myTerrain.transform.position;
		res.x /= data.size.x;
		res.z /= data.size.z;
		res.x = res.x * data.heightmapWidth;
		res.z = res.z * data.heightmapHeight;
		return res;
	}


	void OnGUI (){
		GUI.DrawTexture(new Rect(mousePos.x - cursorTexture.width/8 , Screen.height - mousePos.y - cursorTexture.height/8, cursorTexture.width/4, cursorTexture.height/4), cursorTexture);
	}
}

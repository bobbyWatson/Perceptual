using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {

	void Awake(){
		if(Application.loadedLevelName == "menu"){
			Screen.showCursor = true;
		}
	}

	void OnGUI (){
		if(Application.loadedLevelName == "menu"){
			if(GUI.Button(new Rect(Screen.width/2-50, Screen.height/2 -100, 100, 50),"Cube")){
				Application.LoadLevel(1);
			}
			if(GUI.Button(new Rect(Screen.width/2-50, Screen.height/2, 100, 50),"Terraforming")){
				Application.LoadLevel(2);
			}
# if !UNITY_WEBPLAYER
			if(GUI.Button(new Rect(Screen.width/2-50, Screen.height/2 + 100, 100, 50),"Quit")){
				Application.Quit();
			}
#endif
		}else{
			if(GUILayout.Button("back to menu")){
				Application.LoadLevel(0);
			}
		}
	}
}

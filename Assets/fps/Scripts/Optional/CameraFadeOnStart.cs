// by @torahhorse

using UnityEngine;
using System.Collections;

public class CameraFadeOnStart : MonoBehaviour
{
	public bool fadeInWhenSceneStarts = true;
	public Color fadeColor = Color.black;
	public float fadeTime = 5f;

	void Awake ()
	{
		transform.position = new Vector3(transform.position.x, transform.position.y + 0.7f, transform.position.z);
		if( fadeInWhenSceneStarts )
		{
			Fade();
			
		}
	}
	
	public void Fade()
	{
		CameraFade.StartAlphaFade(fadeColor, true, fadeTime);
	}
}

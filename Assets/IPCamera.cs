using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.IO;

public class IPCamera : MonoBehaviour {

	public MeshRenderer frame;

	private string sourceURL = "http://24.172.4.142/mjpg/video.mjpg?COUNTER";
	private Texture2D texture; 
	private Stream stream;

	public void Start() {
		GetVideo();
	}

	public void GetVideo(){
		texture = new Texture2D(2, 2); 
		HttpWebRequest req = (HttpWebRequest) WebRequest.Create( sourceURL );
		req.Credentials = new NetworkCredential("username", "password");
		WebResponse resp = req.GetResponse();
		stream = resp.GetResponseStream();
		StartCoroutine (GetFrame ());
	}

	IEnumerator GetFrame (){
		Byte [] JpegData = new Byte[65536];

		while(true) {
			int bytesToRead = FindLength(stream);
			if (bytesToRead == -1) {
				print("End of stream");
				yield break;
			}

			int leftToRead=bytesToRead;

			while (leftToRead > 0) {
				leftToRead -= stream.Read (JpegData, bytesToRead - leftToRead, leftToRead);
				yield return null;
			}

			MemoryStream ms = new MemoryStream(JpegData, 0, bytesToRead, false, true);

			texture.LoadImage (ms.GetBuffer ());
			frame.material.mainTexture = texture;
			stream.ReadByte();
			stream.ReadByte();
		}
	}

	int FindLength(Stream stream)  {
		int b;
		string line="";
		int result=-1;
		bool atEOL=false;

		while ((b=stream.ReadByte())!=-1) {
			if (b==10) continue;
			if (b==13) {
				if (atEOL) {
					stream.ReadByte();
					return result;
				}
				if (line.StartsWith("Content-Length:")) {
					result=Convert.ToInt32(line.Substring("Content-Length:".Length).Trim());
				} else {
					line="";
				}
				atEOL=true;
			} else {
				atEOL=false;
				line+=(char)b;
			}
		}
		return -1;
	}
}

Shader "VertexLit No Z Writes" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
}

SubShader {
	Tags {"Queue" = "Transparent-2" }
	LOD 100
	
	// Normal rendering pass
	Pass {
		Material {
			Diffuse [_Color]
			Ambient [_Color]
		} 
		Lighting On
		ZWrite Off
		SeparateSpecular On
		SetTexture [_MainTex] {
			Combine texture * primary DOUBLE, texture * primary
		} 
	}
}
}

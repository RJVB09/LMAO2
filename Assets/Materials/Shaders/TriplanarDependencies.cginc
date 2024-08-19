float flip(float valueToFlip, float signV)
{
	return lerp(1-valueToFlip,valueToFlip,max(sign(signV),0));
}

float4 texTriplanar(sampler2D tex, float3 pos, float3 normal, float3 worldNormal)
{
	float3 face = pow(abs(normal.xyz),_BorderFade);
	face = face/(face.x+face.y+face.z);

	float4 texX = tex2D (tex, pos.zy*_TexScale*float2(sign(normal.x),1));
	float4 texY = tex2D (tex, pos.xz*_TexScale*float2(-1,-sign(normal.y)));
	float4 texZ = tex2D (tex, pos.xy*_TexScale*float2(-1,sign(normal.z)));

	return texX*face.x+texY*face.y+texZ*face.z;
}

float4 texTriplanarUVBased(sampler2D tex, float2 uvX, float2 uvY, float2 uvZ, float3 normal)
{
	float3 face = pow(abs(normal.xyz),_BorderFade);
	face = face/(face.x+face.y+face.z);

	float4 texX = tex2D (tex, uvX);
	float4 texY = tex2D (tex, uvY);
	float4 texZ = tex2D (tex, uvZ);

	return texX*face.x+texY*face.y+texZ*face.z;
}

float3x3 Inverse(float3x3 mat)
{
	float a11 = mat[0][0], a12 = mat[0][1], a13 = mat[0][2];
	float a21 = mat[1][0], a22 = mat[1][1], a23 = mat[1][2];
	float a31 = mat[2][0], a32 = mat[2][1], a33 = mat[2][2];

	float det = a11*a22*a33 + a12*a23*a31 + a13*a21*a32
				- a11*a23*a32 - a12*a21*a33 - a13*a22*a31;

	float3x3 adjugate = float3x3(
		a22*a33 - a23*a32, a13*a32 - a12*a33, a12*a23 - a13*a22,
		a23*a31 - a21*a33, a11*a33 - a13*a31, a13*a21 - a11*a23,
		a21*a32 - a22*a31, a12*a31 - a11*a32, a11*a22 - a12*a21
	);

	return adjugate / det;
}
#version 410

layout(location = 0) in vec3 aVertexPosition;
layout(location = 1) in vec3 aVertexNormal;
layout(location = 2) in vec2 aTextureCoord;
layout(location = 3) in vec3 aVertexTangent;
layout(location = 4) in vec3 aVertexBinormal;

uniform mat4 uMVMatrix;
uniform mat4 uPMatrix;
uniform mat3 uNMatrix;
uniform bool bLq;

out vec2 vTextureCoord;
out vec3 vTransformedNormal;
out vec4 vPosition;
out mat3 vTBN;

void main(void) {
    vPosition = uMVMatrix * vec4(aVertexPosition, 1.0);
    gl_Position = uPMatrix * vPosition;
    vTextureCoord = aTextureCoord;
    vTransformedNormal = uNMatrix * aVertexNormal;
	if (true) {
		vTBN = mat3(0.0);
	} else {
		vec3 T = normalize(vec3(uMVMatrix * vec4(aVertexTangent,   0.0)));
		vec3 B = normalize(vec3(uMVMatrix * vec4(aVertexBinormal, 0.0)));
		vec3 N = normalize(vec3(uMVMatrix * vec4(aVertexNormal,    0.0)));
		vTBN = mat3(T, B, N);
	}
}
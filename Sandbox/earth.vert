#version 410

in vec3 aVertexPosition;
in vec3 aVertexNormal;
in vec2 aTextureCoord;

uniform mat4 uMVMatrix;
uniform mat4 uPMatrix;
uniform mat3 uNMatrix;
uniform sampler2D uNormalMapSampler;

out vec2 vTextureCoord;
out vec3 vTransformedNormal;
out vec4 vPosition;

void main(void) {
    vPosition = uMVMatrix * vec4(aVertexPosition, 1.0);
    gl_Position = uPMatrix * vPosition;
    vTextureCoord = aTextureCoord;
    vTransformedNormal = uNMatrix * aVertexNormal;
}
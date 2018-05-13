#version 330

varying vec2 vTextureCoord;
varying vec3 vTransformedNormal;
varying vec4 vPosition;

uniform vec3 uAmbientColor;
uniform vec3 uPointLightingLocation;
uniform vec3 uPointLightingSpecularColor;
uniform vec3 uPointLightingDiffuseColor;
uniform sampler2D uColorMapSampler;
uniform sampler2D uSpecularMapSampler;
uniform sampler2D uNightMapSampler;
uniform sampler2D uNormalMapSampler;
uniform mat4 uMVMatrix;
uniform mat4 uPMatrix;
uniform mat3 uNMatrix;

void main(void) {
    vec4 fragmentColor = texture2D(uColorMapSampler, vTextureCoord);
    gl_FragColor = vec4(fragmentColor.rgb, fragmentColor.a);
}
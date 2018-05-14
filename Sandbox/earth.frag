#version 410

in vec4 vPosition;
in vec3 vTransformedNormal;
in vec2 vTextureCoord;

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

out vec4 fragColor;

void main(void) {
    vec3 lightDirection = normalize(uPointLightingLocation - vPosition.xyz);
    vec3 normal = normalize(vTransformedNormal);
    float specularLightWeighting = 0.0;
    float shininess = 32.0;
    shininess = texture2D(uSpecularMapSampler, vTextureCoord).r * 255.0;

    if (shininess < 255.0) {
        vec3 eyeDirection = normalize(-vPosition.xyz);
        vec3 reflectionDirection = reflect(-lightDirection, normal);
        specularLightWeighting = pow(max(dot(reflectionDirection, eyeDirection), 0.0), shininess / 32.);
    }

	vec3 normalMap = texture2D(uNormalMapSampler, vTextureCoord).rgb; 
    float diffuseLightWeighting = max(dot(normal, lightDirection), 0.0);
    vec3 lightWeighting = uAmbientColor
        + uPointLightingSpecularColor * specularLightWeighting
        + uPointLightingDiffuseColor * diffuseLightWeighting;
    vec4 fragmentColor = mix(
		texture2D(uNightMapSampler, vTextureCoord),
		texture2D(uColorMapSampler, vTextureCoord),
		pow(diffuseLightWeighting, 1.0/1.5)
	);

    fragColor = vec4(fragmentColor.rgb * lightWeighting, fragmentColor.a);
}
#version 330

varying vec2 vTextureCoord;
varying vec3 vTransformedNormal;
varying vec4 vPosition;

uniform vec3 uPointLightingLocation;

const float PI = 3.14159265359;
const float degToRad = PI / 180.0;
const float MAX = 10000.0;

const float DEG_TO_RAD = PI / 180.0;
float K_R = 0.166;
const float K_M = 0.0025;
const float E = 14.3;
const vec3 C_R = vec3(0.3, 0.7, 1.0);
const float G_M = -0.85;

uniform float fInnerRadius;
uniform float fOuterRadius;

float SCALE_H = 4.0 / (fOuterRadius - fInnerRadius);
float SCALE_L = 1.0 / (fOuterRadius - fInnerRadius);

const int numOutScatter = 8;
const float fNumOutScatter = 8.;
const int numInScatter = 80;
const float fNumInScatter = 80.0;

mat3 rot3xy( vec2 angle ) {
	vec2 c = cos( angle );
	vec2 s = sin( angle );
	
	return mat3(
		c.y      ,  0.0, -s.y,
		s.y * s.x,  c.x,  c.y * s.x,
		s.y * c.x, -s.x,  c.y * c.x
	);
}

vec3 rayDirection(vec3 cP) {
	vec4 ray = vPosition - vec4(cP, 1.0);
	return normalize(vec3(ray));
}

vec2 rayIntersection(vec3 p, vec3 dir, float radius ) {
	float b = dot( p, dir );
	float c = dot( p, p ) - radius * radius;
	
	float d = b * b - c;
	if ( d < 0.0 ) {
		return vec2( MAX, -MAX );
	}
	d = sqrt( d );
	
	float near = -b - d;
	float far = -b + d;
	
	return vec2(near, far);
}

// Mie
// g : ( -0.75, -0.999 )
//      3 * ( 1 - g^2 )               1 + c^2
// F = ----------------- * -------------------------------
//      2 * ( 2 + g^2 )     ( 1 + g^2 - 2 * g * c )^(3/2)
float miePhase( float g, float c, float cc ) {
	float gg = g * g;
	
	float a = ( 1.0 - gg ) * ( 1.0 + cc );

	float b = 1.0 + gg - 2.0 * g * c;
	b *= sqrt( b );
	b *= 2.0 + gg;	
	
	return 1.5 * a / b;
}

// Reyleigh
// g : 0
// F = 3/4 * ( 1 + c^2 )
float rayleighPhase( float cc ) {
	return 0.75 * ( 1.0 + cc );
}

//exp(fScaleOverScaleDepth * (fInnerRadius - fHeight));
float density(vec3 p) {
	return exp(-(length(p) - fInnerRadius) * SCALE_H);
}

float optic(vec3 p, vec3 q) {
	vec3 step = (q - p) / fNumOutScatter;
	vec3 v = p + step * 0.5;
	
	float sum = 0.0;
	for(int i = 0; i < numOutScatter; i++) {
		sum += density(v);
		v += step;
	}
	sum *= length(step)*SCALE_L;
	return sum;
}

vec3 inScatter(vec3 o, vec3 dir, vec2 e, vec3 l) {
	//fSampleLength = fFar / fSamples;
	float len = (e.y - e.x) / fNumInScatter;
	
	//v3SampleRay = v3Ray * fSampleLength;
	vec3 step = dir * len;
	//v3Start = v3CameraPos + v3Ray * fNear;
	vec3 p = o + dir * e.x;
	
	//fCameraAngle = dot(v3Ray, v3SamplePoint) / fHeight;
	//v3SamplePoint = v3Start + v3SampleRay * 0.5;
	vec3 v = p + dir * (len * 0.5);
	
	vec3 sum = vec3(0.0);
	for(int i = 0; i < numInScatter; i++) {
		
		vec2 f = rayIntersection(v, l, fOuterRadius);
		
		//fLightAngle = dot(v3LightDir, v3SamplePoint) / fHeight;
		vec3 u = v + l * f.y;
		
		//fScatter = scale() * scale()
		float n = (optic(p, v) + optic(v, u))*(PI * 4.0);
		
		//density() = //exp(fScaleOverScaleDepth * (fInnerRadius - fHeight));
		//v3Attenuate = * exp(-fScatter * (v3InvWavelength * fKr4PI + fKm4PI));
		sum += density(v)* exp(-n * ( K_R * C_R + K_M ));
		//v3SamplePoint += v3SampleRay;
		v += step;
	}
	sum *= len * SCALE_L;
	float c = dot(dir, -l);
	float cc = c * c;
	return sum * ( K_R * C_R * rayleighPhase( cc ) + K_M * miePhase( G_M, c, cc ) ) * E;
}

void main(void) {	
	vec3 camPosition = vec3(0.0, 0.0, -256.0);
	vec3 dir = rayDirection(camPosition);
	vec3 eye = camPosition;

	vec3 loc = vec3(uPointLightingLocation.xy, -uPointLightingLocation.z);
	vec3 l = normalize(loc);
	
	vec2 e = rayIntersection(eye, dir, fOuterRadius);
	vec2 f = rayIntersection(eye, dir, fInnerRadius);
	e.y = min(e.y, f.x);
	
	vec3 I = inScatter(eye, dir, e, l);
	
	gl_FragColor = vec4(I, 1.0);
}
#version 330 core

struct Material {
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
    float shininess;
};

uniform Material material;
uniform vec3 cameraPosWorld;

uniform vec3 lightPosWorld;
uniform vec3 lightColor;
uniform float lightIntensity;

uniform vec4 flashlightPosWorld;
uniform vec3 flashlightColor;
uniform float flashlightIntensity;
uniform vec3 flashlightDirWorld;

in vec3 normalWorld;
in vec3 fragmentWorld;

out vec4 outColor;

vec3 calculateAmbient() {
    return material.ambient * lightColor * 0.1;
}

vec3 calculateDirectionalLight(vec3 norm, vec3 viewDir) {
    vec3 lightDir = normalize(lightPosWorld - fragmentWorld);
    float diffFactor = max(dot(norm, lightDir), 0.05);
    vec3 diffuse = material.diffuse * diffFactor * lightColor * lightIntensity;
    
    vec3 halfwayDir = normalize(lightDir + viewDir);
    float specFactor = pow(max(dot(norm, halfwayDir), 0.0), material.shininess * 0.5);
    vec3 specular = material.specular * specFactor * lightColor * lightIntensity * 0.3;
    
    return diffuse + specular;
}

vec3 calculateFlashlight(vec3 norm, vec3 viewDir) {
    vec3 flashlightToFrag = fragmentWorld - flashlightPosWorld.xyz;
    float distance = length(flashlightToFrag);
    flashlightToFrag = normalize(flashlightToFrag);
    
    float theta = dot(-flashlightToFrag, normalize(flashlightDirWorld));

    float cutOff = cos(radians(20.0));
    float outerCutOff = cos(radians(30.0));
    float intensity = smoothstep(outerCutOff, cutOff, theta) * 0.8;

    // Attenuation based on distance
    float maxRange = 15.0; // Max range for the flashlight
    float attenuation = 2.0 / (1.0 + 0.09 * distance + 0.032 * (distance * distance));
    intensity *= attenuation;

    float flashDiff = max(dot(norm, -flashlightToFrag), 0.05);
    vec3 flashlightDiffuse = material.diffuse * flashDiff * flashlightColor * flashlightIntensity * intensity;

    vec3 flashHalfwayDir = normalize(-flashlightToFrag + viewDir);
    float flashSpecFactor = pow(max(dot(norm, flashHalfwayDir), 0.0), material.shininess * 0.5);
    vec3 flashlightSpecular = material.specular * flashSpecFactor * flashlightColor * flashlightIntensity * intensity * 0.3;

    return flashlightDiffuse + flashlightSpecular;
}

void main() {
    vec3 norm = normalize(normalWorld);
    vec3 viewDir = normalize(cameraPosWorld - fragmentWorld);
    
    vec3 ambient = calculateAmbient();
    vec3 directional = calculateDirectionalLight(norm, viewDir);
    vec3 flashlight = calculateFlashlight(norm, viewDir);
    
    vec3 finalColor = ambient + directional + flashlight;
    outColor = vec4(finalColor, 1.0);
}

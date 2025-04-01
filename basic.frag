#version 330 core

struct Material {
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
    float shininess;
};

uniform Material material;
uniform vec3 cameraPosWorld;

uniform vec4 lightPosWorld;
uniform vec3 lightColor;
uniform float lightIntensity;

uniform vec4 flashlightPosWorld;
uniform vec3 flashlightColor;
uniform float flashlightIntensity;
uniform vec3 flashlightDirWorld;

in vec3 normalWorld;
in vec3 fragmentWorld;

out vec4 outColor;

void main() {
    // === Ambient Light ===
    vec3 ambient = material.ambient * lightColor * 0.2; // Increased ambient to avoid total black areas

    // Normalize the normal vector
    vec3 norm = normalize(normalWorld);

    // === Directional Light Calculation ===
    vec3 lightDir = normalize(lightPosWorld.xyz - fragmentWorld); 
    float diffFactor = max(dot(norm, lightDir), 0.05); // Added min value to avoid total black spots
    vec3 diffuse = material.diffuse * diffFactor * lightColor * lightIntensity;

    // === Improved Specular Lighting ===
    vec3 viewDir = normalize(cameraPosWorld - fragmentWorld);
    vec3 halfwayDir = normalize(lightDir + viewDir); // Better specular calculation
    float specFactor = pow(max(dot(norm, halfwayDir), 0.0), material.shininess * 0.5); // Reduced shininess effect
    vec3 specular = material.specular * specFactor * lightColor * lightIntensity * 0.3; // Reduced intensity

    // === Flashlight Calculation (Soft Falloff) ===
    vec3 flashlightToFrag = normalize(fragmentWorld - flashlightPosWorld.xyz);
    float theta = dot(-flashlightToFrag, normalize(flashlightDirWorld));

    float cutOff = cos(radians(25.0));  // Wider inner cone
    float outerCutOff = cos(radians(35.0)); // Softer transition
    float intensity = smoothstep(outerCutOff, cutOff, theta) * 0.8; // Reduced strength for softer effect

    // Flashlight Diffuse Light
    float flashDiff = max(dot(norm, -flashlightToFrag), 0.05); // Added min value
    vec3 flashlightDiffuse = material.diffuse * flashDiff * flashlightColor * flashlightIntensity * intensity;

    // Flashlight Specular Light (Fixed)
    vec3 flashHalfwayDir = normalize(-flashlightToFrag + viewDir);
    float flashSpecFactor = pow(max(dot(norm, flashHalfwayDir), 0.0), material.shininess * 0.5);
    vec3 flashlightSpecular = material.specular * flashSpecFactor * flashlightColor * flashlightIntensity * intensity * 0.3; // Reduced intensity

    // === Final Color Output ===
    vec3 finalColor = ambient + diffuse + specular + flashlightDiffuse + flashlightSpecular;
    outColor = vec4(finalColor, 1.0);
}

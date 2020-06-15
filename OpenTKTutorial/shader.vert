#version 450 core

//These are vars that take in data from the vertex data buffer
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;

//Uniforms are vars that can dynamically take in data
//from the CPU side at will
layout(location = 3) uniform vec4 u_TintColor;
layout(location = 4) uniform mat4 u_Transform;


//These are vars that send data out and can be
//used as input into the fragment shader
out vec2 v_TexCoord;
out vec4 v_TintClr;
out float v_TextureIndex;

void main()
{
    v_TintClr = u_TintColor;
    v_TexCoord = aTexCoord;


    gl_Position = vec4(aPosition, 1.0) * u_Transform;
}
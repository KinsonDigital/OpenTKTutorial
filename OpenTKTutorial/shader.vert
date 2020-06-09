#version 450 core

//These are vars that take in data from the vertex data buffer
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;
layout(location = 2) in vec4 aTintColor;
layout(location = 3) in float aTextureIndex;

//Uniforms are vars that can dynamically take in data
//from the CPU side at will
layout(location = 4) uniform mat4 u_Transforms[2];//$TOTAL_TEXTURE_SLOTS


//These are vars that send data out and can be
//used as input into the fragment shader
out vec2 v_TexCoord;
out vec4 v_TintClr;
out float v_TextureIndex;

void main()
{
    int index = int(aTextureIndex);

    v_TintClr = aTintColor;
    v_TexCoord = aTexCoord;
    v_TextureIndex = aTextureIndex;


    gl_Position = vec4(aPosition, 1.0) * u_Transforms[index];
}
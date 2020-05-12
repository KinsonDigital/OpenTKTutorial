#version 330 core
layout (location = 0) in vec3 aPosition;

void main()
{
    //Set the built in gl_Position variable to the value of the aPosition variable
    //This is the position coming into the shader
    gl_Position = vec4(aPosition, 1.0);
}
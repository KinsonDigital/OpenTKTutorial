/*Info:
This is an example of alpha blending.

Src: https://stackoverflow.com/questions/746899/how-to-calculate-an-rgb-colour-by-specifying-an-alpha-blending-amount
*/
#version 450 core

in vec2 v_TexCoord;
in vec4 v_TintClr;
in float v_TextureIndex;

out vec4 o_OutputColor;


uniform sampler2D textures[3];//$TOTAL_TEXTURE_SLOTS


void main ()
{
	int index = int(v_TextureIndex);

	o_OutputColor = texture(textures[index], v_TexCoord) * v_TintClr;
}
//	Credit: Minions Art for this HLSL file as part of her Lit Toon Shader tutorial
//	comments added for note taking purposes

//	function name _float -> tells node which percision to use
void ToonShading_float(
	//	inputs
	in float3 Normal,								//	normal direction of surface
	in float ToonRampSmoothness,					//	softness of transition
	in float3 ClipSpacePos,							//	object space
	in float3 WorldPos,								//	world space
	in float4 ToonRampTinting,						//	toon shadow color
	in float ToonRampOffset,						//	controlls the cutoff
	//	outputs
	out float3 ToonRampOutput,						//	output of the toonramp shader!
	out float3 Direction)							//	light direction 
{

	// set the shader graph node previews
	//		insures we have a preview in the shadergraph
	//		must assign all out values for this to happen!
	#ifdef SHADERGRAPH_PREVIEW
		ToonRampOutput = float3(0.5,0.5,0);
		Direction = float3(0.5,0.5,0);
	#else

		// grab the shadow coordinates
		//	are we using screenspace shadows?
		#if SHADOWS_SCREEN
			//	if so, use clipspaceposition input
			half4 shadowCoord = ComputeScreenPos(ClipSpacePos);
		#else
			//	otherwise, we use the world position input
			half4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
		#endif 

		// grab the main light
			//	Directional light
			//	are shadows enabled?
		#if _MAIN_LIGHT_SHADOWS_CASCADE || _MAIN_LIGHT_SHADOWS
			//	if so, grab shadow coordinates
			Light light = GetMainLight(shadowCoord);
		#else
			//	otherwise, we don't need them
			Light light = GetMainLight();
		#endif

		// dot product for toonramp
			//	dot product of the Normal (passed in) and the light direction
			//	divided by two (multiplied by 0.5) offset added to move toonramp over whole mesh
		half d = dot(Normal, light.direction) * 0.5 + 0.5;
		
		// toonramp in a smoothstep
			//	creates a cutoff effect over the dot product
		half toonRamp = smoothstep(ToonRampOffset, ToonRampOffset+ ToonRampSmoothness, d );
		// multiply with shadows;
		toonRamp *= light.shadowAttenuation;
		// add in lights and extra tinting
		ToonRampOutput = light.color * (toonRamp + ToonRampTinting) ;
		// output direction for rimlight
		Direction = light.direction;
	#endif

}
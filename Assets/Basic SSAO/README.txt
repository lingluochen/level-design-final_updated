Thank you for buying my asset!
If you have any question please send me an email piotrtplaystore@gmail.com

Setup for URP:
-Delete folder called LWRP if it exists (depends on what Unity version you downloaded my package)
-Unpack file called URP.zip

-Make sure depth texture is rendered, check options in your project Settings folder you can find there files called like UniversalRP-High, Low, Medium
in every of them select option called Depth Texture. Also if you have option somewhere to select depth texture bits amount select the biggest value for best SSAO
accuracy

-Create Basic SSAO settings file by clicking right mouse button in assets view then Create -> Basic SSAO -> BasicSSAOSettings, here is the file where you can find all settings you
need :)

-Select created SSAO Settings and in Random Texture field select texture called BasicSSAORandomTexture

-Now go to your Forward Renderer asset, you can probably find it in your Settings folder, click it and in Renderer Features section press plus button and select "SSAO Render feature"

-Now expand your Forward Renderer asset content by clicking Arrow button on right side of it's icon in Assets View, select NewSSAORenderFeature and assign assets:
	-Settings -> Select there your settings you just created or select example settings
	-SSAO Material -> Select material called BasicSSAOMaterial
	-SSAO Blur Material -> Select material called BasicSSAOBlur
	-SSAO Combine Material -> Select material called BasicSSAOCombineMaterial

-Everything now should work! 



Setup for LWRP:  
-Unpack file called LWRP.zip if it is not unpacked allready (depends on what Unity verions you downloaded my package)

-Make sure depth texture is rendered, check options in your project Settings folder you can find there files called like LWRP-HighQuality, Low, Medium
in every of them select option called Depth Texture. Also if you have option somewhere to select depth texture bits amount select the biggest value for best SSAO
accuracy

-Create Basic SSAO settings file by clicking right mouse button in assets view then Create -> Basic SSAO -> BasicSSAOSettings, here is the file where you can find all settings you
need :)

-Select created SSAO Settings and in Random Texture field select texture called BasicSSAORandomTexture

-Create Forward Renderer asset by clicking right mouse button in Asset View then Rendering -> Lightweight Render Pipeline -> Forward Renderer

-Now select you newly created Forward Renderer asset, click it and in Renderer Features section press plus button and select "LWRPSSAO Render feature"

-Now expand your Forward Renderer asset content by clicking Arrow button on right side of it's icon in Assets View, select NewLWRPSSAORenderFeature and assign assets:
	-Settings -> Select there your settings you just created or select example settings
	-SSAO Material -> Select material called BasicSSAOMaterial
	-SSAO Blur Material -> Select material called BasicSSAOBlur
	-SSAO Combine Material -> Select material called BasicSSAOCombineMaterial
	
-Now go to your scene camera and in Renderer Type field select Custom and select renderer you just created 

-Now everything should work :)!

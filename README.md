# Extract-Capturing-Setup-Features
A set of algorithms for extracting of capturing setup features from Structure from Motion reconstruction setups. The prototype requires input data from the SfM software - the reconstructed mesh, the camera positions, orientation, as well as extracted 2D feature points.

The prototype was used to extract features for this dataset - [GGG - Rough or Noisy? Metrics for Noise Detection in SfM Reconstructions](https://data.mendeley.com/datasets/xtv5y29xvz/1)

In it's current form the Unity application is not documented and quite hard to get running. To change the used object and camera setup, you will need to modify the Manager script inputs, as well as the datapaths present in the "LoadFilesForRaycasting" script. You will need to also know the used camera sensor height, focal length, image dimensions and aperture. In addition, if the object is not to real world scale, the input Real Scale needs to be set to the proper scaling factor. 

Four main features can be extracted from the prototype, the current build makes the extraction semi-manual, where the user needs to click for the extraction of the features from each camera. This is done for debugging and visualization purposes, with code automation being possible:

1. 2D image feature reprojections - by clicking the Space bar the extracted 2D features are reprojected to the reconstructed mesh. Saving the features is done with key 4\
2. In/Out of focus mesh parts - by clicking the V button the focus planes for each camera are calculated and the parts of the reconstructed object outside and inside of these planes are marked. Saving the features is done with key 3
3. Number of images seeing each 3d object part - by clicking the B button for each vertex on the 3D mesh the number of cameras seeing it is calculated. Saving the features is done with key 1
4. Angle between the camera forward direction and the normal of each vertex - by clicking the N button for each vertex the angle between its normal direction and the forward direction of the camera is calculated. Saving the features is done with key 2


The application requires Unity 2019.3.7f1 or higher to run.


![Gameplay Gif](GameImages/CapturingSetupFeatures.gif)

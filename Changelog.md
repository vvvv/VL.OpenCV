# Changelog
All notable changes to the VL.OpenCV repository will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

# [3.1.0-preview] - 17/05/2025

## Added

* Camera clibration tool example help patch in Calibration category
* AR Water molecule help patch in Detection category
* Sample calibration file for 1280x720 resolution
* .Utils.Advanced.FromInternalMat node
* .Utils.Advanced.GetInternalMat node

## Changed 

* VideoIn now has a 'Default' enum entry which points to the first camera detected
* Help patches have been reorganized
* VideoIn and related nodes have been refactored
	* Removed Reactive version (breaking change)
	* Merged blocking and non blocking versions (breaking change)
	* Refactored internal nodes
	* VideoIn now uses DSHOW as the default backend
	* VideoIn Index no longer reports supported formats since they are backend dependent
	* Input and output pins have been renamed to match other video input libraries (breaking change)
	* Default Preferred Size is now 1280x720
* VideoPlayer (Blocking) was refactored and renamed to VideoReader (breaking change)
* Resize (Width Height) was renamed to Resize (breaking change)
* All nodes that had separate integer Width and Height or similar inputs and outputs have been refactored to use Int2 instead, this affect the following nodes:
	.Calibration
		BoardCorners (breaking change)
		FindChessboardCornersSB (breaking change)
		FindChessboardCorners (breaking change)
		CalibrateProjector (breaking change)
		CalibrateCamera (breaking change)
		CalibrationMatrixValues (breaking change)
	.Drawing
		DrawChessboardCorners (breaking change)
	.Filter
		Blur (breaking change)
		GaussianBlur (breaking change)
		Resize (breaking change)
	.Sink
		VideoWriter (Append) (breaking change)
		VideoWriter (breaking change)
	.Transform
		PixelToNormalizedSpace (breaking change)
		Perspective (RH) (breaking change)
		IntrinsicsToProjectionMatrix (breaking change)
	.Source
		VideoIn (breaking change)
		CvImage (breaking change)
		CvImage (RGBA) (breaking change)
		CvImage (Array) (breaking change)
	.Utils
		Info (breaking change)
* All help patches using any of the affected nodes above have been updated to match the new node signatures
* OpenPoseDetector, YOLODetector and YOLO3Detector nodes have been marked Obsolete, MediaPipe is the way
* Tooltips now show default CvImages
* VideoIn device listing and SupportedVideoFormats cleaned up and improved, all distinct video input devices for both DSHOW and MediaFoundation will now appear on the device selection enum
* Expose ConvertColor's Cahce region's Force pin as optional for odd cases where the Cache region fails to detect changes (Spout source)
* All help patches that require external resources (OpenPose, YOLO, etc) now have helper nodes to automagically download the necessary resources
* The following nodes in .Utils.Advanced have been moved to .Utils.Internal:
	* EnforceGrayDefault (breaking change)
	* IsDamon (breaking change)
	* CvImage forward (breaking change)
	* PooledQueue (breaking change)
	* Clone (breaking change)
	* Image (breaking change)
	* Image (Rows Cols Format) (breaking change)
	* ImageQueue (breaking change)
	* CopyToAndReturn (breaking change)
	* GetMatTypeFromEnum (breaking change)
	* GetPixelAsFloats (breaking change)
	* MatSerializer (breaking change)
	* CvImageSerializer (breaking change)
* .Source ImageReader (Reactive) moved to .Source.Advanced (breaking change)
* VideoPlayer and VideoPlayer (Experimental) marked obsolete
* The following nodes in .Conversion were moved to .Conversion.Advanced:
	* ToRGBA (BGR) (breaking change)
	* ToRGBA (RGB) (breaking change)
	* ToRGBA (BGRA) (breaking change)
	* FromChannelsToColumns (breaking change)
	* ToValues (Custom) (breaking change)


## Fixed

* Renderer no longer interpolates small images producing gradient artifacts
* Renderer can now more accurately scale down and represent small resolution images
* All calibration related help patches are now using FindChessboardCorners instead of FindChessboardCornersSB due to a bug that reverts the order of the detected corners
* FindChessboardCorners no longer throws exception when receiving default values
* FrameDelay no longer throws exception when receiving default values
* FrameDifference no longer throws exception when receiving default values
* OpticalFlow no longer throws exception when receiving default values


### Removed

* Reference Drawing nodes help patch
* Reference Available filters help patch
* VideoIn (Reactive) (breaking change)
* SupportedVideoFormats (Index) (breaking change)
* Link to vvvTV OpenCV episode

# [3.0.3] - 18/11/2024

## Changed 
* Remove unused files

# [3.0.2] - 18/11/2024

## Fixed 
* Nuget icon issue

# [3.0.1] - 18/11/2024

## Fixed
* Restored missing VL.OpenCV.dll file
* Fixed corrupt nuget icon file
* Fix path in exported dll

# [3.0.0] - 12/11/2024

## Changed
* Upgraded project to .net 8

## Removed
* All vvvv beta related files and dependencies

# [2.6.4] - 26/10/2024

## Added
* MorphologyEx filter node
* Bilateral filter node

## Changed
* Exposed Kernel Shape, Kernel Size and Border Type in Erode and Dilate filters
* Exposed Convert RGB pin in all VideoIn variants (including Index ones)

# [2.6.3] - 05/10/2024

## Added
* HowTo Recognize Faces help patch (thanks untone)

# [2.6.2] - 16/08/2024

## Added
* FrameDifferenc(FilterEquals) node and help patch
* Yolo3Detector with non-maximum suppression and help patch
* Face Detector with Caffe Model and help patch
* Added S+H (OpenCV) node and help patch

Big thanks to dani217s for these additions!

# [2.6.1] - 23/04/2024

## Changed
* Help patches were cleaned up and reference links updated

## Fixed
* Aruco MarkerWriter
* DrawArucoMarker
* MarkerDetector
* MarkerDiamondDetector
* Ridge filter

# [2.6.0] - 16/04/2024

## Added
* OCR Text Detection node (thanks sebl)
* HowTo Detect text using OCR help patch (thanks sebl)

## Changed
* OpenCVSharp updated from version 4.5.5.20211231 to 4.9.0.20240103
* Improved DrawChessboard node's performance
* Exposed Dicitonary IOBox in Tutorial Calculate a camera position using Aruco to prevent mismatch issues
* Removed modified VideoIn input pin from Tutorial Calculate a camera position using SolvePnP
* Teamcity's nuget source now uses https

# [2.5.4] - 10/04/2024

## Added
* HowTo Draw OpenCV in Skia help patch (thanks untone)

## Changed
* Updated Contributing section in README.md

## Fixed
* Serialization exception in VideoWriter

# [2.5.0] - 25/05/2022

## Added
* Add OpenCvSharp4.Extensions reference

## Changed
* Improve Renderer's ImageID calculation performance and reliability
* Update SharpDX.MediaFoundation version from 4.0.1 to 4.2.0
* Update OpenCvSharp4 references from 4.5.3.20210725 to 4.5.5.20211231
* Update VL.Core reference from 2020.2.1 to 2021.4.8
* Replace nodes marked as obsolete
* Help patch cleanup and obsolete node removal

# [2.4.0] - 18/04/2022

## Added
* Ridge filter
* MarkerDiamondDetector (Experimental)

## Changed
* OpenCVSharp updated from version 4.5.3.20210725 to 4.5.5.20211231
* Updated "HowTo Estimate the pose of Aruco markers"

## Fixed
* Renderer now properly calculates imageID for square images (again)
* Fixed IntrinsicsReader to not return bogus results if file was not read
* Fixed Undistort node not to crash in case it receives a 0x0 mat as Intrinsics input
* Fixed for help-patch removing obsolete pin on Camera node

# [2.3.0] - 07/04/2022

## Added
* Renderer "Show Cursor" pin

## Fixed
* Renderer now properly calculates imageID for square images
* Apply pin in (Scalar) fitler nodes now work as expected

# [2.2.0] - 07/04/2022

## Added
* Thinning node to achieve a skeletization of the input image

## Changed
* QRCodeDetector can now detect multiple QR codes

## Fixed
* EstimatePose now checks for valid Intrinsics input

# [2.1.0] - 10/08/2021

## Added
* VideoIn "Video Acceleration" and "Hardware Device Index" input pins
* Facemark detector (thanks sebescudie!)
* CvImage type help patch
* Create an image from scratch help patch
* Intrisics help patch

## Changed
* Update OpenCVSharp dependency to version 4.5.3.20210725
* General cleanup

## Fixed
* Fix Resize (Scale) final dimensions smaller than 1x1 bug

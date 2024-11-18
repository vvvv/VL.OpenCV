# Changelog
All notable changes to the VL.OpenCV repository will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

# [3.0.1] - 18/11/2024

## Fixed
* Restored missing VL.OpenCV.dll file
* Fixed corrupt nuget icon file

## Removed
* All vvvv beta related files and dependencies

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
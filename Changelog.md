# Changelog
All notable changes to the VL.OpenCV repository will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

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